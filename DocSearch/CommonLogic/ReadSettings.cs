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
        public static int ThreadTimeout
        {
            get
            {
                string val = ConfigurationManager.AppSettings["ThreadTimeout"];
                int durationTime;

                int.TryParse(val, out durationTime);

                return durationTime;
            }
        }

        /// <summary>
        /// 訓練データの最大ファイルサイズの取得
        /// </summary>
        public static long MaxTrainingFileSize
        {
            get
            {
                string val = ConfigurationManager.AppSettings["MaxTrainingFileSize"];
                long sizeGB= (long)0;

                long.TryParse(val, out sizeGB);

                long sizeKB = sizeGB * 1024 * 1024 * 1024;

                return sizeKB;
            }
        }
    }
}