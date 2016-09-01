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

        /// <summary>
        /// 処理開始ボタン押下～実際の処理開始までの間に表示するメッセージ
        /// </summary>
        private string MESSAGE_PREPAREING = "処理開始の準備中です。";

        /// <summary>
        /// クロール未実行
        /// </summary>
        private string MESSAGE_NOT_CRAWLED = "クロールが実行されていません";
        /// <summary>
        /// クロール中のメッセージ
        /// </summary>
        private string MESSAGE_CRAWLING = "クロール中です...";
        /// <summary>
        /// クロール完了時のメッセージ
        /// </summary>
        private string MESSAGE_CRAWL_FINISHED = "クロールが完了しました。";
        /// <summary>
        /// クロール処理キャンセル時のメッセージ
        /// </summary>
        private string MESSAGE_CRAWL_CANCELED = "クロール処理がキャンセルされました。";

        /// <summary>
        /// 機械学習未実行
        /// </summary>
        private string MESSAGE_NOT_LEARNED = "機械学習が実行されていません。";
        /// <summary>
        /// 機械学習実行中のメッセージ
        /// </summary>
        private string MESSAGE_LEARNING = "機械学習実行中です...";
        /// <summary>
        /// 機械学習完了時のメッセージ
        /// </summary>
        private string MESSAGE_LEARNING_FINISHED = "機械学習が完了しました。";
        /// <summary>
        /// 機械学習処理キャンセル時のメッセージ
        /// </summary>
        private string MESSAGE_LEARNING_CANCELED = "機械学習処理がキャンセルされました。";

        /// <summary>
        /// 保存完了メッセージ
        /// </summary>
        private string MESSAGE_SAVED = "保存が完了しました";

        // ブラウザからののメッセージ受信イベントを登録したかどうか
        // （二重登録防止用のフラグ）
        private static bool isCatchBrowerMessageRegistered = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetupController()
        {
            // 処理中にブラウザに送るメッセージ
            _crawlProgress.Message = MESSAGE_CRAWLING;
            _crawlProgress.MessageNoProgressRate = MESSAGE_CRAWLING;
            _crawlProgress.MessageFinished = MESSAGE_CRAWL_FINISHED;

            _machineLearningProgress.Message = MESSAGE_LEARNING;
            _machineLearningProgress.MessageNoProgressRate = MESSAGE_LEARNING;
            _machineLearningProgress.MessageFinished = MESSAGE_LEARNING_FINISHED;

            if (!isCatchBrowerMessageRegistered)
            {
                ComHub.CatchBrowserMessage += ProgressHub_CatchBrowserMessage;
                isCatchBrowerMessageRegistered = true;
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
            if (type == SendProgressRate.TYPE || type == History.TYPE || type == Scheduling.TYPE)
            {
                switch (msg)
                {
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
        [HttpPost]
        public ActionResult SetupCrawlFolder(SetupModel setupModel)
        {
            string pathes = setupModel.CrawlFolders.Replace(Environment.NewLine, ",");
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

            setupModel.Message = MESSAGE_SAVED;

            return RedirectToAction("Setup", "Setup", setupModel);
        }
        #endregion

        #region スケジューリング設定
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
                Scheduling.GetInstance().SendMessage(Scheduling.MESSAGE_CRON_STRING_ERROR, connectionID);
                ScheduleSettings.GetInstance().Restore();
                return;
            }
            catch (SchedulerException ex)
            {
                // 一度もスケジュール実行されないような設定（過去日を指定したなど）の場合は、エラーメッセージを出す。
                Scheduling.GetInstance().SendMessage(Scheduling.MESSAGE_NO_EXECUTE_DATE, connectionID);
            }

            ScheduleSettings.GetInstance().Save();

            Scheduling.GetInstance().SendMessage(Scheduling.MESSAGE_SAVED, connectionID);
        }
        #endregion

        #region クロール・機械学習が実行済みかどうかの確認
        /// <summary>
        /// クロールが実行済みかどうかを返す
        /// </summary>
        /// <param name="progressBarID"></param>
        private void CheckCrawlExecuted(string progressBarID)
        {
            string message = MESSAGE_NOT_CRAWLED;
            int rate = 0;

            if (History.CrawlHistory.HistoryData.Count > 0)
            {
                message = MESSAGE_CRAWL_FINISHED;
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
            string message = MESSAGE_NOT_LEARNED;
            int rate = 0;

            if (History.CrawlHistory.HistoryData.Count > 0)
            {
                message = MESSAGE_LEARNING_FINISHED;
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
                string message = MESSAGE_CRAWL_FINISHED;
                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = MESSAGE_CRAWL_CANCELED;
                }

                _crawlProgress.SendRate(message, rate);

                // 履歴保存
                CrawlHistoryInfo history = History.CrawlHistory.CreateHistoryInstance();
                history.allFileNum = CrawlerManager.GetInstance().TotalDocuments;
                history.fileNum = fileCount;
                history.isCanceled = isCanceled;

                History.CrawlHistory.Add(history);
                History.CrawlHistory.Save();

                if (execMachineLearning == EXEC_MACHINE_LEARNING)
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
                string message = MESSAGE_LEARNING_FINISHED;

                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = MESSAGE_LEARNING_CANCELED;
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