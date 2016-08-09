using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.GenerateID
{
    /// <summary>
    /// Elasticsearchへのデータ挿入時に付与するID生成クラスの取得
    /// </summary>
    public class GenerateIDFactory
    {
        /// <summary>
        /// 連番
        /// </summary>
        public const string SEQUENCE = "SEQUENCE";
        /// <summary>
        /// GUID
        /// </summary>
        public const string GUID = "GUID";

        /// <summary>
        /// テキスト抽出方式の設定と取得
        /// </summary>
        public static string IDType
        {
            get;
            set;
        }

        /// <summary>
        /// テキスト抽出オブジェクトの取得
        /// </summary>
        /// <returns></returns>
        public static IGenerateID GeObj()
        {
            IGenerateID obj = null;

            if (IDType == SEQUENCE)
            {
                obj = new SeqNum();
            }
            else
            {
                obj = new GetGUID();
            }

            return obj;
        }
    }
}
