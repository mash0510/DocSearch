using DocSearch.Models;
using DocSearch.hubs;
using FolderCrawler;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using DocSearch.CommonLogic;

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
        private const string EXEC_MACHINE_LEARNING = "1";

        SendProgressRate _crawlProgress = new ProgressRateCrawl();
        SendProgressRate _machineLearningProgress = new ProgressRateMachineLearning();

        /// <summary>
        /// 処理開始ボタン押下～実際の処理開始までの間に表示するメッセージ
        /// </summary>
        private string MESSAGE_PREPAREING = "処理開始の準備中です。";

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

            ComHub.CatchBrowserMessage -= ProgressHub_CatchBrowserMessage;
            ComHub.CatchBrowserMessage += ProgressHub_CatchBrowserMessage;
        }
        
        /// <summary>
        /// ブラウザからのメッセージ受信
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        private void ProgressHub_CatchBrowserMessage(string type, string msg, string[] args)
        {
            if (type == SendProgressRate.TYPE)
            {
                switch (msg)
                {
                    case "CancelCrawl":
                        CrawlerManager.GetInstance().Stop();
                        break;
                    case "StartCrawl":
                        StartCrawl(args[0], args[1]);
                        break;
                    case "StartMachineLearning":
                        StartMachineLearning(args[0]);
                        break;
                    case "CancelMachineLearning":
                        TrainingDataManager.GetInstance().KillMachineLearningProcess();
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

            setupModel.Message = "保存が完了しました。";

            return RedirectToAction("Setup", "Setup", setupModel);
        }
        #endregion

        #region クロール処理実行
        /// <summary>
        /// クロール開始
        /// </summary>
        /// <param name="execMachineLearning"></param>
        /// <param name="progressBarID"></param>
        /// <param name="machineLearningProgressBarID"></param>
        private void StartCrawl(string progressBarID, string machineLearningProgressBarID)
        {
            if (!CrawlerManager.GetInstance().IsAllCrawlFinished || TrainingDataManager.GetInstance().IsProcessingTrainingDataGeneration)
                return;

            TrainingDataManager.TrainingDataGenerateFinishedDelegate handler = (isCanceled) =>
            {
                int rate = SendProgressRate.PROGRESS_COMPLETED;
                string message = MESSAGE_CRAWL_FINISHED;
                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = MESSAGE_CRAWL_CANCELED;
                }

                _crawlProgress.SendRate(message, rate);
            };

            TrainingDataManager.GetInstance().TrainingDataGenerateFinished -= handler;
            TrainingDataManager.GetInstance().TrainingDataGenerateFinished += handler;

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
            if (TrainingDataManager.GetInstance().IsProcessingMachineLearning)
                return;

            TrainingDataManager.MachineLearningFinishedDelegate handler = (isCanceled) =>
            {
                int rate = SendProgressRate.PROGRESS_COMPLETED;
                string message = MESSAGE_LEARNING_FINISHED;

                if (isCanceled)
                {
                    rate = SendProgressRate.PROGRESS_RATE_CANCELED;
                    message = MESSAGE_LEARNING_CANCELED;
                }

                _machineLearningProgress.SendRate(message, rate);
            };

            TrainingDataManager.GetInstance().MachineLearningFinished -= handler;
            TrainingDataManager.GetInstance().MachineLearningFinished += handler;

            _machineLearningProgress.ProgressBarID = progressBarID;
            _machineLearningProgress.Start();

            TrainingDataManager.GetInstance().StartTraining();
        }
        #endregion
    }
}