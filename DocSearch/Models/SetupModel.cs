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

    }
}