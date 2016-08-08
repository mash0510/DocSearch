using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// Web.configの設定の読み込み
    /// </summary>
    public static class ReadSettings
    {
        /// <summary>
        /// ホームディレクトリの取得
        /// </summary>
        public static string HomeDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["HomeDirectory"];
            }
        }

        /// <summary>
        /// ElasticsearchのURIの取得
        /// </summary>
        public static string ElasticsearchURI
        {
            get
            {
                return ConfigurationManager.AppSettings["ElasticsearchURI"];
            }
        }

        /// <summary>
        /// ElasticsearchのIndex名の取得
        /// </summary>
        public static string ElasticsearchIndex
        {
            get
            {
                return ConfigurationManager.AppSettings["ElasticsearchIndex"];
            }
        }

        /// <summary>
        /// Elasticsearchへのデータ挿入スレッドや訓練データ生成スレッドが、Queueからのデータ受付 0件になった後に何秒後に終了するかを取得
        /// </summary>
        public static string ThreadTimeout
        {
            get
            {
                return ConfigurationManager.AppSettings["ThreadTimeout"];
            }
        }
    }
}