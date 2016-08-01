﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace DocSearch.hubs
{
    public class ProgressHub : Hub
    {
        /// <summary>
        /// ブラウザへのデータ送信
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="count"></param>
        public static void SendMessage(string msg, int count)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.sendMessage(string.Format(msg), count);
        }

        /// <summary>
        /// ブラウザからのメッセージを取得するメソッド
        /// </summary>
        /// <param name="msg"></param>
        public void GetMessage(string msg)
        {
            // 今現在は未使用。
        }
    }
}