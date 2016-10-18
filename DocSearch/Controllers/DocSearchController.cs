using DocSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Nest;
using FolderCrawler;
using Word2Vec.Net;
using System.Text;
using DocSearch.CommonLogic;
using System.IO;
using DocSearch.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocSearch.Controllers
{
    public class DocSearchController : Controller
    {
        /// <summary>
        /// 複数の検索キーワードの区切り文字
        /// </summary>
        private char[] _keywordDelimiter = { ' ', '　' };

        /// <summary>
        /// ツリー表示のフォルダの区切り文字
        /// </summary>
        private char[] _folderDelimiter = { ',' };

        /// <summary>
        /// 単語間の類似性を保持するインスタンス
        /// </summary>
        Word2Vec.Net.Distance _distance = null;

        /// <summary>
        /// ページ表示
        /// </summary>
        private Pagination _pagination = new Pagination();

        /// <summary>
        /// 検索フォルダの設定と取得
        /// </summary>
        public string SearchFolder
        {
            set
            {
                Session[Constants.SESSION_SEARCH_FOLDER] = value;
            }
            get
            {
                if (Session == null || Session[Constants.SESSION_SEARCH_FOLDER] == null)
                    return string.Empty;

                string folder = Session[Constants.SESSION_SEARCH_FOLDER].ToString();

                return folder;
            }
        }

        /// <summary>
        /// 類似文書検索の検索元文書IDの設定と取得
        /// </summary>
        private string SimilarDocSearchID
        {
            set
            {
                Session[Constants.SIMILER_DOCSEARCH_ID] = value;
            }
            get
            {
                if (Session == null || Session[Constants.SIMILER_DOCSEARCH_ID] == null)
                    return string.Empty;

                string docID = Session[Constants.SIMILER_DOCSEARCH_ID].ToString();

                return docID;
            }
        }

        // GET: DocSearch
        [HttpGet]
        public ActionResult Index(DocSearchModel docSearchModel, int? page, bool? searchSimilarDoc)
        {
            if (searchSimilarDoc != null && searchSimilarDoc == true)
            {
                ActionResult ar = SameKindDoc(docSearchModel, SimilarDocSearchID, page);
                return ar;
            }
            else
            {
                docSearchModel.IsSimilarDocSearch = false;
            }

            // 検索キーワードが何も入力されていなかったら、検索処理はしない。
            if (docSearchModel.InputKeywords == null ||
                docSearchModel.InputKeywords == string.Empty)
            {
                docSearchModel.SearchExecuted = false;
                return View(docSearchModel);
            }

            docSearchModel.SearchExecuted = true;

            int pageNo = page ?? 1;
            if (pageNo <= 0) pageNo = 1;

            Search(docSearchModel, pageNo);

            docSearchModel.SearchFolder = SearchFolder;

            return View(docSearchModel);
        }

        // GET: DocSearch
        [HttpGet]
        public ActionResult SameKindDoc(DocSearchModel docSearchModel, string docID, int? page)
        {
            docSearchModel.SearchExecuted = true;

            SimilarDocSearchID = docID;

            int pageNo = page ?? 1;
            if (pageNo <= 0) pageNo = 1;

            SearchSameKindDoc(docSearchModel, docID, pageNo);

            docSearchModel.SearchFolder = SearchFolder;
            docSearchModel.IsSimilarDocSearch = true;

            return View("Index", docSearchModel);
        }

        /// <summary>
        /// 検索先のフォルダの設定
        /// </summary>
        /// <param name="selectedFolder"></param>
        [HttpPost]
        public void SetSearchFolder(string selectedFolder)
        {
            string searchFolder = Server.UrlDecode(selectedFolder).Replace("//", "/").Replace("/", "\\").TrimEnd(new char[] { '\\' });

            SearchFolder = searchFolder;
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="docSearchModel"></param>
        /// <returns></returns>
        private void Search(DocSearchModel docSearchModel, int page)
        {
            string[] keywords = docSearchModel.InputKeywords.Split(_keywordDelimiter);
            docSearchModel.RelatedWords = GetRelatedWords(keywords);
            ListConvert(docSearchModel);

            SearchEngineConnection.InitConnectClient();
            ElasticClient client = SearchEngineConnection.Client;

            _pagination.PageSize = docSearchModel.PageSize;
            Pagination.DataRange dataRange = _pagination.GetDataRange(page);

            ISearchResponse<DocumentInfo> response = null;
            string searchFolder = SearchFolder;

            if (searchFolder != string.Empty)
            {
                response = client.Search<DocumentInfo>(s => s
                    .From(dataRange.Start)
                    .Size(_pagination.PageSize)
                    .Query(q => q
                        .Bool(b => b
                            .Must(must => must.Match(match => match.Field(fld => fld.DocContent).Query(docSearchModel.InputKeywords).Operator(Operator.And)))
                            .Filter(Filter => Filter.Prefix(pre => pre.Field(fld => fld.FolderPath).Value(searchFolder)))))
                );
            }
            else
            {
                response = client.Search<DocumentInfo>(s => s
                    .From(dataRange.Start)
                    .Size(_pagination.PageSize)
                    .Query(q => q.Match(ma => ma.Field(fld => fld.DocContent).Query(docSearchModel.InputKeywords).Operator(Operator.And)))
                );
            }


            _pagination.TotalDataNum = (int)response.Total - 1; // 1つ大きい値が入るので -1 する。

            docSearchModel.Total = _pagination.TotalDataNum;
            docSearchModel.PageList = _pagination.GetPageList();
            docSearchModel.Page = page;

            ConvertToDocSearchModel(response.Documents, docSearchModel, keywords);
        }

        /// <summary>
        /// 入力されたキーワードをList化する。
        /// </summary>
        /// <param name="docSearchModel"></param>
        private void ListConvert(DocSearchModel docSearchModel)
        {
            if (docSearchModel.InputKeywordList == null)
                docSearchModel.InputKeywordList = new List<string>();
            else
                docSearchModel.InputKeywordList.Clear();

            foreach (string keyword in docSearchModel.RelatedWords.Keys)
            {
                docSearchModel.InputKeywordList.Add(keyword);
            }
        }

        /// <summary>
        /// 検索文書の初期化
        /// </summary>
        /// <param name="docSearchModel"></param>
        private void InitSearchedDocument(DocSearchModel docSearchModel)
        {
            if (docSearchModel.SearchedDocument == null)
                docSearchModel.SearchedDocument = new List<DocData>();
            else
                docSearchModel.SearchedDocument.Clear();
        }

        /// <summary>
        /// 検索結果をModelに変換
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="docSearchModel"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private void ConvertToDocSearchModel(IEnumerable<DocumentInfo> documents, DocSearchModel docSearchModel, string[] keywords)
        {
            InitSearchedDocument(docSearchModel);

            foreach (DocumentInfo docInfo in documents)
            {
                DocData dispData = new DocData();
                dispData.Score = Convert.ToDouble(Constants.NO_SCORE);
                dispData.DocID = IDDictionary.GetInstanse().GetElasticsearchID(docInfo.FileFullPath);
                dispData.FileName = docInfo.FileName;
                dispData.UpdatedDate = docInfo.UpdatedDate;
                dispData.FileFullPath = docInfo.FileFullPath;
                dispData.Extention = docInfo.Extention;
                dispData.DocSummary = GetTextAroundKeyword(docInfo.DocContent, keywords, ReadSettings.LettersAroundKeyword);

                docSearchModel.SearchedDocument.Add(dispData);
            }
        }

        /// <summary>
        /// 類似文書の検索
        /// </summary>
        /// <param name="docSearchModel"></param>
        /// <param name="docID"></param>
        private void SearchSameKindDoc(DocSearchModel docSearchModel, string docID, int page)
        {
            string[] keywords = docSearchModel.InputKeywords.Split(_keywordDelimiter);
            docSearchModel.RelatedWords = GetRelatedWords(keywords);
            ListConvert(docSearchModel);

            SearchEngineConnection.InitConnectClient();
            ElasticClient client = SearchEngineConnection.Client;

            _pagination.PageSize = docSearchModel.PageSize;
            Pagination.DataRange dataRange = _pagination.GetDataRange(page);

            // LINQ形式のmore_like_this検索では、検索結果が違った。正しくない検索結果が返される。
            // なので、ここではJSON形式のmore_like_thisクエリを、LowLevelクラスで送信して結果を得るようにする。
            string json = @"{""from"":" + dataRange.Start + @", ""size"":" + _pagination.PageSize + @", ""query"":{""more_like_this"":{""like"":[{""_id"": """ +  docID + @""" }]}}}";

            var res = client.LowLevel.Search<string>("docinfoindex", "documentinfo", json);
            JToken resJson = JsonConvert.DeserializeObject(res.Body) as JToken;

            InitSearchedDocument(docSearchModel);

            if (resJson == null || 
                resJson.SelectToken("hits.total") == null ||
                resJson.SelectToken("hits.hits") == null)
                return;

            int total = resJson.SelectToken("hits.total").Value<int>();

            foreach (JObject obj in resJson.SelectToken("hits.hits"))
            {
                double score = obj.GetValue(Constants.JSON_RES_SCORE).Value<double>();
                string fileName = obj.Last.Last[Constants.JSON_RES_FILENAME].Value<string>();
                string updatedDate = obj.Last.Last[Constants.JSON_RES_UPDATED_DATE].Value<string>();
                string fileFullPath = obj.Last.Last[Constants.JSON_RES_FILE_FULL_PATH].Value<string>();
                string extention = obj.Last.Last[Constants.JSON_RES_EXTENTION].Value<string>();
                string docContent = obj.Last.Last[Constants.JSON_RES_DOC_CONTENT].Value<string>();

                DocData dispData = new DocData();
                dispData.Score = score;
                dispData.DocID = IDDictionary.GetInstanse().GetElasticsearchID(fileFullPath);
                dispData.FileName = fileName;
                dispData.UpdatedDate = FolderCrawler.CommonLogic.SafeConvertDateTime(updatedDate);
                dispData.FileFullPath = fileFullPath;
                dispData.Extention = extention;
                dispData.DocSummary = GetTextFromHead(docContent, ReadSettings.LettersFromHead);

                docSearchModel.SearchedDocument.Add(dispData);
            }

            _pagination.TotalDataNum = total;

            docSearchModel.Total = _pagination.TotalDataNum;
            docSearchModel.PageList = _pagination.GetPageList();
            docSearchModel.Page = page;
        }

        /// <summary>
        /// キーワード周辺の文書データを取得
        /// </summary>
        /// <param name="allDocData"></param>
        /// <param name="keywords"></param>
        /// <param name="numLettersAround"></param>
        /// <returns></returns>
        private string GetTextAroundKeyword(string allDocData, string[] keywords, int numLettersAround)
        {
            StringBuilder retval = new StringBuilder();

            string substr = string.Empty;

            for (int i = 0; i < keywords.Length; i++)
            {
                int keywordIndex = allDocData.IndexOf(keywords[i]);

                int startIndex = keywordIndex - numLettersAround;
                if (startIndex < 0)
                    startIndex = 0;

                int extractWords = numLettersAround * 2 + keywords[i].Length;
                if (startIndex + extractWords > allDocData.Length)
                {
                    substr = allDocData.Substring(startIndex);
                }
                else
                {
                    substr = allDocData.Substring(startIndex, extractWords);
                }

                retval.Append(substr);

                if (i < keywords.Length - 1)
                    retval.Append(Constants.SNIP);
            }

            return retval.ToString();
        }

        /// <summary>
        /// 画面に表示する文書内容の取得。先頭からWeb.Config中の"LettersFromHead"で指定した文字数分だけ取得
        /// </summary>
        /// <param name="docContent"></param>
        /// <param name="letters"></param>
        /// <returns></returns>
        private string GetTextFromHead(string docContent, int letters)
        {
            string retval = docContent.Substring(0, letters);

            return retval;
        }

        /// <summary>
        /// 入力された検索キーワードの関連語の取得
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private Dictionary<string, SortedList<float, string>> GetRelatedWords(string[] keywords)
        {
            Dictionary<string, SortedList<float, string>> retval = new Dictionary<string, SortedList<float, string>>();

            if (_distance == null)
                _distance = new Word2Vec.Net.Distance(CommonParameters.VectorFileNameInUseFullPath);

            foreach (string keyword in keywords)
            {
                if (keyword == string.Empty || keyword == " " || keyword == "　")
                    continue;

                string trimedKeyword = keyword.Trim();

                BestWord[] bestwords = _distance.Search(trimedKeyword);
                SortedList<float, string> relatedWordList = new SortedList<float, string>(new DescComparer<float>());

                foreach (BestWord bestword in bestwords)
                {
                    if (relatedWordList.ContainsKey(bestword.Distance))
                        continue;

                    if (keywords.Contains(bestword.Word))
                        continue;

                    relatedWordList.Add(bestword.Distance, bestword.Word);
                }

                retval.Add(trimedKeyword, relatedWordList);
            }

            return retval;
        }        

        /// <summary>
        /// フォルダのツリー表示画面で表示させるフォルダとファイルの一覧の取得
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult GetFiles(string dir, bool onlyFolders, bool onlyFiles, string[] rootFolders)
        {
            List<FileTreeViewModel> files = new List<FileTreeViewModel>();

            List<string> rootList = rootFolders.Select(folder => Server.UrlDecode(folder).Replace("/", "\\")).ToList<string>();

            dir = Server.UrlDecode(dir);
            List<string> dirList = dir.Replace("/", "\\").Split(_folderDelimiter).ToList<string>();

            foreach (string realDir in dirList)
            {
                DirectoryInfo di = new DirectoryInfo(realDir);

                // ルートノードと同じフォルダに対する操作に対しては、そのフォルダ配下の情報は返さないようにする。
                if (rootList.Contains(realDir))
                {
                    files.Add(new FileTreeViewModel() { Name = di.Name, Path = String.Format("{0}\\", realDir), IsDirectory = true });
                    continue;
                }

                if (!onlyFiles)
                {
                    foreach (DirectoryInfo dc in di.GetDirectories())
                    {
                        files.Add(new FileTreeViewModel() { Name = dc.Name, Path = String.Format("{0}\\{1}\\", realDir, dc.Name), IsDirectory = true });
                    }
                }

                if (!onlyFolders)
                {
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        files.Add(new FileTreeViewModel() { Name = fi.Name, Ext = fi.Extension.Substring(1).ToLower(), Path = realDir + fi.Name, IsDirectory = false });
                    }
                }
            }

            return PartialView(files);
        }
    }

    /// <summary>
    /// SortedListで降順ソートを行うクラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DescComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(y, x);
        }
    }
}