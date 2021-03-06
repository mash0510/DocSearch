﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FolderCrawler.Setting
{
    /// <summary>
    /// ファイル保存する設定項目
    /// </summary>
    public class Settings
    {
        public const string TYPE = "SETUP_CRAWL_FOLDER";

        private static Settings _self = new Settings();

        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <returns></returns>
        public static Settings GetInstance()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private Settings()
        {

        }

        #region 設定ファイルの保存と読み込み
        /// <summary>
        /// 読み込んだ設定を復元
        /// </summary>
        /// <param name="parameters"></param>
        public void RestoreSettings(Settings parameters)
        {
            CrawlFolders.Clear();
            CrawlFolders = parameters.CrawlFolders;
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        public void SaveSettings()
        {
            CommonLogic.SafeCreateDirectory(Path.GetDirectoryName(CommonParameters.SettingFileFullPath));

            //ファイルを開く（UTF-8 BOM無し）
            StreamWriter sw = new StreamWriter(CommonParameters.SettingFileFullPath, false, new UTF8Encoding(false));

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, _self);
            }
            catch (Exception ex)
            {
                // ログ出力処理を後ほど記述
            }
            finally
            {
                //閉じる
                sw.Close();
            }
        }

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        public void LoadSettings()
        {
            if (!File.Exists(CommonParameters.SettingFileFullPath))
                return;

            //ファイルを開く
            StreamReader sr = new StreamReader(CommonParameters.SettingFileFullPath, new UTF8Encoding(false));

            try
            {
               XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                //XMLファイルから読み込み、逆シリアル化する
                _self.RestoreSettings((Settings)serializer.Deserialize(sr));
            }
            catch (Exception ex)
            {
                // ログ出力処理を後ほど記述
            }
            finally
            {
                //閉じる
                sr.Close();
            }
        }
        #endregion

        #region 設定パラメータ（ファイル保存されるもの）
        /// <summary>
        /// クロール先のフォルダリスト
        /// </summary>
        private List<string> _crawlFolders = new List<string>();
        /// <summary>
        /// クロール先のフォルダリスト
        /// </summary>
        public List<string> CrawlFolders
        {
            get { return _crawlFolders; }
            set { _crawlFolders = value; }
        }
        #endregion
    }
}
