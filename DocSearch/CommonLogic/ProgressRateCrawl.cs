using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// クロール処理の進捗率の取得
    /// </summary>
    public class ProgressRateCrawl : SendProgressRate
    {
        /// <summary>
        /// クロール処理の進捗率の取得
        /// </summary>
        /// <returns></returns>
        protected override int GetProgressRate()
        {
            int rate = CrawlerManager.GetInstance().InsertProgressRate;
            return rate;
        }
    }
}