using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace DocSearch.hubs
{
    public class ProgressHub : Hub
    {
        public delegate void BrowserMessage(string msg, string arg1, string arg2);
        public static event BrowserMessage CatchBrowserMessage;

        /// <summary>
        /// ブラウザへのデータ送信
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="count"></param>
        /// <param name="arg1"></param>
        public static void SendMessage(string msg, int count, string arg1)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.sendMessage(string.Format(msg), count, arg1);
        }

        /// <summary>
        /// ブラウザからのメッセージを取得するメソッド
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void GetMessage(string msg, string arg1, string arg2)
        {
            CatchBrowserMessage?.Invoke(msg, arg1, arg2);
        }
    }
}