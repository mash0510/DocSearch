﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 共有フォルダ内の文書ファイルクローラー
/// </summary>
namespace FolderCrawler
{
    /// <summary>
    /// 文書ファイルのクローラー
    /// </summary>
    public class DocCrawler
    {
        /// <summary>
        /// クロールするルートフォルダ
        /// </summary>
        public string CrawlRoot { get; set; }
        /// <summary>
        /// 対象とするファイルの拡張子
        /// </summary>
        public List<string> TargetFileExt { get; set; }

        /// <summary>
        /// 処理中かどうかの取得
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// クロールした文書ファイルの数
        /// </summary>
        private decimal _crawledDocCount = 0;

        /// <summary>
        /// クロールの停止フラグ
        /// </summary>
        private bool _stop = false;

        /// <summary>
        /// クロール停止
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

        public delegate void CrawlFinishedDelegate(object sender, decimal crawledFileCount, string crawlRootFolder, bool isCanceled);
        /// <summary>
        /// クロールの完了を通知するイベント
        /// </summary>
        public event CrawlFinishedDelegate CrawlFinished;

        public delegate void DocCrawled(object sender, decimal cumulativeFileCount);
        /// <summary>
        /// ファイルが1件クロールされると発生するイベント
        /// </summary>
        public event DocCrawled SingleDocCrawled;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DocCrawler()
        {
            IsProcessing = false;
            SetDefaultFileExt();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        public DocCrawler(string rootPath)
        {
            this.CrawlRoot = rootPath;
            SetDefaultFileExt();
        }

        private void SetDefaultFileExt()
        {
            if (TargetFileExt == null)
                TargetFileExt = new List<string>();

            UniqueAdd(this.TargetFileExt, ".txt");
            UniqueAdd(this.TargetFileExt, ".xml");
            UniqueAdd(this.TargetFileExt, ".csv");
            UniqueAdd(this.TargetFileExt, ".doc");
            UniqueAdd(this.TargetFileExt, ".docx");
            UniqueAdd(this.TargetFileExt, ".xls");
            UniqueAdd(this.TargetFileExt, ".xlsx");
            UniqueAdd(this.TargetFileExt, ".ppt");
            UniqueAdd(this.TargetFileExt, ".pptx");
            UniqueAdd(this.TargetFileExt, ".rtf");
            UniqueAdd(this.TargetFileExt, ".pdf");
            UniqueAdd(this.TargetFileExt, ".htm");
            UniqueAdd(this.TargetFileExt, ".html");
            UniqueAdd(this.TargetFileExt, ".eml");
            UniqueAdd(this.TargetFileExt, ".vcf");
            UniqueAdd(this.TargetFileExt, ".zip");
            UniqueAdd(this.TargetFileExt, ".vsd");
            UniqueAdd(this.TargetFileExt, ".vsdx");

            // 以下はVoxyが対応しているファイルではあるが、文書ファイルではないので除外
            //UniqueAdd(this.TargetFileExt, ".pub");
            //UniqueAdd(this.TargetFileExt, ".shw");
            //UniqueAdd(this.TargetFileExt, ".sldpart");
            //UniqueAdd(this.TargetFileExt, ".mp3");
            //UniqueAdd(this.TargetFileExt, ".ape");
            //UniqueAdd(this.TargetFileExt, ".wma");
            //UniqueAdd(this.TargetFileExt, ".flac");
            //UniqueAdd(this.TargetFileExt, ".aif");
            //UniqueAdd(this.TargetFileExt, ".jpeg");
            //UniqueAdd(this.TargetFileExt, ".jpg");
            //UniqueAdd(this.TargetFileExt, ".gif");
            //UniqueAdd(this.TargetFileExt, ".tiff");
            //UniqueAdd(this.TargetFileExt, ".png");
        }

        /// <summary>
        /// Listに存在しないデータのみ追加する
        /// </summary>
        /// <param name="list"></param>
        /// <param name="data"></param>
        private void UniqueAdd(List<string> list, string data)
        {
            if (list.Contains(data))
                return;

            list.Add(data);
        }

        /// <summary>
        /// クロール処理の強制停止
        /// </summary>
        public void Stop()
        {
            StopProcessing = true;
        }

        /// <summary>
        /// クロール実行
        /// </summary>
        public void Crawl()
        {
            StopProcessing = false;
            _crawledDocCount = 0;

            // 実行前にキューの中をクリアする。
            // 前回実行時に、クロール途中でキャンセルされ、キュー内にデータが残っている場合があるので、一旦キューの中身をクリアしてから使う。
            QueueManager.GetInstance().FileInfoQueue.Clear();

            DirectoryInfo di = new DirectoryInfo(this.CrawlRoot);
            CrawlRecursive(di);

            StopProcessing = true;

            CrawlFinished?.Invoke(this, _crawledDocCount, this.CrawlRoot, StopProcessing);
        }

        /// <summary>
        /// クロールの再帰処理
        /// </summary>
        /// <param name="di"></param>
        private void CrawlRecursive(DirectoryInfo di)
        {
            if (!IDDictionary.GetInstanse().IDLoaded)
            {
                // 文書IDのロードが完了していない場合は、IDの読み込みを行う。
                IDDictionary.GetInstanse().Load();
            }

            try
            {
                if (di.GetDirectories().Count() > 0)
                {
                    foreach (DirectoryInfo diChild in di.GetDirectories())
                    {
                        CrawlRecursive(diChild);
                    }
                }

                if (StopProcessing)
                {
                    return;
                }

                foreach (FileInfo fi in di.GetFiles())
                {
                    if (StopProcessing)
                    {
                        return;
                    }

                    if (!TargetFileExt.Contains(fi.Extension))
                        continue;

                    // 得たファイルオブジェクトをキューイングする。
                    // このファイルからテキスト抽出＆ElasticsearchへのIndexingは別スレッドで行う。
                    QueueManager.GetInstance().FileInfoQueue.Enqueue(fi);

                    _crawledDocCount++;
                    SingleDocCrawled?.Invoke(this, _crawledDocCount);
                }
            }
            catch(Exception ex)
            {
                // 例外は握りつぶす。
                // アクセス権などの問題で例外が出ることがあるが、それが出てもシステムを止めないでクロールし続けるようにする。
                // ログは出力する。
            }
        }
    }
}