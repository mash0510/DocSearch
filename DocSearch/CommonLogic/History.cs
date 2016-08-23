using DocSearch.hubs;
using FolderCrawler.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// 履歴クラス
    /// </summary>
    public static class History
    {
        public static string TYPE = "HISTORY";
        public static string HISTORY_CRAWL = "Crawl";
        public static string HISTORY_WORD2VEC = "word2vec";

        private static HistoryCrawl _crawlHistory = new HistoryCrawl();
        private static HistoryWord2Vec _word2vecHistory = new HistoryWord2Vec();

        /// <summary>
        /// クロール履歴管理クラス
        /// </summary>
        public static HistoryCrawl CrawlHistory
        {
            get { return _crawlHistory; }
            set { _crawlHistory = value; }
        }

        /// <summary>
        /// word2vec履歴管理クラス
        /// </summary>
        public static HistoryWord2Vec Word2VecHistory
        {
            get { return _word2vecHistory; }
            set { _word2vecHistory = value; }
        }

        /// <summary>
        /// 履歴データのブラウザへの送信
        /// </summary>
        /// <param name="historyKind"></param>
        public static void SendHistory(string historyKind)
        {
            if (historyKind == HISTORY_CRAWL)
            {
                ComHub.SendMessage(TYPE, historyKind, History.CrawlHistory.HistoryDataArray);
            }
            else if (historyKind == HISTORY_WORD2VEC)
            {
                ComHub.SendMessage(TYPE, historyKind, History.Word2VecHistory.HistoryDataArray);
            }
        }
    }
}