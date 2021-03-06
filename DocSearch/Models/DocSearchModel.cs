﻿using DocSearch.CommonLogic;
using FolderCrawler;
using FolderCrawler.Setting;
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
        private string _fileFullPath = string.Empty;

        /// <summary>
        /// Elasticsearchへのindexing時に割り振ったID
        /// </summary>
        public string DocID { get; set; }
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
        public string FileFullPath
        {
            get
            {
                return this._fileFullPath;
            }
            set
            {
                if (!value.StartsWith(@"file:///"))
                {
                    this._fileFullPath = (@"file:///" + value).Replace(@"\", "/");
                }
                else
                {
                    this._fileFullPath = value.Replace(@"\", "/");
                }
            }
        }
        /// <summary>
        /// 拡張子
        /// </summary>
        public string Extention { get; set; }
        /// <summary>
        /// ドキュメント文書内容。キーワードの周辺の文書データを入れる
        /// </summary>
        public string DocSummary { get; set; }
        /// <summary>
        /// マッチの度合いを示すスコア
        /// </summary>
        public double Score { get; set; }
    }

    /// <summary>
    /// 検索画面のモデル
    /// </summary>
    public class DocSearchModel
    {
        /// <summary>
        /// 検索が実行されたかどうか
        /// </summary>
        public bool SearchExecuted { get; set; }

        /// <summary>
        /// 検索されたデータの総件数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 検索された文書データのリスト
        /// </summary>
        public List<DocData> SearchedDocument { get; set; }

        /// <summary>
        /// 検索対象フォルダ
        /// </summary>
        public string SearchFolder { get; set; }

        /// <summary>
        /// 検索先ルートフォルダ
        /// </summary>
        public string SearchRootFolders
        {
            get
            {
                string retval = string.Empty;

                foreach(string folder in Settings.GetInstance().CrawlFolders)
                {
                    retval = retval + "'" + folder.Replace(@"\", @"\\") + "',";
                }

                retval = retval.TrimEnd(new char[] { ',' });

                return retval;
            }
        }

        #region ページ表示
        /// <summary>
        /// 表示しているページ番号
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 全体のページ数
        /// </summary>
        public int TotalPage { get; set; }

        private int _pageSize = 10;
        /// <summary>
        /// 1ページあたりのデータ件数
        /// </summary>
        public int PageSize
        {
            get { return this._pageSize; }
            set { this._pageSize = value; }
        }

        /// <summary>
        /// ページ番号リスト
        /// </summary>
        public List<int> PageList { get; set; }
        #endregion

        #region 検索キーワード
        /// <summary>
        /// 検索キーワード
        /// </summary>
        [Display(Name = "検索キーワード")]
        public string InputKeywords { get; set; }

        /// <summary>
        /// 検索キーワードリスト
        /// </summary>
        public List<string> InputKeywordList { get; set; }

        /// <summary>
        /// 検索キーワードの関連語（Key : 入力されたキーワード, Value : word2vecが返した、そのキーワードの関連語一覧）
        /// </summary>
        public Dictionary<string, SortedList<float, string>> RelatedWords { get; set; }

        /// <summary>
        /// 検索キーワードの情報の一覧の取得
        /// </summary>
        /// <param name="keyword">指定したキーワードの関連語を取得</param>
        /// <param name="num">取得する関連語の数</param>
        public List<string> RelatedWordList(string keyword, int num)
        {
            List<string> retval = new List<string>();

            SortedList<float, string> relatedWords = RelatedWords[keyword];

            int i = 0;

            foreach (string relatedWord in relatedWords.Values)
            {
                retval.Add(relatedWord);

                i++;
                if (i >= num)
                    break;
            }

            return retval;
        }
        #endregion

        /// <summary>
        /// フォルダツリービューのモデル
        /// </summary>
        public FileTreeViewModel FileTreeViewModel { get; set; }

        /// <summary>
        /// 関連語の表示数
        /// </summary>
        public int RelatedWordDispNum
        {
            get
            {
                return ReadSettings.RelatedWordDispNum;
            }
        }

        /// <summary>
        /// 類似文書検索かどうか（true : 類似文書検索 / false：通常のキーワード検索）
        /// 
        /// </summary>
        public bool IsSimilarDocSearch { get; set; }
    }
}