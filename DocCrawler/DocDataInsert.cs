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
        /// 検索エンジンへのデータ投入をストップ
        /// </summary>
        private bool _stop = false;

        /// <summary>
        /// 経過時間測定インスタンス
        /// </summary>
        private TimeElapse _timeElapse = new TimeElapse();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DocDataInsert()
        {
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
                _idObj = new GenerateIDFactory<GetGUID>().Create();
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

            try
            {
                ITextExtract textExt = new TextExtractFactory<ExtractWithXdoc2Txt>().Create();
                docInfo.DocContent = textExt.Extract(fi.FullName, Encoding.Unicode);

                return docInfo;
            }
            catch
            {
                // 例外が発生してもnullを返してクロールは続ける。
                return null;
            }
        }

        /// <summary>
        /// 検索エンジンへのデータ投入をストップ
        /// </summary>
        public void Stop()
        {
            this._stop = true;
        }

        /// <summary>
        /// ワーカースレッドの未処理時間経過後、ワーカースレッドを止める
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeElapse_Elapsed(object sender, EventArgs e)
        {
            this._stop = true;
            _timeElapse.TimerStop();
        }

        /// <summary>
        /// 実処理
        /// </summary>
        public void DocDataInsertProc()
        {
            this._stop = false;

            while(true)
            {
                if (QueueManager.GetInstance().FileInfoQueue.Count == 0)
                {
                    _timeElapse.TimerStart(CommonParameters.WorkerThreadStopDuration);

                    if (this._stop)
                        break;
                    else
                        continue;
                }

                _timeElapse.TimerStop();

                FileInfo fi = QueueManager.GetInstance().FileInfoQueue.Dequeue() as FileInfo;

                if (fi == null)
                    continue;

                DocumentInfo docInfo = GenerateDocumentInfo(fi);
                if (docInfo == null)
                    continue;

                string id = IDDictionary.GetInstanse().GetElasticsearchID(docInfo.FileFullPath);

                if (id == null)
                    id = GetID();

                var response = SearchEngineConnection.Client.Index(docInfo, idx => idx.Index(SearchEngineConnection.IndexName).Id(id));

                IDDictionary.GetInstanse().AddID(docInfo.FileFullPath, id);

                // 訓練データ作成ロジックに、ドキュメントデータを渡す。
                QueueManager.GetInstance().DocInfoQueue.Enqueue(docInfo);
            }
        }
    }
}
