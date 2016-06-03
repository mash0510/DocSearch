using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// スレッド間のデータ受け渡しに使うキューを用意するクラス
    /// </summary>
    public class QueueManager
    {
        /// <summary>
        /// フォルダをクロールして取得したFileInfoインスタンスをElasticsearchにIndexingするスレッドに送り込むキュー
        /// </summary>
        private Queue _filePathQueue = new Queue();
        private Queue _filePathQueueSynchronized = null;

        /// <summary>
        /// テキスト抽出したドキュメント情報を訓練データ生成ロジックに渡すキュー
        /// </summary>
        private Queue _docInfoQueue = new Queue();
        private Queue _docInfoQueueSynchronized = null;

        private static QueueManager _self = new QueueManager();

        /// <summary>
        /// ファイル情報を入れるキューの取得
        /// </summary>
        public Queue FileInfoQueue
        {
            get { return _filePathQueueSynchronized; }
        }

        /// <summary>
        /// テキスト抽出したドキュメント情報を訓練データ生成ロジックに渡すキューの取得
        /// </summary>
        public Queue DocInfoQueue
        {
            get { return _docInfoQueueSynchronized; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private QueueManager()
        {
            _filePathQueueSynchronized = Queue.Synchronized(_filePathQueue);
            _docInfoQueueSynchronized = Queue.Synchronized(_docInfoQueue);
        }

        /// <summary>
        /// インスタンスを返す
        /// </summary>
        /// <returns></returns>
        public static QueueManager GetInstance()
        {
            return _self;
        }
    }
}
