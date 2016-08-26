using System;
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
        /// ホームディレクトリ
        /// </summary>
        private static string _homeDirectory = @"C:\DocSearch";
        /// <summary>
        /// クローラーシステムのホームディレクトリ
        /// </summary>
        /// <summary>
        /// クローラープログラムのホームディレクトリ。
        /// </summary>
        public static string HomeDirectory
        {
            get { return _homeDirectory; }
            set { _homeDirectory = value; }
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
        /// スケジュール設定ファイル名
        /// </summary>
        private static string _scheduleFileName = "scheduling.xml";
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
        /// スケジュール設定ファイルのフルパス取得
        /// </summary>
        public static string SchedulingFileFullPath
        {
            get
            {
                return _settingDataFolder + "\\" + _scheduleFileName;
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

        /// <summary>
        /// 直近クロールした時の文書ファイル数を保存したファイル
        /// </summary>
        private static string _totalDocumentsFile = "totalDocuments.dat";
        /// <summary>
        /// 直近クロールした時の文書ファイル数を保存したファイルのフルパス取得
        /// </summary>
        public static string TotalDocumentsFile
        {
            get
            {
                return _trainingDataFolder + "\\" + _totalDocumentsFile;
            }
        }

        /// <summary>
        /// クロール履歴フォルダ
        /// </summary>
        private static string _historyFolderCrawl = HomeDirectory + @"\history\crawl";
        /// <summary>
        /// クロール・機械学習履歴フォルダの取得
        /// </summary>
        public static string HistoryFolderCrawl
        {
            get
            {
                return _historyFolderCrawl;
            }
        }

        /// <summary>
        /// word2vec履歴フォルダ
        /// </summary>
        private static string _historyFolderWord2Vec = HomeDirectory + @"\history\word2vec";
        /// <summary>
        /// クロール・機械学習履歴フォルダの取得
        /// </summary>
        public static string HistoryFolderWord2Vec
        {
            get
            {
                return _historyFolderWord2Vec;
            }
        }

        /// <summary>
        /// 全体のドキュメント数が取得できなかったことを示す値
        /// </summary>
        public const decimal NO_TOTAL_DOCUMENTS = -1;


        private static long _maxTrainingFileSize = 0;
        /// <summary>
        /// word2vecに機械学習させる文書データの最大サイズのデフォルト値の取得
        /// </summary>
        public static long MaxTrainingFileSize
        {
            get { return _maxTrainingFileSize; }
            set { _maxTrainingFileSize = value; }
        }

        private static int _workerThreadStopDuration = 5 * 1000;
        /// <summary>
        /// ワーカースレッドが処理するデータが無くなって、止まるまでの秒数の取得
        /// </summary>
        public static int WorkerThreadStopDuration
        {
            get { return _workerThreadStopDuration; }
            set { _workerThreadStopDuration = value; }
        }

        private static int _fileIOBufferSize = 1024;
        /// <summary>
        /// 設定ファイルや訓練データなどのファイル読み書きバッファサイズ
        /// </summary>
        public static int FileIOBufferSize
        {
            get { return _fileIOBufferSize; }
            set { _fileIOBufferSize = value; }
        }
    }
}
