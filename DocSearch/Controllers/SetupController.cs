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
        private const int EXEC_MACHINE_LEARNING = 1;

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
            _crawlProgress.Message = "クロール中です...";
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
        /// <param name="setupModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AsyncTimeout(21600000)]
        public void StartCrawlAsync(SetupModel setupModel)
        {
            AsyncManager.OutstandingOperations.Increment();

            TrainingDataManager.GetInstance().TrainingDataGenerateFinished += (sender, e) =>
            {
                if (setupModel.ExecMachineLearning == EXEC_MACHINE_LEARNING)
                    TrainingDataManager.GetInstance().StartTraining();

                AddValue("result", setupModel);  // ① keyを"result"にすると... → ②へ

                AsyncManager.OutstandingOperations.Decrement();
            };

            CrawlerManager.GetInstance().Start();
            _crawlProgress.Start();
        }

        /// <summary>
        /// クロール完了
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        public ActionResult StartCrawlCompleted(SetupModel result) // ② AsyncManager.Parameters.Add()で指定したkeyの名称を引数名に指定すると、値を受け取れる。
        {
            _crawlProgress.ProcessFinished(MESSAGE_CRAWL_FINISHED);

            result.Message = MESSAGE_CRAWL_FINISHED;

            return RedirectToAction("Setup", "Setup", result);
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