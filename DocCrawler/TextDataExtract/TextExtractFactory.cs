using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.TextDataExtract
{
    public class TextExtractFactory
    {
        /// <summary>
        /// Toxyを使ったテキスト抽出
        /// </summary>
        public const string TOXY = "TOXY";
        /// <summary>
        /// xdoc2txtを使ったテキスト抽出
        /// </summary>
        public const string XDOC2TXT = "XDOC2TXT";

        /// <summary>
        /// テキスト抽出方式の設定と取得
        /// </summary>
        public static string TextExtractWay
        {
            get;
            set;
        }

        /// <summary>
        /// テキスト抽出オブジェクトの取得
        /// </summary>
        /// <returns></returns>
        public static ITextExtract GeObj()
        {
            ITextExtract obj = null;

            if (TextExtractWay == TOXY)
            {
                obj = new ExtractWithToxy();
            }
            else
            {
                obj = new ExtractWithXdoc2Txt();
            }

            return obj;
        }
    }
}
