using FolderCrawler.GenerateID;
using FolderCrawler.TextDataExtract;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toxy;

namespace FolderCrawler
{
    /// <summary>
    /// 文書データのテキスト抽出と検索エンジンへの挿入。
    /// </summary>
    public class DocDataInsert
    {
        /// <summary>
        /// データへ与えるID生成オブジェクト
        /// </summary>
        private IGenerateID _idObj;
        /// <summary>
        /// テキスト抽出オブジェクト
        /// </summary>
        private ITextExtract _textExt;

        /// <summary>
        /// 処理の停止
        /// </summary>
        private bool _stop = false;
        /// <summary>
        /// ユーザーによるキャンセル操作が実行されたかどうか
        /// </summary>
        private bool _cancel = false;

        /// <summary>
        /// 処理の停止
        /// </summary>
        private bool StopProcessing
        {
            set
            {
                IsProcessing = !value;
                _stop = value;
            }
            get
            {
                return _stop;
            }
        }

        /// <summary>
        /// 経過時間測定インスタンス
        /// </summary>
        private TimeElapse _timeElapse = new TimeElapse();

        /// <summary>
        /// クロール処理実行中にElasticSearchに挿入した文書データの数
        /// </summary>
        public decimal CumurativeInsertedDocuments
        {
            get;
            private set;
        }

        /// <summary>
        /// 処理中かどうかの取得
        /// </summary>
        public bool IsProcessing
        {
            get;
            private set;
        }

        public delegate void DocInsertedDelegate(object sender, decimal docCount);
        /// <summary>
        /// クロール処理実行中にElasticSearchに1ファイルIndexingされると発生するイベント
        /// </summary>
        public event DocInsertedDelegate SingleDocInserted;

        public delegate void DocInsertedFinishedDelegate(object sender, decimal docCount, bool isCanceled);
        /// <summary>
        /// ElasticSearchへのIndexing完了時のイベント
        /// </summary>
        public event DocInsertedFinishedDelegate DocInsertFinished;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DocDataInsert()
        {
            IsProcessing = false;
            SearchEngineConnection.InitConnectClient();
            _timeElapse.Elapsed += _timeElapse_Elapsed;
        }

        /// <summary>
        /// IDの取得
        /// </summary>
        /// <remarks>
        /// デフォルトはGUID。違う採番体系にしたい場合は、採番ロジックの実装とその呼び出しを変更すること。
        /// </remarks>
        /// <returns></returns>
        private string GetID()
        {
            if (this._idObj == null)
            {
                _idObj = GenerateIDFactory.GeObj();
            }

            string retval = _idObj.GetID(null);

            return retval;
        }

        /// <summary>
        /// 検索エンジンに挿入する文書データの生成
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        private DocumentInfo GenerateDocumentInfo(FileInfo fi)
        {
            DocumentInfo docInfo = new DocumentInfo();

            docInfo.FileName = fi.Name;
            docInfo.Extention = fi.Extension;
            docInfo.UpdatedDate = fi.LastAccessTime;
            docInfo.FileFullPath = fi.FullName;
            docInfo.FolderPath = Path.GetDirectoryName(fi.FullName);

            try
            {
                if (_textExt == null)
                    _textExt = TextExtractFactory.GeObj();

                docInfo.DocContent = _textExt.Extract(fi.FullName, Encoding.Unicode);

                return docInfo;
            }
            catch
            {
                // 例外が発生してもnullを返してクロールは続ける。
                return null;
            }
        }

        /// <summary>
        /// データ挿入処理の強制停止
        /// </summary>
        public void Stop()
        {
            StopProcessing = true;
            _cancel = true;

            _timeElapse.TimerStop();
        }

        /// <summary>
        /// ワーカースレッドの未処理時間経過後、ワーカースレッドを止める
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeElapse_Elapsed(object sender, EventArgs e)
        {
            StopProcessing = true;
            _timeElapse.TimerStop();
        }

        /// <summary>
        /// 実処理
        /// </summary>
        public void DocDataInsertProc()
        {
            StopProcessing = false;
            _cancel = false;

            bool first = true;
            CumurativeInsertedDocuments = 0;

            while (true)
            {
                if (QueueManager.GetInstance().FileInfoQueue.Count == 0)
                {
                    if (!_timeElapse.IsTimerStarted)
                        _timeElapse.TimerStart(CommonParameters.WorkerThreadStopDuration);

                    if (StopProcessing)
                        break;
                    else
                        continue;
                }

                _timeElapse.TimerStop();

                if (StopProcessing)
                {
                    // ユーザーがキャンセル操作を実行したら、ループを停止する。
                    break;
                }

                FileInfo fi = QueueManager.GetInstance().FileInfoQueue.Dequeue() as FileInfo;

                if (fi == null)
                    continue;

                DocumentInfo docInfo = GenerateDocumentInfo(fi);
                if (docInfo == null)
                    continue;

                string id = IDDictionary.GetInstanse().GetElasticsearchID(docInfo.FileFullPath);

                if (id == null)
                    id = GetID();

                if (first)
                {
                    var result = SearchEngineConnection.Client.Count<DocumentInfo>(c => c
                        .Index(SearchEngineConnection.IndexName)
                    );

                    if (result.Count == 0)
                    {
                        // データが1件も入っていない場合は、Indexのmapping定義がなされていないので、mapping定義を実行する。
                        SearchEngineConnection.Client.CreateIndex(SearchEngineConnection.IndexName, c => c
                            .Mappings(ms => ms
                                .Map<DocumentInfo>(m => m.AutoMap()))
                        );
                    }

                    first = false;
                }

                var response = SearchEngineConnection.Client.Index(docInfo, idx => idx.Index(SearchEngineConnection.IndexName).Id(id));

                IDDictionary.GetInstanse().AddID(docInfo.FileFullPath, id, true);

                CumurativeInsertedDocuments++;
                SingleDocInserted?.Invoke(this, CumurativeInsertedDocuments);

                // 訓練データ作成ロジックに、ドキュメントデータを渡す。
                QueueManager.GetInstance().DocInfoQueue.Enqueue(docInfo);
            }

            DocInsertFinished?.Invoke(this, CumurativeInsertedDocuments, _cancel);
        }
    }
}
