using DocSearch.Models;
using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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


        // Get: Setup
        [HttpGet]
        public ActionResult Setup(SetupModel setupModel)
        {
            foreach(string crawlFolder in Settings.GetInstance().CrawlFolders)
            {
                setupModel.CrawlFolders += crawlFolder + Environment.NewLine;
            }

            return View(setupModel);
        }

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


        /// <summary>
        /// クロール開始
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        [HttpPost]
        public void StartCrawlAsync(SetupModel setupModel)
        {
            AsyncManager.OutstandingOperations.Increment();

            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);

                AsyncManager.Parameters.Add("result", setupModel); // keyの名前を"result"にする

                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        /// <summary>
        /// クロール完了
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        public ActionResult StartCrawlCompleted(SetupModel result) // AsyncManager.Parameters.Add()で指定したkeyの名称を引数名に指定すると、値を受け取れる。
        {
            result.Message = "クロールが完了しました。";

            return RedirectToAction("Setup", "Setup", result);
        }


        /// <summary>
        /// 機械学習の実行開始
        /// </summary>
        /// <param name="setupModel"></param>
        [HttpPost]
        public void StartMachineLearningAsync(SetupModel setupModel)
        {
            AsyncManager.OutstandingOperations.Increment();

            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);

                AsyncManager.Parameters.Add("result", setupModel); // keyの名前を"result"にする

                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        /// <summary>
        /// 機械学習の完了
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public ActionResult StartMachineLearningCompleted(SetupModel result)
        {
            result.Message = "関連語学習が完了しました。";

            return RedirectToAction("Setup", "Setup", result);
        }
    }
}