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
        public delegate void BrowserMessage(string type, string mes, string[] args, string connectionID);
        public static event BrowserMessage CatchBrowserMessage;

        /// <summary>
        /// 全ブラウザへのデータ送信
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="count"></param>
        /// <param name="arg1"></param>
        public static void SendMessageToAll(string type, string msg, string[] args)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ComHub>();
            hubContext.Clients.All.sendMessage(string.Format(type), string.Format(msg), args);
        }

        /// <summary>
        /// 引数connectionIDで示されるクライアントに対してのみメッセージを返す
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        /// <param name="connectionID"></param>
        public static void SendMessageToTargetClient(string type, string msg, string[] args, string connectionID)
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
            CatchBrowserMessage?.Invoke(type, msg, args, Context.ConnectionId);
        }
    }
}