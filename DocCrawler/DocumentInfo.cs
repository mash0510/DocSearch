using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 文書情報
    /// </summary>
    public class DocumentInfo
    {
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime UpdatedDate { get; set; }
        /// <summary>
        /// ファイルのフルパス
        /// </summary>
        public string FileFullPath { get; set; }
        /// <summary>
        /// 拡張子
        /// </summary>
        public string Extention { get; set; }
        /// <summary>
        /// ドキュメント文書内容。テキスト抽出したドキュメントの全文をここに入れる。
        /// </summary>
        public string DocContent { get; set; }
    }
}
