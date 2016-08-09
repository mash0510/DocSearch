﻿using System;
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
        private long _maxFileSize = CommonParameters.MaxTrainingFileSize;

        /// <summary>
        /// ファイル書き出しバッファサイズ
        /// </summary>
        private int BUFFER = CommonParameters.FileIOBufferSize;

        private bool _stopGenerateData = false;
        private bool _cancelGenerateData = false;

        /// <summary>
        /// 訓練データ生成処理実行中かどうか
        /// </summary>
        public bool IsProcessingTrainingDataGeneration
        {
            get;
            private set;
        }

        /// <summary>
        /// 訓練データ生成処理の停止
        /// </summary>
        private bool StopGenerateData
        {
            set
            {
                IsProcessingTrainingDataGeneration = !value;
                _stopGenerateData = value;
            }
            get
            {
                return _stopGenerateData;
            }
        }

        /// <summary>
        /// 機械学習処理を実行中かどうか
        /// </summary>
        public bool IsProcessingMachineLearning
        {
            get;
            private set;
        }

        /// <summary>
        /// 機械学習用データの最大サイズの設定と取得
        /// </summary>
        public long MaxTrainingFileSize
        {
            get { return this._maxFileSize; }
            set { this._maxFileSize = value; }
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
        /// 機械学習データ作成の
        /// </summary>
        public void Stop()
        {
            StopGenerateData = true;
            _cancelGenerateData = true;

            _timeElapse.TimerStop();
        }

        /// <summary>
        /// ワーカースレッドの未処理時間経過後、ワーカースレッドを止める
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeElapse_Elapsed(object sender, EventArgs e)
        {
            StopGenerateData = true;
            _timeElapse.TimerStop();
        }

        public delegate void TrainingDataGenerateFinishedDelegate(bool isCanceled);
        public event TrainingDataGenerateFinishedDelegate TrainingDataGenerateFinished;

        /// <summary>
        /// 訓練データ生成処理
        /// </summary>
        public void StartTrainingDataGenerate()
        {
            // 既存の訓練データの削除
            DeleteTrainingDataFile();

            StopGenerateData = false;
            _cancelGenerateData = false;

            while (true)
            {
                if (QueueManager.GetInstance().DocInfoQueue.Count == 0)
                {
                    if (!_timeElapse.IsTimerStarted)
                        _timeElapse.TimerStart(CommonParameters.WorkerThreadStopDuration);

                    if (StopGenerateData)
                        break;
                    else
                        continue;
                }

                _timeElapse.TimerStop();

                if (StopGenerateData)
                {
                    // ユーザーがキャンセル操作を実行したら、ループを停止する。
                    break;
                }

                DocumentInfo docInfo = QueueManager.GetInstance().DocInfoQueue.Dequeue() as DocumentInfo;
                if (docInfo == null)
                    continue;

                GenerateTrainingData(docInfo);
            }

            // 一時退避用に保持していたバックアップファイルを削除
            File.Delete(CommonParameters.TrainingDataFileBackupFullPath);

            TrainingDataGenerateFinished?.Invoke(_cancelGenerateData);
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
        /// ファイルのバックアップを取る
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="backupFile"></param>
        private void FileBackup(string sourceFile, string backupFile)
        {
            if (!File.Exists(sourceFile))
                return;

            // 既に取られたバックアップファイルがある場合は削除。
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }

            // 既存のバックアップファイルを削除した後に、一時バックアップファイル名を正式なバックアップファイル名にリネームする
            File.Move(sourceFile, backupFile);
        }

        /// <summary>
        /// 訓練データの削除
        /// </summary>
        public void DeleteTrainingDataFile()
        {
            string outputFile = CommonParameters.TrainingDataFileFullPath;
            string backupFile = CommonParameters.TrainingDataFileBackupFullPath;

            FileBackup(outputFile, backupFile);
        }

        /// <summary>
        /// word2vecにより生成された単語ベクトルファイルを、実際に使用するファイル名に変更
        /// </summary>
        public void ChangeVectorFileNameInUse()
        {
            string outputFile = CommonParameters.VectorFileNameFullPath;
            string newFileName = CommonParameters.VectorFileNameInUseFullPath;

            FileBackup(outputFile, newFileName);
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
            // 0が設定されていたら、上限なし。
            if (_maxFileSize == 0)
                return false;

            long fileSize = GetTrainingDataFileSize();

            if (fileSize > _maxFileSize)
                return true;

            return false;
        }
    }
}
