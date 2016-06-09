using DocSearch.Models;
using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocSearch.Controllers
{
    public class SetupController : Controller
    {
        /// <summary>
        /// 区切り文字
        /// </summary>
        private char[] delimiter = { ',' };

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
        /// 
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StartCrawl(SetupModel setupModel)
        {

            return RedirectToAction("Setup", "Setup", setupModel);
        }
    }
}