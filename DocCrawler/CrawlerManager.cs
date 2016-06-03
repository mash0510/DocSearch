﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// クローラー管理クラス
    /// </summary>
    public class CrawlerManager
    {
        /// <summary>
        /// Elasticsearchへの文書Indexingインスタンス
        /// </summary>
        private static DocDataInsert _indexing = new DocDataInsert();
        /// <summary>
        /// クローラーインスタンスのリスト
        /// </summary>
        private static List<DocCrawler> _crawlList = new List<DocCrawler>();
        /// <summary>
        /// クロール先のルートフォルダのリスト
        /// </summary>
        private List<string> _crawlFolderList = new List<string>();


        private bool _isAllCrawlFinished = true;
        /// <summary>
        /// 全てのクロールインスタンスのクロールが終わったかどうかの取得
        /// </summary>
        public bool IsAllCrawlFinished
        {
            get { return this._isAllCrawlFinished; }
        }

        /// <summary>
        /// クロールシステムのホームディレクトリの設定と取得。本システムの管理用ファイルの保存先ルートフォルダになる。
        /// </summary>
        public string HomeDirectory
        {
            set
            {
                CommonParameters.HomeDirectory = value;
            }
            get
            {
                return CommonParameters.HomeDirectory;
            }
        }

        /// <summary>
        /// クロール先のフォルダのリスト
        /// </summary>
        public List<string> CrawlFolderList
        {
            get { return this._crawlFolderList; }
            set { this._crawlFolderList = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CrawlerManager()
        {
        }

        /// <summary>
        /// 文書クロールインスタンスの生成
        /// </summary>
        private void InitCrawlInstances()
        {
            _crawlList.Clear();

            foreach (string crawlRoot in CrawlFolderList)
            {
                DocCrawler crawlInstance = new DocCrawler(crawlRoot);
                _crawlList.Add(crawlInstance);

                crawlInstance.CrawlFinished += CrawlInstance_CrawlFinished;
            }
        }

        /// <summary>
        /// 全てのクロールが完了したときに発生するイベント
        /// </summary>
        public event System.EventHandler AllCrawlFinished;

        /// <summary>
        /// クロール完了時の処理
        /// </summary>
        /// <param name="crawledFileCount"></param>
        /// <param name="crawlRootFolder"></param>
        private void CrawlInstance_CrawlFinished(object sender, decimal crawledFileCount, string crawlRootFolder)
        {
            // 全クロールインスタンスのクロール処理が終わったかどうかを検出する。

            bool allFinished = true;

            foreach (DocCrawler crawlInstance in _crawlList)
            {
                if (!crawlInstance.IsFinished)
                {
                    allFinished = false;
                    break;
                }
            }

            this._isAllCrawlFinished = allFinished;

            AllCrawlFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 文書クロールとElasticsearchへのIndexingのスタート
        /// </summary>
        public void Start()
        {
            InitCrawlInstances();

            Task.Run(() => {
                _indexing.DocDataInsertProc();
            });

            Task.Run(() => {
                TrainingDataManager.GetInstance().StartTrainingDataGenerate();
            });

            this._isAllCrawlFinished = false;

            foreach (DocCrawler crwl in _crawlList)
            {
                Task.Run(() =>
                {
                    crwl.Crawl();
                });
            }
        }
    }
}
