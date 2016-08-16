using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 機械学習クラス
    /// </summary>
    public class MachineLearningManager
    {
        /// <summary>
        /// 機械学習処理を実行中かどうか
        /// </summary>
        public bool IsProcessingMachineLearning
        {
            get;
            private set;
        }

        /// <summary>
        /// 機械学習処理の進捗率
        /// </summary>
        public int MachineLearningProgressRate
        {
            get;
            private set;
        }

        /// <summary>
        /// 自身のインスタンス
        /// </summary>
        private static MachineLearningManager _self = new MachineLearningManager();

        /// <summary>
        /// インスタンスの取得
        /// </summary>
        /// <returns></returns>
        public static MachineLearningManager GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MachineLearningManager()
        {
        }

        /// <summary>
        /// word2vecにより生成された単語ベクトルファイルを、実際に使用するファイル名に変更
        /// </summary>
        public void ChangeVectorFileNameInUse()
        {
            string outputFile = CommonParameters.VectorFileNameFullPath;
            string newFileName = CommonParameters.VectorFileNameInUseFullPath;

            CommonLogic.FileBackup(outputFile, newFileName);
        }

        public delegate void MachineLearningFinishedDelegate(bool isCanceled);
        /// <summary>
        /// 機械学習終了時のイベント
        /// </summary>
        public event MachineLearningFinishedDelegate MachineLearningFinished;
        /// <summary>
        /// 機械学習処理を途中でキャンセルしたかどうか
        /// </summary>
        private bool _cancelMachineLearning = false;

        private Process mecabProgram = null;
        private Process word2vecProc = null;

        /// <summary>
        /// 訓練スタート
        /// </summary>
        public void StartTraining()
        {
            IsProcessingMachineLearning = true;

            if (File.Exists(CommonParameters.VectorFileNameFullPath))
                File.Delete(CommonParameters.VectorFileNameFullPath);

            bool result = true;
            _cancelMachineLearning = false;

            mecabProgram = new Process();
            word2vecProc = new Process();

            // 分かち書き
            mecabProgram.StartInfo.FileName = CommonParameters.MecabProgram;
            mecabProgram.StartInfo.Arguments = CommonParameters.TrainingDataFileFullPath + " -b 8192000 -Owakati -o " + CommonParameters.MeCabOutputFileName;
            result = mecabProgram.Start();

            mecabProgram.WaitForExit();

            mecabProgram.Close();
            mecabProgram.Dispose();

            Task.Run(() =>
            {
                // 機械学習
                word2vecProc.StartInfo.FileName = CommonParameters.Word2VecProgram;
                word2vecProc.StartInfo.Arguments = "-train " + CommonParameters.MeCabOutputFileName + " -output " + CommonParameters.VectorFileNameFullPath + " -cbow 0 -size 200 -window 10 -negative 0 -hs 1 -sample 1e-3 -threads 12 -binary 1";
                word2vecProc.StartInfo.UseShellExecute = false;
                word2vecProc.StartInfo.RedirectStandardOutput = true;
                word2vecProc.OutputDataReceived += Word2vecProc_OutputDataReceived;
                word2vecProc.StartInfo.RedirectStandardInput = false;
                word2vecProc.StartInfo.CreateNoWindow = true;

                result = word2vecProc.Start();
                word2vecProc.BeginOutputReadLine();

                word2vecProc.WaitForExit();

                MachineLearningFinished?.Invoke(_cancelMachineLearning);

                word2vecProc.Close();
                word2vecProc.Dispose();

                IsProcessingMachineLearning = false;

                MachineLearningFinished?.Invoke(_cancelMachineLearning);
                ChangeVectorFileNameInUse();
            });
        }

        /// <summary>
        /// word2vecの進捗率出力の識別子
        /// </summary>
        private const string WORD2VEC_PROGRESS_RATE_INDICATOR = "Progress:";
        /// <summary>
        /// word2vec出力データの分割文字
        /// </summary>
        private char[] delimiter = new char[] { ' ' };

        /// <summary>
        /// word2vecの出力内容読み取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Word2vecProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;

            MachineLearningProgressRate = GetProgressRate(data);
        }

        /// <summary>
        /// word2vecの出力から進捗率を取得する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int GetProgressRate(string data)
        {
            if (data == null)
                return 0;

            // "Progress:"という文字が出力されるまでは学習が開始していないので、0%を返す
            if (!data.Contains(WORD2VEC_PROGRESS_RATE_INDICATOR))
                return 0;

            string[] dataArray = data.Split(delimiter);
            if (dataArray.Length < 4)
                return 0;

            var rateData = dataArray.Where(output => output.Contains("%")).First<string>().TrimEnd(new char[] { '%' });

            double rate = 0;
            double.TryParse(rateData, out rate);

            int retval = Convert.ToInt32(Math.Ceiling(rate));

            return retval;
        }

        /// <summary>
        /// 機械学習プロセスを途中で強制終了する。
        /// </summary>
        public void KillMachineLearningProcess()
        {
            IsProcessingMachineLearning = false;

            if (word2vecProc == null)
                return;

            word2vecProc.Kill();

            _cancelMachineLearning = true;

            MachineLearningFinished?.Invoke(_cancelMachineLearning);
        }
    }
}
