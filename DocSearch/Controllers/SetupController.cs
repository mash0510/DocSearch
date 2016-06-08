using DocSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocSearch.Controllers
{
    public class SetupController : Controller
    {
        // POST: Setup
        [HttpGet]
        public ActionResult Setup(SetupModel setupModel)
        {

            return View();
        }

        /// <summary>
        /// クロール先のフォルダ指定
        /// </summary>
        /// <param name="setupModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetupCrawlFolder(SetupModel setupModel)
        {
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