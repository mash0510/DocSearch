using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FolderCrawler.Setting
{
    /// <summary>
    /// スケジュールセッティングの読み込み・保存
    /// </summary>
    public class ScheduleSettings
    {
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
        /// 日毎を選択したときの実行時間
        /// </summary>
        public string ExecTimeDaily
        {
            get;
            set;
        }

        /// <summary>
        /// 曜日を選択したときの実行時間
        /// </summary>
        public string ExecTimeDay
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

        private static ScheduleSettings _self = new ScheduleSettings();

        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <returns></returns>
        public static ScheduleSettings GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ScheduleSettings()
        {

        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            CommonLogic.SafeCreateDirectory(Path.GetDirectoryName(CommonParameters.SchedulingFileFullPath));

            //ファイルを開く（UTF-8 BOM無し）
            StreamWriter sw = new StreamWriter(CommonParameters.SchedulingFileFullPath, false, new UTF8Encoding(false));

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ScheduleSettings));

                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, _self);
            }
            catch (Exception ex)
            {
                // ログ出力処理を後ほど記述
            }
            finally
            {
                //閉じる
                sw.Close();
            }
        }

        /// <summary>
        /// 設定読み込み
        /// </summary>
        public void Load()
        {
            if (!File.Exists(CommonParameters.SchedulingFileFullPath))
                return;

            //ファイルを開く
            StreamReader sr = new StreamReader(CommonParameters.SchedulingFileFullPath, new UTF8Encoding(false));

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                //XMLファイルから読み込み、逆シリアル化する
                _self.RestoreSettings((ScheduleSettings)serializer.Deserialize(sr));
            }
            catch (Exception ex)
            {
                // ログ出力処理を後ほど記述
            }
            finally
            {
                //閉じる
                sr.Close();
            }
        }

        /// <summary>
        /// 読み込んだ設定を復元
        /// </summary>
        /// <param name="parameters"></param>
        public void RestoreSettings(ScheduleSettings parameters)
        {
            _self.ScheduleType = parameters.ScheduleType;
            _self.OneTimeDateTime = parameters.OneTimeDateTime;
            _self.ExecTimeDaily = parameters.ExecTimeDaily;
            _self.ExecTimeDay = parameters.ExecTimeDay;
            _self.DayInterval = parameters.DayInterval;
            _self.ExecFriday = parameters.ExecFriday;
            _self.ExecMonday = parameters.ExecMonday;
            _self.ExecSunday = parameters.ExecSunday;
            _self.ExecSurtarday = parameters.ExecSurtarday;
            _self.ExecThursday = parameters.ExecThursday;
            _self.ExecTuesday = parameters.ExecTuesday;
            _self.ExecWendnesday = parameters.ExecWendnesday;
            _self.CronString = parameters.CronString;
        }
    }
}
