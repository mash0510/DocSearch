using DocSearch.Controllers;
using DocSearch.hubs;
using DocSearch.Models;
using FolderCrawler.Setting;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// スケジューリング
    /// </summary>
    public class Scheduling
    {
        public const string TYPE = "SCHEDULING";

        public const string MESSAGE_SAVED = "保存しました";
        public const string MESSAGE_CRON_STRING_ERROR = "詳細設定の文字列が正しいcron形式になっていません。";
        public const string MESSAGE_NO_EXECUTE_DATE = "スケジュールが一度も実行されない設定です（過去日など）。";

        /// <summary>
        /// Key情報 - 機械学習をクロール後に実行するかどうか
        /// </summary>
        public const string KEY_EXEC_MACHINE_LEARNING = "execMachineLearning";

        /// <summary>
        /// スケジューラーオブジェクト
        /// </summary>
        IScheduler scheduler;

        /// <summary>
        /// 非実行
        /// </summary>
        private const string NONE = "none";
        /// <summary>
        /// １回のみ
        /// </summary>
        private const string ONE_TIME = "oneTime";
        /// <summary>
        /// 日毎
        /// </summary>
        private const string DATE = "date";
        /// <summary>
        /// 曜日指定
        /// </summary>
        private const string DAY = "day";
        /// <summary>
        /// 詳細設定
        /// </summary>
        private const string DETAIL = "detail";

        /// <summary>
        /// cronフォーマット 秒 分 時 日 月 曜日 年
        /// </summary>
        private const string CRON_FORMAT = "{0} {1} {2} {3} {4} {5} {6}";

        public delegate void ExecJobDelegate(Dictionary<string, object> args);
        /// <summary>
        /// 指定したスケジュールで定期的に発行されるイベント
        /// </summary>
        public event ExecJobDelegate ExecJob;

        private static Scheduling _self = new Scheduling();
        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <returns></returns>
        public static Scheduling GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private Scheduling()
        {
            var factory = new StdSchedulerFactory();
            scheduler = factory.GetScheduler();
        }

        /// <summary>
        /// スケジューリング設定のセット
        /// </summary>
        /// <param name="setupModel"></param>
        public void SetSchedule(string[] args)
        {
            ScheduleSettings.GetInstance().Backup();

            ScheduleSettings.GetInstance().ScheduleType = args[0];
            ScheduleSettings.GetInstance().OneTimeDateTime = args[1];
            ScheduleSettings.GetInstance().DayInterval = SafeConvInt(args[2]);
            ScheduleSettings.GetInstance().ExecTimeDaily = args[3];
            ScheduleSettings.GetInstance().ExecMonday = SaveConvBool(args[4]);
            ScheduleSettings.GetInstance().ExecTuesday = SaveConvBool(args[5]);
            ScheduleSettings.GetInstance().ExecWendnesday = SaveConvBool(args[6]);
            ScheduleSettings.GetInstance().ExecThursday = SaveConvBool(args[7]);
            ScheduleSettings.GetInstance().ExecFriday = SaveConvBool(args[8]);
            ScheduleSettings.GetInstance().ExecSurtarday = SaveConvBool(args[9]);
            ScheduleSettings.GetInstance().ExecSunday = SaveConvBool(args[10]);
            ScheduleSettings.GetInstance().ExecTimeDay = args[11];
            ScheduleSettings.GetInstance().CronString = args[12];

            SetQuartz();
        }

        /// <summary>
        /// スケジュール設定の取得
        /// </summary>
        /// <param name="setupModel"></param>
        public void GetSchedule(SetupModel setupModel)
        {
            setupModel.CronString = ScheduleSettings.GetInstance().CronString;
            setupModel.DayInterval = ScheduleSettings.GetInstance().DayInterval;
            setupModel.ExecFriday = ScheduleSettings.GetInstance().ExecFriday;
            setupModel.ExecMonday = ScheduleSettings.GetInstance().ExecMonday;
            setupModel.ExecSunday = ScheduleSettings.GetInstance().ExecSunday;
            setupModel.ExecSurtarday = ScheduleSettings.GetInstance().ExecSurtarday;
            setupModel.ExecThursday = ScheduleSettings.GetInstance().ExecThursday;
            setupModel.ExecTimeDaily = ScheduleSettings.GetInstance().ExecTimeDaily;
            setupModel.ExecTimeDay = ScheduleSettings.GetInstance().ExecTimeDay;
            setupModel.ExecTuesday = ScheduleSettings.GetInstance().ExecTuesday;
            setupModel.ExecWendnesday = ScheduleSettings.GetInstance().ExecWendnesday;
            setupModel.OneTimeDateTime = ScheduleSettings.GetInstance().OneTimeDateTime;
            setupModel.ScheduleType = ScheduleSettings.GetInstance().ScheduleType;
        }

        /// <summary>
        /// ブラウザ側にメッセージを送信
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connectionID"></param>
        public void SendMessage(string message, string connectionID)
        {
            ComHub.SendMessageToTargetClient(TYPE, message, new string[0], connectionID);
        }

        /// <summary>
        /// int型への変換
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private int SafeConvInt(string val)
        {
            int retval = 0;
            int.TryParse(val, out retval);

            return retval;
        }

        /// <summary>
        /// boolへの変換
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool SaveConvBool(string val)
        {
            bool retval = false;
            bool.TryParse(val, out retval);

            return retval;
        }

        /// <summary>
        /// cron形式の文字列が正しいかどうかのチェック
        /// </summary>
        /// <param name="cronString"></param>
        /// <returns></returns>
        public bool CheckCronString(string cronString)
        {
            bool result = CronExpression.IsValidExpression(cronString);

            return result;
        }

        /// <summary>
        /// スケジューリングモジュールへの設定
        /// </summary>
        public void SetQuartz()
        {
            if (scheduler == null)
                return;

            if (ScheduleSettings.GetInstance().ScheduleType == NONE)
            {
                scheduler.Clear();
                return;
            }

            var crawljob = JobBuilder.Create<ScheduleJob>()
                        .WithIdentity("CrawlJob", "group1")
                        .Build();

            //タイムゾーンを定義
            var timeZoneInfo = TimeZoneInfo.Local;

            string cronString = GetCronString();

            //起動のタイミングを定義、CRONと同じ要領で記述が可能、最後にタイムゾーン情報を付与
            var cronScheduleBuilder1 = CronScheduleBuilder.CronSchedule(cronString).InTimeZone(timeZoneInfo);


            //トリガー情報を定義
            var crawlTrigger = TriggerBuilder.Create()
                                    .WithSchedule(cronScheduleBuilder1)
                                    .Build();

            scheduler.Clear();

            //スケジューラーにジョブとトリガーの情報を渡す
            scheduler.ScheduleJob(crawljob, crawlTrigger);

            //ジョブを起動
            scheduler.Start();
        }

        /// <summary>
        /// スケジューリング設定のcron文字列化
        /// </summary>
        /// <returns></returns>
        private string GetCronString()
        {
            string retval = string.Empty;

            if (ScheduleSettings.GetInstance().ScheduleType == DETAIL)
            {
                retval = ScheduleSettings.GetInstance().CronString;
            }
            else if (ScheduleSettings.GetInstance().ScheduleType == ONE_TIME)
            {
                DateTime oneTime = Convert.ToDateTime(ScheduleSettings.GetInstance().OneTimeDateTime);

                string second = oneTime.Second.ToString();
                string minute = oneTime.Minute.ToString();
                string hour = oneTime.Hour.ToString();
                string date = oneTime.Day.ToString();
                string month = oneTime.Month.ToString();
                string year = oneTime.Year.ToString();

                retval = string.Format(CRON_FORMAT, second, minute, hour, date, month, "?", year);
            }
            else if (ScheduleSettings.GetInstance().ScheduleType == DATE)
            {
                DateTime time = Convert.ToDateTime(ScheduleSettings.GetInstance().ExecTimeDaily);

                string second = time.Second.ToString();
                string minute = time.Minute.ToString();
                string hour = time.Hour.ToString();

                int interval = ScheduleSettings.GetInstance().DayInterval;
                string date = "*/" + interval.ToString();

                retval = string.Format(CRON_FORMAT, second, minute, hour, date, "*", "?", "").Trim();
            }
            else if (ScheduleSettings.GetInstance().ScheduleType == DAY)
            {
                string week = string.Empty;
                week += ScheduleSettings.GetInstance().ExecMonday ? "MON," : "";
                week += ScheduleSettings.GetInstance().ExecTuesday ? "TUE," : "";
                week += ScheduleSettings.GetInstance().ExecWendnesday ? "WED," : "";
                week += ScheduleSettings.GetInstance().ExecThursday ? "THU," : "";
                week += ScheduleSettings.GetInstance().ExecFriday ? "FRI," : "";
                week += ScheduleSettings.GetInstance().ExecSurtarday ? "SAT," : "";
                week += ScheduleSettings.GetInstance().ExecSunday ? "SUN," : "";
                week = week.TrimEnd(',');

                DateTime time = Convert.ToDateTime(ScheduleSettings.GetInstance().ExecTimeDay);

                string second = time.Second.ToString();
                string minute = time.Minute.ToString();
                string hour = time.Hour.ToString();

                retval = string.Format(CRON_FORMAT, second, minute, hour, "?", "*", week, "").Trim();
            }

            return retval;
        }

        #region Quartzによって実行されるジョブの本体
        /// <summary>
        /// Quartzによって実行されるジョブの本体
        /// </summary>
        public class ScheduleJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                Dictionary<string, object> args = new Dictionary<string, object>();
                args[KEY_EXEC_MACHINE_LEARNING] = SetupController.EXEC_MACHINE_LEARNING;

                Scheduling.GetInstance().ExecJob(args);
            }
        }
        #endregion
    }
}