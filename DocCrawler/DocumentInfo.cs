using Nest;
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
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string FileName { get; set; }
        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime UpdatedDate { get; set; }
        /// <summary>
        /// フォルダパス
        /// </summary>
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string FolderPath { get; set; }
        /// <summary>
        /// ファイルのフルパス
        /// </summary>
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string FileFullPath { get; set; }
        /// <summary>
        /// 拡張子
        /// </summary>
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Extention { get; set; }
        /// <summary>
        /// ドキュメント文書内容。テキスト抽出したドキュメントの全文をここに入れる。
        /// </summary>
        [String(TermVector = TermVectorOption.WithPositionsOffsetsPayloads)]
        public string DocContent { get; set; }
    }
}
