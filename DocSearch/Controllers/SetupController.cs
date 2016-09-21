using DocSearch.Models;
using DocSearch.hubs;
using FolderCrawler;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using DocSearch.CommonLogic;
using FolderCrawler.History;
using FolderCrawler.Setting;
using Quartz;
using DocSearch.Resources;

namespace DocSearch.Controllers
{
    public class SetupController : AsyncController
    {
        /// <summary>
        /// 区切り文字
        /// </summary>
        private char[] delimiter = { ',' };

        /// <summary>
        /// 「機械学習をする」を示す値
        /// </summary>
        internal const string EXEC_MACHINE_LEARNING = "1";

        SendProgressRate _crawlProgress = new ProgressRateCrawl();
        SendProgressRate _machineLearningProgress = new ProgressRateMachineLearning();

        //  イベントを登録したかどうか
        // （二重登録防止用のフラグ）
        private static bool isEventRegistered = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetupController()
        {
            // 処理中にブラウザに送るメッセージ
            _crawlProgress.Message = Message.CRAWLING;
            _crawlProgress.MessageNoProgressRate = Message.CRAWLING;
            _crawlProgress.MessageFinished = Message.CRAWL_FINISHED;

            _machineLearningProgress.Message = Message.LEARNING;
            _machineLearningProgress.MessageNoProgressRate = Message.LEARNING;
            _machineLearningProgress.MessageFinished = Message.LEARNING_FINISHED;

            if (!isEventRegistered)
            {
                ComHub.CatchBrowserMessage += ProgressHub_CatchBrowserMessage;
                Scheduling.GetInstance().ExecJob += SetupController_ExecJob;

                isEventRegistered = true;
            }
        }

        /// <summary>
        /// ブラウザからのメッセージ受信
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        /// <param name="connectionID"></param>
        private void ProgressHub_CatchBrowserMessage(string type, string msg, string[] args, string connectionID)
        {
            if (type == SendProgressRate.TYPE || type == History.TYPE || type == Scheduling.TYPE || type == Settings.TYPE)
            {
                switch (msg)
                {
                    case "SetupCrawlFolders":
                        SetupCrawlFolder(args[0], connectionID);
                        break;
                    case "CancelCrawl":
                        CrawlerManager.GetInstance().Stop();
                        break;
                    case "StartCrawl":
                        StartCrawl(args[0], args[1], args[2]);
                        break;
                    case "StartMachineLearning":
                        StartMachineLearning(args[0]);
                        break;
                    case "CancelMachineLearning":
                        MachineLearningManager.GetInstance().KillMachineLearningProcess();
                        break;
                    case "IsCrawlExecuted":
                        CheckCrawlExecuted(args[0]);
                        break;
                    case "IsWord2VecLearned":
                        CheckWord2VecExecuted(args[0]);
                        break;
                    case "GetHistoryData":
                        GetHistory(args[0], connectionID);
                        break;
                    case "SaveScheduling":
                        SaveSchedule(args, connectionID);
                        break;
                }
            }
        }

