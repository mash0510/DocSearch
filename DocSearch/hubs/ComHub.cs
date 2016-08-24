using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace DocSearch.hubs
{
    public class ComHub : Hub
    {
        public delegate void BrowserMessage(string type, string mes, string[] args);
        public static event BrowserMessage CatchBrowserMessage;

        private static string connectionID;

        /// <summary>
        /// 全ブラウザへのデータ送信
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="count"></param>
        /// <param name="arg1"></param>
        public static void SendMessage(string type, string msg, string[] args)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ComHub>();
            hubContext.Clients.All.sendMessage(string.Format(type), string.Format(msg), args);
        }

        /// <summary>
        /// 呼び出し元へのみメッセージを返す
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void SendMessageToCaller(string type, string msg, string[] args)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ComHub>();
            hubContext.Clients.Client(connectionID).sendMessage(string.Format(type), string.Format(msg), args);
        }

        /// <summary>
        /// ブラウザからのメッセージを取得するメソッド
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void GetMessage(string type, string msg, string[] args)
        {
            connectionID = Context.ConnectionId;
            CatchBrowserMessage?.Invoke(type, msg, args);
        }
    }
}