using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.Models
{
    /// <summary>
    /// 設定画面のモデル
    /// </summary>
    public class SetupModel
    {
        /// <summary>
        /// クロール先のフォルダ
        /// </summary>
        public string CrawlFolders { set; get; }

        /// <summary>
        /// クロール後の機械学習を実行するかどうか（0：実行しない、1：実行する）
        /// </summary>
        public int ExecMachineLearning { set; get; }


        private bool _processFinished = false;
        /// <summary>
        /// 処理完了したかどうか
        /// </summary>
        public bool ProcessFinished
        {
            get { return _processFinished; }
        }

        private string _message = string.Empty;
        /// <summary>
        /// 応答メッセージ
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _processFinished = true;
                _message = value;
            }
        }
    }
}