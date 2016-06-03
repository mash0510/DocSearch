using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 検索エンジンへの接続情報を保持するクラス
    /// </summary>
    public static class SearchEngineConnection
    {
        private static Uri _node = new Uri("http://localhost:9200");
        private static ConnectionSettings _connSettings = null;
        private static ElasticClient _client = null;
        private static string _index = "docinfoindex";

        /// <summary>
        /// ElasticsearchのサーバーのURIの設定と取得
        /// </summary>
        public static Uri ElasticsearchUri
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;

                _client = null;
                InitConnectClient();
            }
        }

        /// <summary>
        /// ElasticsearchのIndex名の設定と取得。すべて小文字でないとElasticsearchは受け付けてくれないみたい。
        /// </summary>
        public static string IndexName
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// Elasticsearchの接続インスタンスの取得
        /// </summary>
        public static ElasticClient Client
        {
            get { return _client; }
        }
        

        /// <summary>
        /// Elasticsearchの接続クライアントのインスタンスを初期化
        /// </summary>
        public static void InitConnectClient()
        {
            if (_client != null)
                return;

            _connSettings = new ConnectionSettings(_node);
            _client = new ElasticClient(_connSettings);
        }
    }
}
