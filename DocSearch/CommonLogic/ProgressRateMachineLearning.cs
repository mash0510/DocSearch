using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// 機械学習の進捗率の取得
    /// </summary>
    public class ProgressRateMachineLearning : SendProgressRate
    {
        /// <summary>
        /// 機械学習の進捗率の取得
        /// </summary>
        /// <returns></returns>
        protected override int GetProgressRate()
        {
            int rate = TrainingDataManager.GetInstance().MachineLearningProgressRate;
            return rate;
        }
    }
}