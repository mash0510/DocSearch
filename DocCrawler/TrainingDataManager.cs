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
    /// 機械学習用トレーニングデータ管理クラス
    /// </summary>
    public class TrainingDataManager
    {
        /// <summary>
        /// 機械学習用データの最大サイズ(単位：バイト)
        /// </summary>
        private long _maxFileSize = CommonParameters.DefaultMaxTrainingFileSize;

        /// <summary>
        /// ファイル書き出しバッファサイズ
        /// </summary>
        private const int BUFFER = 1024;

        /// <summary>
        /// 訓練データ作成スレッドの停止
        /// </summary>
        private bool _stop = false;

        /// <summary>
        /// 機械学習用データの最大サイズの設定と取得
        /// </summary>
        public long MaxTrainingFileSiz
        {
            get { return this._maxFileSize; }
            set { this._maxFileSize = value; }
        }

        /// <summary>
        /// 経過時間測定インスタンス
        /// </summary>
        private TimeElapse _timeElapse = new TimeElapse();

        /// <summary>
        /// 自身のインスタンス
        /// </summary>
        private static TrainingDataManager _self = new TrainingDataManager();

        /// <summary>
        /// インスタンスの取得
        /// </summary>
        /// <returns></returns>
        public static TrainingDataManager GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private TrainingDataManager()
        {
            _timeElapse.Elapsed += _timeElapse_Elapsed;
        }

        /// <summary>
        /// 訓練データ生成スレッドの停止
        /// </summary>
        public void Stop()
        {
            this._stop = true;
        }

        /// <summary>
        /// ワーカースレッドの未処理時間経過後、ワーカースレッドを止める
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeElapse_Elapsed(object sender, EventArgs e)
        {
            this._stop = true;
            _timeElapse.TimerStop();
        }

        public event EventHandler TrainingDataGenerateFinished;

        /// <summary>
        /// 訓練データ生成処理
        /// </summary>
        public void StartTrainingDataGenerate()
        {
            this._stop = false;

            while (!CheckMaxSize())
            {
                if (QueueManager.GetInstance().DocInfoQueue.Count == 0)
                {
                    if (!_timeElapse.IsTimerStarted)
                        _timeElapse.TimerStart(CommonParameters.WorkerThreadStopDuration);

                    if (this._stop)
                        break;
                    else
                        continue;
                }

                _timeElapse.TimerStop();

                DocumentInfo docInfo = QueueManager.GetInstance().DocInfoQueue.Dequeue() as DocumentInfo;
                if (docInfo == null)
                    continue;

                GenerateTrainingData(docInfo);
            }

            TrainingDataGenerateFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 訓練データの作成
        /// </summary>
        private void GenerateTrainingData(DocumentInfo docInfo)
        {
            if (CheckMaxSize())
                return;

            string outputFile = CommonParameters.TrainingDataFileFullPath;
            FileStream fs = new FileStream(outputFile, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8, BUFFER);

            try
            {
                string docDataWithoutNewLine = docInfo.DocContent.Replace(Environment.NewLine, string.Empty);
                sw.Write(docDataWithoutNewLine);
                sw.Flush();
            }
            catch (Exception ex)
            {
                // ログ出力のロジックを後ほど入れる
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 訓練スタート
        /// </summary>
        public void StartTraining()
        {
            Process mecabProgram = new Process();
            Process word2vecProc = new Process();

            bool result = true;

            // 分かち書き
            mecabProgram.StartInfo.FileName = CommonParameters.MecabProgram;
            mecabProgram.StartInfo.Arguments = CommonParameters.TrainingDataFileFullPath + " -b 8192000 -Owakati -o " + CommonParameters.MeCabOutputFileName;
            result = mecabProgram.Start();

            mecabProgram.WaitForExit();

            mecabProgram.Close();
            mecabProgram.Dispose();


            // 機械学習
            word2vecProc.StartInfo.FileName = CommonParameters.Word2VecProgram;
            word2vecProc.StartInfo.Arguments = "-train " + CommonParameters.MeCabOutputFileName + " -output " + CommonParameters.VectorFileNameFullPath + " -cbow 0 -size 200 -window 10 -negative 0 -hs 1 -sample 1e-3 -threads 12 -binary 1";
            word2vecProc.Start();

            word2vecProc.WaitForExit();

            word2vecProc.Close();
            word2vecProc.Dispose();
        }

        /// <summary>
        /// 訓練データファイルのサイズを取得
        /// </summary>
        /// <returns></returns>
        private long GetTrainingDataFileSize()
        {
            string file = CommonParameters.TrainingDataFileFullPath;
            FileInfo fi = null;

            try
            {
                fi = new FileInfo(file);
                return fi.Length;
            }
            catch
            {
                // ログ出力をする。

                return -1;
            }
        }

        /// <summary>
        /// 訓練データファイルにこれ以上ファイルを書き出すかどうかのチェック
        /// </summary>
        /// <returns></returns>
        private bool CheckMaxSize()
        {
            long fileSize = GetTrainingDataFileSize();

            if (fileSize > _maxFileSize)
                return true;

            return false;
        }
    }
}
