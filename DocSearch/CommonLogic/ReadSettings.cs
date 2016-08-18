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
                int durationTime = ConvertInt(val);

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
                long sizeGB= 0;

                long.TryParse(val, out sizeGB);

                long sizeKB = sizeGB * 1024 * 1024 * 1024;

                return sizeKB;
            }
        }

        /// <summary>
        /// 設定ファイルや訓練データなどのファイル読み書きバッファサイズ。
        /// </summary>
        public static int FileIOBufferSize
        {
            get
            {
                string val = ConfigurationManager.AppSettings["FileIOBufferSize"];
                int buffer = ConvertInt(val);

                return buffer;
            }
        }

        /// <summary>
        /// テキスト抽出方式の取得
        /// </summary>
        public static string TextExtractWay
        {
            get
            {
                return ConfigurationManager.AppSettings["TextExtractWay"];
            }
        }

        /// <summary>
        /// テキスト抽出方式の取得
        /// </summary>
        public static string IDType
        {
            get
            {
                return ConfigurationManager.AppSettings["IDType"];
            }
        }

        /// <summary>
        /// クロールの履歴を記録するファイル名のプレフィックス。
        /// </summary>
        public static string HistoryFileNameCrawl
        {
            get
            {
                return ConfigurationManager.AppSettings["HistoryFileNameCrawl"];
            }
        }

        /// <summary>
        /// word2vecの履歴を記録するファイル名のプレフィックス。
        /// </summary>
        public static string HistoryFileNameWord2Vec
        {
            get
            {
                return ConfigurationManager.AppSettings["HistoryFileNameWord2Vec"];
            }
        }

        /// <summary>
        /// 履歴として記録する最大レコード数
        /// </summary>
        public static int HistoryMaxRecord
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["HistoryMaxRecord"]);
            }
        }

        /// <summary>
        /// int型への変換
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static int ConvertInt(string val)
        {
            int intval = 0;

            int.TryParse(val, out intval);

            return intval;
        }
    }
}