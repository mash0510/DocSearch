﻿using DocSearch.Models;
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

namespace DocSearch.Controllers
{
    public class DocSearchController : Controller
    {
        /// <summary>
        /// 複数の検索キーワードの区切り文字
        /// </summary>
        private char[] delimiter = { ' ', '　' };

        /// <summary>
        /// 単語間の類似性を保持するインスタンス
        /// </summary>
        Word2Vec.Net.Distance distance = null;

        private const int LETTERS_AROUND_KEYWORD = 50;

        // GET: DocSearch
        [HttpGet]
        public ActionResult Index(DocSearchModel docSearchModel)
        {
            // 検索キーワードが何も入力されていなかったら、検索処理はしない。
            if (docSearchModel.InputKeywords == null ||
                docSearchModel.InputKeywords == string.Empty)
                return View();

            Search(docSearchModel);

            return View(docSearchModel);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="docSearchModel"></param>
        /// <returns></returns>
        private void Search(DocSearchModel docSearchModel)
        {
            string[] keywords = docSearchModel.InputKeywords.Split(delimiter);
            docSearchModel.RelatedWords = GetRelatedWords(keywords);

            // 入力されたキーワードをListに保持する
            foreach (string keyword in docSearchModel.RelatedWords.Keys)
            {
                if (docSearchModel.InputKeywordList == null)
                    docSearchModel.InputKeywordList = new List<string>();
                
                docSearchModel.InputKeywordList.Add(keyword);
            }

            SearchEngineConnection.InitConnectClient();
            ElasticClient client = SearchEngineConnection.Client;

            //【TODO】ページ処理を実装する必要がある。
            // OR検索などは優先度低で良いか。
            var response = client.Search<DocumentInfo>(s => s
                .From(0)
                .Size(10)
                .Query(q => q.Match(ma => ma.Field(fld => fld.DocContent).Query(docSearchModel.InputKeywords).Operator(Operator.And)))
            );

            ConvertToDocSearchModel(response.Documents, docSearchModel, keywords);
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
            {
                docSearchModel.SearchedDocument = new List<DocData>();
            }
            else
            {
                docSearchModel.SearchedDocument.Clear();
            }

            foreach(DocumentInfo docInfo in documents)
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

            if (distance == null)
                distance = new Word2Vec.Net.Distance(CommonParameters.VectorFileNameFullPath);

            foreach (string keyword in keywords)
            {
                if (keyword == string.Empty || keyword == " " || keyword == "　")
                    continue;

                string trimedKeyword = keyword.Trim();

                BestWord[] bestwords = distance.Search(trimedKeyword);
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