        // Get: Setup
        [HttpGet]
        public ActionResult Setup(SetupModel setupModel)
        {
            foreach(string crawlFolder in Settings.GetInstance().CrawlFolders)
            {
                setupModel.CrawlFolders += crawlFolder.Replace(@"\\", @"\") + Environment.NewLine;
            }

            Scheduling.GetInstance().GetSchedule(setupModel);

            return View(setupModel);
        }

        #region クロール先フォルダの指定
        /// <summary>
        /// クロール先のフォルダ指定
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        public void SetupCrawlFolder(string crawlFolders, string connectionID)
        {
            string pathes = crawlFolders.Replace("\n", ",");
            string[] pathesArray = pathes.Split(delimiter);

            Settings.GetInstance().CrawlFolders.Clear();

            foreach(string path in pathesArray)
            {
                string pathTrimed = path.Trim();
                if (pathTrimed == string.Empty)
                    continue;

                Settings.GetInstance().CrawlFolders.Add(pathTrimed);
            }

            Settings.GetInstance().SaveSettings();

            ComHub.SendMessageToTargetClient(Settings.TYPE, Message.SAVED, new string[0], connectionID);
        }
        #endregion

        #region スケジューリング
        /// <summary>
        /// スケジューリング設定の保存
        /// </summary>
        /// <param name="args"></param>
        /// <param name="connectionID"></param>
        public void SaveSchedule(string[] args, string connectionID)
        {
            try
            {
                Scheduling.GetInstance().SetSchedule(args);
            }
            catch (FormatException ex)
            {
                // 詳細設定選択時にcron文字列が不正だったら、設定保存させない。
                Scheduling.GetInstance().SendMessage(Message.CRON_STRING_ERROR, connectionID);
                ScheduleSettings.GetInstance().Restore();
                return;
            }
            catch (SchedulerException ex)
            {
                // 一度もスケジュール実行されないような設定（過去日を指定したなど）の場合は、エラーメッセージを出す。
                Scheduling.GetInstance().SendMessage(Message.NO_EXECUTE_DATE, connectionID);
            }

            ScheduleSettings.GetInstance().Save();

            Scheduling.GetInstance().SendMessage(Message.SAVED, connectionID);
        }

        /// <summary>
        /// スケジュールイベントハンドラ
        /// </summary>
        /// <param name="args"></param>
        private void SetupController_ExecJob(System.Collections.Generic.Dictionary<string, object> args)
        {
            string arg = args[Scheduling.KEY_EXEC_MACHINE_LEARNING].ToString();

            StartCrawl("#crawlProgressBar", "#machineLearningProgressBar", arg);
        }
        #endregion

        #region クロール・機械学習が実行済みかどうかの確認
        /// <summary>
        /// クロールが実行済みかどうかを返す
        /// </summary>
        /// <param name="progressBarID"></param>
        private void CheckCrawlExecuted(string progressBarID)
        {
            string message = Message.NOT_CRAWLED;
            int rate = 0;

            if (History.CrawlHistory.HistoryData.Count > 0)
            {
                message = Message.CRAWL_FINISHED;
                rate = SendProgressRate.PROGRESS_COMPLETED;
            }

            _crawlProgress.ProgressBarID = progressBarID;
            _crawlProgress.SendRate(message, rate);
        }

        /// <summary>
        /// word2vecの機械学習が実行済みかどうかを返す
        /// </summary>
        /// <param name="progressBarID"></param>
        private void CheckWord2VecExecuted(string progressBarID)
        {
            string message = Message.NOT_LEARNED;
            int rate = 0;

            if (History.CrawlHistory.HistoryData.Count > 0)
            {
                message = Message.LEARNING_FINISHED;
                rate = SendProgressRate.PROGRESS_COMPLETED;
            }

            _machineLearningProgress.ProgressBarID = progressBarID;
            _machineLearningProgress.SendRate(message, rate);
        }
        #endregion

        #region 履歴取得
        /// <summary>
        /// 履歴の取得
        /// </summary>
        /// <param name="historyKind"></param>
        /// <param name="connectionID"></param>
        private void GetHistory(string historyKind, string connectionID)
        {
            History.SendHistory(historyKind, connectionID);
        }
        #endregion

        #region クロール処理実行
        /// <summary>
        /// クロール開始
        /// </summary>
        /// <param name="progressBarID"></param>
        /// <param name="machineLearningProgressBarID"></param>
        /// <param name="execMachineLearning"></param>
        private void StartCrawl(string progressBarID, string machineLearningProgressBarID, string execMachineLearning)
        {
            if (!CrawlerManager.GetInstance().IsAllCrawlFinished || !CrawlerManager.GetInstance().IsAllInsertFinished)
                return;

            CrawlerManager.DocCrawlDelegate handler = (sender, fileCount, isCanceled) =>
            {
                int rate = SendProgressRate.PROGRESS_COMPLETED;
                string message = Message.CRAWL_FINISHED;
                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = Message.CRAWL_CANCELED;
                }

                _crawlProgress.SendRate(message, rate);

                // 履歴保存
                CrawlHistoryInfo history = History.CrawlHistory.CreateHistoryInstance();
                history.allFileNum = CrawlerManager.GetInstance().TotalDocuments;
                history.fileNum = fileCount;
                history.isCanceled = isCanceled;

                History.CrawlHistory.Add(history);
                History.CrawlHistory.Save();

                if (!isCanceled && execMachineLearning == EXEC_MACHINE_LEARNING)
                {
                    // 機械学習を続けて実行する場合は、機械学習実行。
                    StartMachineLearning(machineLearningProgressBarID);
                }
            };

            CrawlerManager.GetInstance().AllDocInsertFinished -= handler;
            CrawlerManager.GetInstance().AllDocInsertFinished += handler;

            CrawlerManager.GetInstance().Start();

            _crawlProgress.ProgressBarID = progressBarID;
            _crawlProgress.Start();
        }
        #endregion

        #region 機械学習処理実行
        /// <summary>
        /// 機械学習の実行開始
        /// </summary>
        /// <param name="progressBarID"></param>
        private void StartMachineLearning(string progressBarID)
        {
            if (MachineLearningManager.GetInstance().IsProcessingMachineLearning)
                return;

            MachineLearningManager.MachineLearningFinishedDelegate handler = (isCanceled) =>
            {
                int rate = SendProgressRate.PROGRESS_COMPLETED;
                string message = Message.LEARNING_FINISHED;

                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = Message.LEARNING_CANCELED;
                }

                _machineLearningProgress.SendRate(message, rate);

                // 履歴保存
                Word2VecHistoryInfo history = History.Word2VecHistory.CreateHistoryInstance();
                history.isCanceled = isCanceled;

                History.Word2VecHistory.Add(history);
                History.Word2VecHistory.Save();
            };

            MachineLearningManager.GetInstance().MachineLearningFinished -= handler;
            MachineLearningManager.GetInstance().MachineLearningFinished += handler;

            _machineLearningProgress.ProgressBarID = progressBarID;
            _machineLearningProgress.Start();

            MachineLearningManager.GetInstance().StartTraining();
        }
        #endregion
    }
}