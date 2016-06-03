using System;
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
        /// クロールした文書ファイルの数
        /// </summary>
        private decimal _crawledDocCount = 0;

        /// <summary>
        /// クロールが完了したかどうか
        /// </summary>
        public bool IsFinished
        {
            get; set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DocCrawler()
        {
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

        public delegate void CrawlFinishedDelegate(object sender, decimal crawledFileCount, string crawlRootFolder);
        public event CrawlFinishedDelegate CrawlFinished;

        /// <summary>
        /// クロール実行
        /// </summary>
        public void Crawl()
        {
            IsFinished = false;
            _crawledDocCount = 0;

            DirectoryInfo di = new DirectoryInfo(this.CrawlRoot);
            CrawlRecursive(di);

            IsFinished = true;

            CrawlFinished?.Invoke(this, _crawledDocCount, this.CrawlRoot);
        }

        /// <summary>
        /// クロールの再帰処理
        /// </summary>
        /// <param name="di"></param>
        private void CrawlRecursive(DirectoryInfo di)
        {
            try
            {
                if (di.GetDirectories().Count() > 0)
                {
                    foreach (DirectoryInfo diChild in di.GetDirectories())
                    {
                        CrawlRecursive(diChild);
                    }
                }

                foreach (FileInfo fi in di.GetFiles())
                {
                    if (!TargetFileExt.Contains(fi.Extension))
                        continue;

                    // 得たファイルオブジェクトをキューイングする。
                    // このファイルからテキスト抽出＆ElasticsearchへのIndexingは別スレッドで行う。
                    QueueManager.GetInstance().FileInfoQueue.Enqueue(fi);

                    _crawledDocCount++;
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