using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.History
{
    /// <summary>
    /// 履歴情報1件分
    /// </summary>
    public struct Word2VecHistoryInfo
    {
        /// <summary>
        /// キャンセルされたかどうか
        /// </summary>
        public bool isCanceled;
    }

    public class HistoryWord2Vec : HistoryBase<Word2VecHistoryInfo>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HistoryWord2Vec() : base()
        {

        }

        /// <summary>
        /// 履歴レコードインスタンスの取得
        /// </summary>
        /// <returns></returns>
        public override Word2VecHistoryInfo CreateHistoryInstance()
        {
            return new Word2VecHistoryInfo();
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
        private string ConvertHistoryInfo(Word2VecHistoryInfo record)
        {
            StringBuilder retval = new StringBuilder();

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
            bool isCanceled = false;

            DateTime.TryParse(data[0], out dt);
            bool.TryParse(data[1], out isCanceled);

            Word2VecHistoryInfo info = new Word2VecHistoryInfo();
            info.isCanceled = isCanceled;

            _history.Add(dt, info);
        }
    }
}
