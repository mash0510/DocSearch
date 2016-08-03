using DocSearch.hubs;
using FolderCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// サーバー側の重たい処理の進捗率をブラウザ側に知らせるクラス
    /// </summary>
    public class SendProgressRate
    {
        /// <summary>
        /// タイプ
        /// </summary>
        public const string TYPE = "PROGRESS_BAR";

        /// <summary>
        /// 進捗率取得のインターバル時間（ms）
        /// </summary>
        private const int PROGRESS_INTERVAL = 500;

        /// <summary>
        /// 完了時の進捗率
        /// </summary>
        public const int PROGRESS_COMPLETED = 100;

        /// <summary>
        /// 進捗率取得タイマー
        /// </summary>
        TimeElapse _progressRateTimer = new TimeElapse();

        /// <summary>
        /// 直近の進捗率
        /// </summary>
        private int _prevRate = 0;

        /// <summary>
        /// 進捗率と共にブラウザ側に送るメッセージの設定と取得
        /// </summary>
        public string Message
        {
            set;
            get;
        }

        /// <summary>
        /// 全体のドキュメント数が取得できない状態で、進捗率を算出できないときにブラウザに表示するメッセージ
        /// </summary>
        public string MessageNoProgressRate
        {
            set;
            get;
        }

        /// <summary>
        /// 完了時にブラウザに表示するメッセージ
        /// </summary>
        public string MessageFinished
        {
            set;
            get;
        }

        /// <summary>
        /// 進捗率更新先のプログレスバーID
        /// </summary>
        public string ProgressBarID
        {
            set;
            get;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SendProgressRate()
        {
            _progressRateTimer.Elapsed += _progressRateTimer_Elapsed;
        }

        /// <summary>
        /// 進捗率取得ポーリング処理の開始
        /// </summary>
        public void Start()
        {
            _progressRateTimer.TimerStart(PROGRESS_INTERVAL);
        }

        /// <summary>
        /// 進捗率取得ポーリング処理の停止
        /// </summary>
        public void Stop()
        {
            _progressRateTimer.TimerStop();
        }

        /// <summary>
        /// 処理完了時に、ブラウザ側に進捗率100%の数字を送る
        /// </summary>
        /// <param name="message">ブラウザ側に送るメッセージ</param>
        public void ProcessFinished(string message)
        {
            string[] args = { "100", ProgressBarID };

            ComHub.SendMessage(TYPE, message, args);
        }

        /// <summary>
        /// 進捗率の取得
        /// </summary>
        /// <returns></returns>
        protected virtual int GetProgressRate()
        {
            return 0;
        }


        /// <summary>
        /// PROGRESS_INTERVALで設定した時間の経過毎に呼ばれるメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _progressRateTimer_Elapsed(object sender, EventArgs e)
        {
            int rate = GetProgressRate();

            // 直近の進捗率と同じ値なら送信しない。
            if (_prevRate == rate)
                return;

            string mes = Message;
            if (rate == (int)CommonParameters.NO_TOTAL_DOCUMENTS)
            {
                mes = MessageNoProgressRate;
            }
            else if (rate == PROGRESS_COMPLETED)
            {
                mes = MessageFinished;
            }

            string[] args = { rate.ToString(), ProgressBarID };

            // ブラウザ側に進捗率を通知する
            ComHub.SendMessage(TYPE, mes, args);
        }
    }
}