using DocSearch.hubs;
using DocSearch.Resources;
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
        /// <param name="connectionID"></param>
        public static void SendHistory(string historyKind, string connectionID)
        {
            if (historyKind == Constants.HISTORY_KIND_CRAWL)
            {
                ComHub.SendMessageToTargetClient(Constants.TYPE_HISTORY, historyKind, History.CrawlHistory.HistoryDataArray, connectionID);
            }
            else if (historyKind == Constants.HISTORY_KIND_WORD2VEC)
            {
                ComHub.SendMessageToTargetClient(Constants.TYPE_HISTORY, historyKind, History.Word2VecHistory.HistoryDataArray, connectionID);
            }
        }
    }
}