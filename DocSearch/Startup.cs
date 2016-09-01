using DocSearch.CommonLogic;
using FolderCrawler;
using FolderCrawler.Setting;
using Microsoft.Owin;
using Owin;
using Quartz;
using System;

[assembly: OwinStartupAttribute(typeof(DocSearch.Startup))]
namespace DocSearch
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Web.Configからのパラメータ読み込み
            InitParameters.Initialize();

            // 設定ファイルのロード
            Settings.GetInstance().LoadSettings();
            ScheduleSettings.GetInstance().Load();

            // スケジューリングの設定
            try
            {
                Scheduling.GetInstance().SetQuartz();
            }
            catch
            {
                // ここで発生した例外は無視する。ログ出力を後日実装
            }

            app.MapSignalR();

            // 保存したデータの読み込み
            InitParameters.Load();
        }
    }
}
