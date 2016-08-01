using System;
using System.Collections.Generic;
using System.IO;
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
            set { CommonParameters.HomeDirectory = value; }
            get { return CommonParameters.HomeDirectory;  }
        }

        /// <summary>
        /// クロール先のフォルダのリスト
        /// </summary>
        public List<string> CrawlFolderList
        {
            get { return Settings.GetInstance().CrawlFolders; }
            set { Settings.GetInstance().CrawlFolders = value; }
        }

        /// <summary>
        /// クロールした文書ファイルの数
        /// </summary>
        public decimal TotalDocuments
        {
            get;
            private set;
        }

        /// <summary>
        /// 前回のクロール処理にてクロールしたファイルの数
        /// </summary>
        public decimal PrevTotalDocuments
        {
            get;
            private set;
        }

        /// <summary>
        /// クロール処理実行中に、クロールが完了したファイルの数を取得できるプロパティ
        /// </summary>
        public decimal CumulativeCrawlDocuments
        {
            get;
            private set;
        }

        /// <summary>
        /// クロールの進捗率の取得
        /// </summary>
        public int CrawlProgressRate
        {
            get
            {
                double docNum = (double)CumulativeCrawlDocuments;
                double totalNum = (double)PrevTotalDocuments;
                int retval = (int)Math.Ceiling((docNum / totalNum) * 100);

                return retval;
            }
        }

        /// <summary>
        /// クロール処理実行中に、ElasticSearchへのIndexingが完了したファイルの数を取得できるプロパティ
        /// </summary>
        public decimal CumulativeInsertedDocuments
        {
            get;
            private set;
        }

        /// <summary>
        /// ElasticSearchへのIndexingの進捗率
        /// </summary>
        public int InsertProgressRate
        {
            get
            {
                double docNum = (double)CumulativeInsertedDocuments;
                double totalNum = (double)PrevTotalDocuments;
                int retval = (int)Math.Ceiling((docNum / totalNum) * 100);

                return retval;
            }
        }

        public delegate void DocCrawlDelegate(object sender, decimal totalFiles);
        /// <summary>
        /// 全てのクロールが完了したときに発生するイベント
        /// </summary>
        public event DocCrawlDelegate AllCrawlFinished;
        /// <summary>
        /// 全てのElastisSearchへのIndexingが完了したときに発生するイベント
        /// </summary>
        public event DocCrawlDelegate AllDocInsertFinished;

        private static CrawlerManager _self = new CrawlerManager();

        /// <summary>
        /// インスタンスの取得
        /// </summary>
        /// <returns></returns>
        public static CrawlerManager GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private CrawlerManager()
        {
            LoadTotalDocuments();
        }

        /// <summary>
        /// 文書クロールインスタンスの生成
        /// </summary>
        private void InitCrawlInstances()
        {
            TotalDocuments = 0;
            _crawlList.Clear();

            foreach (string crawlRoot in CrawlFolderList)
            {
                DocCrawler crawlInstance = new DocCrawler(crawlRoot);
                _crawlList.Add(crawlInstance);

                crawlInstance.SingleDocCrawled -= CrawlInstance_SingleDocCrawled;
                crawlInstance.CrawlFinished -= CrawlInstance_CrawlFinished;

                crawlInstance.SingleDocCrawled += CrawlInstance_SingleDocCrawled;
                crawlInstance.CrawlFinished += CrawlInstance_CrawlFinished;
            }

            _indexing.SingleDocInserted -= _indexing_SingleDocInserted;
            _indexing.SingleDocInserted += _indexing_SingleDocInserted;

            _indexing.DocInsertFinished -= _indexing_DocInsertFinished;
            _indexing.DocInsertFinished += _indexing_DocInsertFinished;
        }

        /// <summary>
        /// 1ファイルクロールする毎に実行されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cumulativeFileCount"></param>
        private void CrawlInstance_SingleDocCrawled(object sender, decimal cumulativeFileCount)
        {
            CumulativeCrawlDocuments = cumulativeFileCount;
        }

        /// <summary>
        /// ElasticSearchに1ファイルIndexingされる毎に実行されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cumulativeDocCount"></param>
        private void _indexing_SingleDocInserted(object sender, decimal cumulativeDocCount)
        {
            CumulativeInsertedDocuments++;
        }

        /// <summary>
        /// クロール完了時の処理
        /// </summary>
        /// <param name="crawledFileCount"></param>
        /// <param name="crawlRootFolder"></param>
        private void CrawlInstance_CrawlFinished(object sender, decimal crawledFileCount, string crawlRootFolder)
        {
            // 全クロールインスタンスのクロール処理が終わったかどうかを検出する。

            TotalDocuments += crawledFileCount;

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

            if (allFinished)
            {
                PrevTotalDocuments = TotalDocuments;
                SaveTotalDocuments();
                AllCrawlFinished?.Invoke(this, TotalDocuments);
            }
        }

        /// <summary>
        /// 全てのElastisSearchへのIndexingが完了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="totalDocCount"></param>
        private void _indexing_DocInsertFinished(object sender, decimal totalDocCount)
        {
            AllDocInsertFinished?.Invoke(this, totalDocCount);
        }

        /// <summary>
        /// クロールした文書ファイル数の保存
        /// </summary>
        private void SaveTotalDocuments()
        {
            StreamWriter sw = new StreamWriter(CommonParameters.TotalDocumentsFile, false);

            try
            {
                sw.WriteLine(TotalDocuments.ToString());
            }
            catch
            {
                // 後ほどログ出力
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// 前回クロールした文書ファイルの数の読み込み
        /// </summary>
        private void LoadTotalDocuments()
        {
            StreamReader sr = new StreamReader(CommonParameters.TotalDocumentsFile);

            try
            {
                string prevTotalDocuments = sr.ReadLine();

                decimal total;
                decimal.TryParse(prevTotalDocuments, out total);

                PrevTotalDocuments = total;
            }
            catch
            {
                // 後ほどログ出力
            }
            finally
            {
                sr.Close();
            }

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
