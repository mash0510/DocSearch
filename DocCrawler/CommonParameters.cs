﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 共通パラメータ
    /// </summary>
    public static class CommonParameters
    {
        /// <summary>
        /// クローラープログラムのホームディレクトリ。
        /// </summary>
        public static string HomeDirectory
        {
            get { return Settings.GetInstance().HomeDirectory; }
            set {  Settings.GetInstance().HomeDirectory = value; }
        }

        /// <summary>
        /// 設定データの格納先フォルダ
        /// </summary>
        private static string _settingDataFolder = HomeDirectory + @"\settings";
        /// <summary>
        /// 設定ファイル名
        /// </summary>
        private static string _settingFileName = "setting.xml";
        /// <summary>
        /// 設定ファイルのフルパス取得
        /// </summary>
        public static string SettingFileFullPath
        {
            get
            {
                return _settingDataFolder + "\\" + _settingFileName;
            }
        }


        /// <summary>
        /// データファイルの格納先フォルダ
        /// </summary>
        private static string _trainingDataFolder = HomeDirectory + @"\data";
        /// <summary>
        /// 訓練データファイル名
        /// </summary>
        private static string _trainingDataFileName = "training.dat";
        /// <summary>
        /// 訓練データのバックアップ
        /// </summary>
        private static string _trainingDataFileBackup = "training.bak";

        /// <summary>
        /// 訓練データファイルのフルパスの取得
        /// </summary>
        public static string TrainingDataFileFullPath
        {
            get
            {
                return _trainingDataFolder + "\\" + _trainingDataFileName;
            }
        }

        /// <summary>
        /// 訓練データバックアップファイルのフルパスの取得
        /// </summary>
        public static string TrainingDataFileBackupFullPath
        {
            get
            {
                return _trainingDataFolder + "\\" + _trainingDataFileBackup;
            }
        }

        /// <summary>
        /// 分かち書き訓練データファイルのフルパスの取得
        /// </summary>
        private static string _mecabOutputFileName = "trainingMeCab.dat";

        /// <summary>
        /// 訓練データファイルのフルパスの取得
        /// </summary>
        public static string MeCabOutputFileName
        {
            get
            {
                return _trainingDataFolder + "\\" + _mecabOutputFileName;
            }
        }

        /// <summary>
        /// ElasticsearchIDと文書ファイルフルパスとの対応関係を保存するファイルの名前
        /// </summary>
        private static string _dicFileName = "docIDDic.dat";

        /// <summary>
        /// ElasticsearchIDと文書ファイルフルパスとの対応関係を保存するファイル名の取得
        /// </summary>
        public static string DicFileNameFullPath
        {
            get
            {
                return _trainingDataFolder + "\\" + _dicFileName;
            }
        }

        /// <summary>
        /// 単語のベクトルデータファイル名
        /// </summary>
        private static string _vectorFileName = "vectors.dat";
        /// <summary>
        /// 利用中の単語ベクトルデータファイル名
        /// </summary>
        private static string _vectorFileNameInUse = "vectors_inuse.dat";

        /// <summary>
        /// 単語ベクトルデータファイルのフルパスの取得
        /// </summary>
        public static string VectorFileNameFullPath
        {
            get
            {
                return _trainingDataFolder + "\\" + _vectorFileName;
            }
        }

        /// <summary>
        /// 利用中の単語ベクトルデータファイル名の取得
        /// </summary>
        public static string VectorFileNameInUseFullPath
        {
            get
            {
                return _trainingDataFolder + "\\" + _vectorFileNameInUse;
            }
        }

        /// <summary>
        /// exeファイルの格納先フォルダ
        /// </summary>
        private static string _binFileFolder = HomeDirectory + @"\bin";

        /// <summary>
        /// MeCabのexeファイル名
        /// </summary>
        private static string _mecabProgram = "mecab.exe";

        /// <summary>
        /// MeCabのexeファイルのフルパスの取得
        /// </summary>
        public static string MecabProgram
        {
            get
            {
                return _mecabProgram;
            }
        }

        /// <summary>
        /// word2vecのexeファイル名
        /// </summary>
        private static string _word2vecProgram = "word2vec.exe";

        /// <summary>
        /// word2Vecのexeファイルのフルパスの取得
        /// </summary>
        public static string Word2VecProgram
        {
            get
            {
                return _word2vecProgram;
            }
        }

        private static long _defaultMaxTrainingFileSize = 100L * 1024 * 1024 * 1024;
        /// <summary>
        /// word2vecに機械学習させる文書データの最大サイズのデフォルト値の取得
        /// </summary>
        public static long DefaultMaxTrainingFileSize
        {
            get
            {
                return _defaultMaxTrainingFileSize;
            }
        }

        private static int _workerThreadStopDuration = 30 * 1000;
        /// <summary>
        /// ワーカースレッドが処理するデータが無くなって、止まるまでの秒数の取得
        /// </summary>
        public static int WorkerThreadStopDuration
        {
            get { return _workerThreadStopDuration; }
        }
    }
}
