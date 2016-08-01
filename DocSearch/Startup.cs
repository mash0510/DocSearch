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
            // 設定ファイルのロード
            Settings.GetInstance().LoadSettings();

            app.MapSignalR();

            // 文書IDのロード
            IDDictionary.GetInstanse().LoadAsync();
        }
    }
}
