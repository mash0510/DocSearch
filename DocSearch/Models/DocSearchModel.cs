using FolderCrawler;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocSearch.Models
{
    /// <summary>
    /// 1件分の文書情報
    /// </summary>
    public class DocData
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
        /// ドキュメント文書内容。キーワードの周辺の文書データを入れる
        /// </summary>
        public string DocSummary{ get; set; }
    }

    /// <summary>
    /// 検索された文書データのモデル
    /// </summary>
    public class DocSearchModel
    {
        /// <summary>
        /// 検索された文書データのリスト
        /// </summary>
        public List<DocData> SearchedDocument { get; set; }

        #region 検索キーワード
        /// <summary>
        /// 検索キーワード
        /// </summary>
        [Display(Name = "検索キーワード")]
        public string InputKeywords { get; set; }

        /// <summary>
        /// 検索キーワードの関連語（Key : 入力されたキーワード, Value : word2vecが返した、そのキーワードの関連語一覧）
        /// </summary>
        public Dictionary<string, SortedList<float, string>> RelatedWords { get; set; }

        /// <summary>
        /// 検索キーワードの情報の一覧の取得
        /// </summary>
        public List<string> RelatedWordList(int num)
        {
            List<string> retval = new List<string>();

            int i = 0;

            foreach(SortedList<float, string> relatedWords in RelatedWords.Values)
            {
                foreach(string word in relatedWords.Values)
                {
                    retval.Add(word);

                    i++;
                    if (i >= num)
                        break;
                }

                if (i >= num)
                    break;

            }

            return retval;
        }
        #endregion
    }
}