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
        /// 検索先フォルダのセッションキー
        /// </summary>
        private const string SESSION_SEARCH_FOLDER = "searchFolder";
        /// <summary>
        /// 検索フォルダの設定と取得
        /// </summary>
        public string SearchFolder
        {
            set
            {
                Session[SESSION_SEARCH_FOLDER] = value;
            }
            get
            {
                if (Session == null || Session[SESSION_SEARCH_FOLDER] == null)
                    return string.Empty;

                string folder = Session[SESSION_SEARCH_FOLDER].ToString();

                return folder;
            }
        }


        /// <summary>
        /// サマリー表示の文字数。入力されたキーワードの前後何文字を検索画面中に表示するか。
        /// </summary>
        private const int LETTERS_AROUND_KEYWORD = 50;

        // GET: DocSearch
        [HttpGet]
        public ActionResult Index(DocSearchModel docSearchModel, int? page)
        {
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
        /// 検索結果をModelに変換
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="docSearchModel"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private void ConvertToDocSearchModel(IEnumerable<DocumentInfo> documents, DocSearchModel docSearchModel, string[] keywords)
        {
            if (docSearchModel.SearchedDocument == null)
                docSearchModel.SearchedDocument = new List<DocData>();
            else
                docSearchModel.SearchedDocument.Clear();

            foreach (DocumentInfo docInfo in documents)
            {
                DocData dispData = new DocData();
                dispData.FileName = docInfo.FileName;
                dispData.UpdatedDate = docInfo.UpdatedDate;
                dispData.FileFullPath = docInfo.FileFullPath;
                dispData.Extention = docInfo.Extention;
                dispData.DocSummary = GetTextAroundKeyword(docInfo.DocContent, keywords, LETTERS_AROUND_KEYWORD);

                docSearchModel.SearchedDocument.Add(dispData);
            }
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
                    retval.Append("........");
            }

            return retval.ToString();
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

            char[] delimiter = new char[] { ',' };

            List<string> rootList = rootFolders.Select(folder => Server.UrlDecode(folder).Replace("/", "\\")).ToList<string>();

            dir = Server.UrlDecode(dir);
            List<string> dirList = dir.Replace("/", "\\").Split(delimiter).ToList<string>();

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