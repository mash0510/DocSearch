﻿using FolderCrawler;
using FolderCrawler.GenerateID;
using FolderCrawler.TextDataExtract;
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

            // 設定ファイルや訓練データなどのファイル読み書きバッファサイズ
            CommonParameters.FileIOBufferSize = ReadSettings.FileIOBufferSize;

            // ElasticsearchのURI
            SearchEngineConnection.ElasticsearchUri = new Uri(ReadSettings.ElasticsearchURI);

            // 検索システムのIndex名
            SearchEngineConnection.IndexName = ReadSettings.ElasticsearchIndex;

            // テキスト抽出方式
            TextExtractFactory.TextExtractWay = ReadSettings.TextExtractWay;

            // ElasticsearchのID生成方式
            GenerateIDFactory.IDType = ReadSettings.IDType;

            // 履歴ファイル名と保持する履歴件数
            History.CrawlHistory.HistoryFolder = CommonParameters.HistoryFolderCrawl;
            History.CrawlHistory.HistoryFile = ReadSettings.HistoryFileNameCrawl;
            History.CrawlHistory.MaxRecord = ReadSettings.HistoryMaxRecord;

            History.Word2VecHistory.HistoryFolder = CommonParameters.HistoryFolderWord2Vec;
            History.Word2VecHistory.HistoryFile = ReadSettings.HistoryFileNameWord2Vec;
            History.Word2VecHistory.MaxRecord = ReadSettings.HistoryMaxRecord;
        }

        /// <summary>
        /// システム起動時のデータロード
        /// </summary>
        public static void Load()
        {
            // 文書IDのロード
            IDDictionary.GetInstanse().LoadAsync();

            // 履歴のロード
            History.CrawlHistory.Load();
            History.Word2VecHistory.Load();
        }
    }
}