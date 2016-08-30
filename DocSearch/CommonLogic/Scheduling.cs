using DocSearch.hubs;
using DocSearch.Models;
using FolderCrawler.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// スケジューリング
    /// </summary>
    public static class Scheduling
    {
        public const string TYPE = "SCHEDULING";

        public const string MESSAGE_SAVED = "保存しました";

        /// <summary>
        /// スケジューリング設定のセット
        /// </summary>
        /// <param name="setupModel"></param>
        public static void SetSchedule(string[] args)
        {
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

        }

        /// <summary>
        /// スケジュール設定の取得
        /// </summary>
        /// <param name="setupModel"></param>
        public static void GetSchedule(SetupModel setupModel)
        {
            setupModel.CronString = ScheduleSettings.GetInstance().CronString;
            setupModel.DayInterval = ScheduleSettings.GetInstance().DayInterval;
            setupModel.ExecFriday = ScheduleSettings.GetInstance().ExecFriday;
            setupModel.ExecMonday = ScheduleSettings.GetInstance().ExecMonday;
            setupModel.ExecSunday = ScheduleSettings.GetInstance().ExecSunday;
            setupModel.ExecSurtarday = ScheduleSettings.GetInstance().ExecSurtarday;
            setupModel.ExecThursday = ScheduleSettings.GetInstance().ExecThursday;
            setupModel.ExecTime = ScheduleSettings.GetInstance().ExecTimeDaily;
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
        public static void SendMessage(string message, string connectionID)
        {
            ComHub.SendMessageToTargetClient(TYPE, message, new string[0], connectionID);
        }

        /// <summary>
        /// int型への変換
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static int SafeConvInt(string val)
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
        private static bool SaveConvBool(string val)
        {
            bool retval = false;
            bool.TryParse(val, out retval);

            return retval;
        }
    }
}