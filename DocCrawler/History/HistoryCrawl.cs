using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.History
{
    /// <summary>
    /// 履歴情報1件分
    /// </summary>
    public struct CrawlHistoryInfo
    {
        /// <summary>
        /// 全体のファイル数
        /// </summary>
        public decimal allFileNum;
        /// <summary>
        /// クロールしたファイル数
        /// </summary>
        public decimal fileNum;
        /// <summary>
        /// キャンセルされたかどうか
        /// </summary>
        public bool isCanceled;
    }

    /// <summary>
    /// クロールの履歴
    /// </summary>
    public class HistoryCrawl : HistoryBase<CrawlHistoryInfo>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HistoryCrawl() : base()
        {

        }

        /// <summary>
        /// 履歴レコードインスタンスの取得
        /// </summary>
        /// <returns></returns>
        public override CrawlHistoryInfo CreateHistoryInstance()
        {
            return new CrawlHistoryInfo();
        }

        /// <summary>
        /// ファイルに書き込む形式のデータを取得
        /// </summary>
        /// <returns></returns>
        protected override string GetSaveRecord(DateTime dt)
        {
            string keyData = base.GetSaveRecord(dt);

            StringBuilder record = new StringBuilder();

            record.Append(keyData);
            record.Append(ConvertHistoryInfo(_history[dt]));

            return record.ToString();
        }

        /// <summary>
        /// 履歴1レコードの文字列型への変換
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private string ConvertHistoryInfo(CrawlHistoryInfo record)
        {
            StringBuilder retval = new StringBuilder();

            retval.Append(record.allFileNum.ToString()).Append("\t");
            retval.Append(record.fileNum.ToString()).Append("\t");
            retval.Append(record.isCanceled.ToString());

            return retval.ToString();
        }

        /// <summary>
        /// 履歴データをSortedListに積み上げる
        /// </summary>
        /// <param name="data"></param>
        protected override void AddHistoryRecord(string[] data)
        {
            DateTime dt = new DateTime(1900, 1, 1, 0, 0, 0);
            decimal allFileNum = 0;
            decimal fileNum = 0;
            bool isCanceled = false;

            DateTime.TryParse(data[0], out dt);
            decimal.TryParse(data[1], out allFileNum);
            decimal.TryParse(data[2], out fileNum);
            bool.TryParse(data[3], out isCanceled);

            CrawlHistoryInfo info = new CrawlHistoryInfo();
            info.allFileNum = allFileNum;
            info.fileNum = fileNum;
            info.isCanceled = isCanceled;

            _history.Add(dt, info);
        }

        /// <summary>
        /// 文字列配列形式に履歴データを変換。降順ソートする。
        /// </summary>
        /// <returns></returns>
        protected override string[] ConvertToStringArray()
        {
            List<string> data = new List<string>();

            StringBuilder record = new StringBuilder();

            foreach(DateTime dt in _history.Keys)
            {
                record.Append(dt.ToString()).Append(",");
                record.Append(_history[dt].allFileNum.ToString()).Append(",");
                record.Append(_history[dt].fileNum.ToString()).Append(",");
                record.Append(_history[dt].isCanceled.ToString());

                data.Add(record.ToString());

                record.Clear();
            }

            string[] retval = data.ToArray();

            return retval;
        }
    }
}
