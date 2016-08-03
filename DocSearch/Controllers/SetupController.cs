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

        SendProgressRate _crawlProgress = new SendProgressRate();

        /// <summary>
        /// クロール中のメッセージ
        /// </summary>
        private string MESSAGE_CRAWLING = "クロール中です...";
        /// <summary>
        /// クロール完了時のメッセージ
        /// </summary>
        private string MESSAGE_CRAWL_FINISHED = "クロールが完了しました。";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetupController()
        {
            // 処理中にブラウザに送るメッセージ
            _crawlProgress.Message = MESSAGE_CRAWLING;
            _crawlProgress.MessageNoProgressRate = MESSAGE_CRAWLING;
            _crawlProgress.MessageFinished = MESSAGE_CRAWL_FINISHED;

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

        /// <summary>
        /// 非同期処理完了メソッドに渡すパラメータを追加する
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        private void AddValue(string keyName, SetupModel value)
        {
            if (AsyncManager.Parameters.ContainsKey(keyName))
                AsyncManager.Parameters[keyName] = value;
            else
                AsyncManager.Parameters.Add(keyName, value);
        }

        #region クロール処理実行
        /// <summary>
        /// クロール開始
        /// </summary>
        /// <param name="execMachineLearning"></param>
        /// <param name="progressBarID"></param>
        private void StartCrawl(string execMachineLearning, string progressBarID)
        {
            EventHandler handler = (sender, e) =>
            {
                if (execMachineLearning == EXEC_MACHINE_LEARNING)
                    TrainingDataManager.GetInstance().StartTraining();

                _crawlProgress.ProcessFinished(MESSAGE_CRAWL_FINISHED);
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
        /// <param name="setupModel"></param>
        [HttpPost]
        [AsyncTimeout(21600000)]
        public void StartMachineLearningAsync(SetupModel setupModel)
        {
            AsyncManager.OutstandingOperations.Increment();

            Task.Run(() =>
            {
                TrainingDataManager.GetInstance().StartTraining();

                AddValue("result", setupModel);  // ① keyを"result"にすると... → ②へ

                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        /// <summary>
        /// 機械学習の完了
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public ActionResult StartMachineLearningCompleted(SetupModel result) // ② AsyncManager.Parameters.Add()で指定したkeyの名称を引数名に指定すると、値を受け取れる。
        {
            result.Message = "関連語学習が完了しました。";

            return RedirectToAction("Setup", "Setup", result);
        }
        #endregion
    }
}