using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.History
{
    /// <summary>
    /// クロールや機械学習実行の履歴の保存、取得を行うクラス
    /// </summary>
    public class HistoryBase<T>
    {
        /// <summary>
        /// 履歴ファイルの拡張子
        /// </summary>
        private string _extention = ".dat";

        /// <summary>
        /// 履歴データのデリミタ
        /// </summary>
        private char[] delimiter = { '\t' };

        /// <summary>
        /// 保存先フォルダの設定と取得
        /// </summary>
        public string HistoryFolder
        {
            get;
            set;
        }

        /// <summary>
        /// 履歴ファイル名接頭辞の設定と取得。
        /// ここで指定したファイル名に連番が付加されたファイル名で保存される。
        /// </summary>
        public string HistoryFile
        {
            get;
            set;
        }

        /// <summary>
        /// 保持する最大履歴データ数。直近の履歴データをこの件数分だけ保持する。
        /// それ以上になったら、古い履歴データは捨てる。
        /// </summary>
        public int MaxRecord
        {
            get;
            set;
        }

        /// <summary>
        /// 履歴データ
        /// </summary>
        protected SortedList<DateTime, T> _history = new SortedList<DateTime, T>();
        /// <summary>
        /// 履歴データの取得
        /// </summary>
        public SortedList<DateTime, T> HistoryData
        {
            get { return _history; }
        }

        /// <summary>
        /// 履歴データを文字列配列形式で取得
        /// </summary>
        public string[] HistoryDataArray
        {
            get { return ConvertToStringArray(); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HistoryBase()
        {

        }

        /// <summary>
        /// 履歴データを文字列配列に変換
        /// </summary>
        /// <returns></returns>
        protected virtual string[] ConvertToStringArray()
        {
            return null;
        }

        /// <summary>
        /// 履歴ファイル名の取得
        /// </summary>
        /// <returns></returns>
        protected string GetFileName()
        {
            string fileName = HistoryFolder + "\\" + HistoryFile + _extention;

            return fileName;
        }

        /// <summary>
        /// 履歴レコードインスタンスの取得
        /// </summary>
        /// <returns></returns>
        public virtual T CreateHistoryInstance()
        {
            return default(T);
        }

        /// <summary>
        /// 履歴データの追加
        /// </summary>
        /// <param name="historyData"></param>
        public void Add(T historyData)
        {
            DateTime dtNow = DateTime.Now;

            if (_history.ContainsKey(dtNow))
                return;

            _history[dtNow] = historyData;

            if (_history.Count > MaxRecord)
            {
                _history.RemoveAt(0);
            }
        }

        /// <summary>
        /// 履歴データの追加
        /// </summary>
        /// <param name="record"></param>
        private void Add(string record)
        {
            string[] data = record.Split(delimiter);
            AddHistoryRecord(data);
        }

        /// <summary>
        /// 履歴データをSortedListに積み上げる
        /// </summary>
        /// <param name="data"></param>
        protected virtual void AddHistoryRecord(string[] data)
        {

        }

        /// <summary>
        /// 履歴データをファイルに保存
        /// </summary>
        public void Save()
        {
            string fileName = GetFileName();
            CommonLogic.SafeCreateDirectory(Path.GetDirectoryName(fileName));

            StreamWriter sw = new StreamWriter(fileName, false);

            try
            {
                foreach (DateTime dt in _history.Keys)
                {
                    string saveRecord = GetSaveRecord(dt);
                    sw.WriteLine(saveRecord);
                }
            }
            catch (Exception ex)
            {
                // 後ほどログ出力をする
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// 履歴データの読み込み
        /// </summary>
        public void Load()
        {
            string fileName = GetFileName();

            if (!File.Exists(fileName))
                return;

            StreamReader sr = new StreamReader(fileName);

            try
            {
                while(sr.Peek() > 0)
                {
                    string record = sr.ReadLine();
                    Add(record);
                }
            }
            catch
            {
                // ログ出力を後で実装
            }
            finally
            {
                sr.Close();
            }
        }

        /// <summary>
        /// 保存するデータの1レコードを取得
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        protected virtual string GetSaveRecord(DateTime dt)
        {
            string record = dt.ToString() + "\t";

            return record;
        }
    }
}
