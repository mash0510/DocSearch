using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace DocSearch.hubs
{
    public class ComHub : Hub
    {
        public delegate void BrowserMessage(string type, string mes, string[] args);
        public static event BrowserMessage CatchBrowserMessage;

        /// <summary>
        /// ブラウザへのデータ送信
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
        /// ブラウザからのメッセージを取得するメソッド
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void GetMessage(string type, string msg, string[] args)
        {
            CatchBrowserMessage?.Invoke(type, msg, args);
        }
    }
}