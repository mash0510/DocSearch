using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.Models
{
    /// <summary>
    /// 設定画面のモデル
    /// </summary>
    public class SetupModel
    {
        /// <summary>
        /// クロール先のフォルダ
        /// </summary>
        public string CrawlFolders { set; get; }

        /// <summary>
        /// クロール後の機械学習を実行するかどうか（0：実行しない、1：実行する）
        /// </summary>
        public int ExecMachineLearning { set; get; }


        private bool _processFinished = false;
        /// <summary>
        /// 処理完了したかどうか
        /// </summary>
        public bool ProcessFinished
        {
            get { return _processFinished; }
        }

        private string _message = string.Empty;
        /// <summary>
        /// 応答メッセージ
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _processFinished = true;
                _message = value;
            }
        }

        #region スケジューリング設定
        /// <summary>
        /// 定期実行スケジュールのタイプ（１回のみ、日毎、曜日毎など）
        /// </summary>
        public string ScheduleType
        {
            get;
            set;
        }

        /// <summary>
        /// 「１回のみ」を選択したときの実行日時
        /// </summary>
        public string OneTimeDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 「1回のみ」以外を選択したときの実行時間
        /// </summary>
        public string ExecTime
        {
            get;
            set;
        }

        /// <summary>
        /// 日毎を選択した場合の、実行日インターバル
        /// </summary>
        public int DayInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 月曜実行
        /// </summary>
        public bool ExecMonday
        {
            get;
            set;
        }

        /// <summary>
        /// 火曜実行
        /// </summary>
        public bool ExecTuesday
        {
            get;
            set;
        }

        /// <summary>
        /// 水曜実行
        /// </summary>
        public bool ExecWendnesday
        {
            get;
            set;
        }

        /// <summary>
        /// 木曜実行
        /// </summary>
        public bool ExecThursday
        {
            get;
            set;
        }

        /// <summary>
        /// 金曜実行
        /// </summary>
        public bool ExecFriday
        {
            get;
            set;
        }

        /// <summary>
        /// 土曜実行
        /// </summary>
        public bool ExecSurtarday
        {
            get;
            set;
        }

        /// <summary>
        /// 日曜実行
        /// </summary>
        public bool ExecSunday
        {
            get;
            set;
        }

        /// <summary>
        /// cron形式の設定文字列
        /// </summary>
        public string CronString
        {
            get;
            set;
        }
        #endregion
    }
}