using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.Models
{
    /// <summary>
    /// 設定画面のモデル
    /// </summary>
    public class SetupModel
    {
        /// <summary>
        /// クロール先のフォルダ
        /// </summary>
        public string CrawlFolders { set; get; }

        /// <summary>
        /// クロール後の機械学習を実行するかどうか（0：実行しない、1：実行する）
        /// </summary>
        public int ExecMachineLearning { set; get; }
    }
}