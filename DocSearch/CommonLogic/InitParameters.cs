using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// パラメータの初期化
    /// </summary>
    public static class InitParameters
    {
        public static void Initialize()
        {
            // ホームディレクトリ（設定ファイルなど、検索システムの動作に必要なファイルのあるフォルダ）の初期化
            CommonParameters.HomeDirectory = ReadSettings.HomeDirectory;

            // 訓練データの最大サイズ。あまりにも大きくなりすぎると学習の処理時間が大きくなるため、制限をかけられるようにする。
            // この値が0だと、最大サイズなしとなる。
            CommonParameters.MaxTrainingFileSize = ReadSettings.MaxTrainingFileSize;

            // Elasticsearchへのデータ挿入スレッド、訓練データ生成スレッドのタイムアウト終了時間。
            // Queueからのデータ取得が0件である状態がここで設定した時間だけ経過したら、そのスレッドを終了させる。
            CommonParameters.WorkerThreadStopDuration = ReadSettings.ThreadTimeout;

            // ElasticsearchのURI
            SearchEngineConnection.ElasticsearchUri = new Uri(ReadSettings.ElasticsearchURI);

            // 検索システムのIndex名
            SearchEngineConnection.IndexName = ReadSettings.ElasticsearchIndex;
        }
    }
}