using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 指定した時間が経過したらイベントを発生させるクラス
    /// </summary>
    public class TimeElapse
    {
        /// <summary>
        /// ワーカースレッドの稼働時間を計測するタイマー
        /// </summary>
        private Timer _timer = null;

        /// <summary>
        /// タイマー実行中かどうか
        /// </summary>
        public bool IsTimerStarted { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimeElapse()
        {
            IsTimerStarted = false;
        }

        /// <summary>
        /// 経過時間の計測スタート
        /// </summary>
        /// <param name="duration"></param>
        public void TimerStart(int duration)
        {
            if (_timer == null)
            {
                _timer = new Timer(new TimerCallback(ThreadingTimerCallback), null, duration, duration);
                IsTimerStarted = true;

                return;
            }

            if (!IsTimerStarted)
            {
                _timer.Change(duration, duration);
                IsTimerStarted = true;
            }
        }

        /// <summary>
        /// 経過時間の計測終了
        /// </summary>
        public void TimerStop()
        {
            if (_timer == null)
            {
                IsTimerStarted = false;
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            IsTimerStarted = false;
        }

        /// <summary>
        /// 時間経過後に発生させるイベント
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// 指定した計測時間経過後の処理
        /// </summary>
        /// <param name="state"></param>
        private void ThreadingTimerCallback(object state)
        {
            Elapsed?.Invoke(this, new EventArgs());
        }
    }
}
