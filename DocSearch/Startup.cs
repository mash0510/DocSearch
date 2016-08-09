using DocSearch.CommonLogic;
using FolderCrawler;
using Microsoft.Owin;
using Owin;

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

            app.MapSignalR();

            // 文書IDのロード
            IDDictionary.GetInstanse().LoadAsync();
        }
    }
}
