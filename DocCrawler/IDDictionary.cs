using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// ElasticsearchのIDと文書ファイルのフルパスの対応を管理
    /// </summary>
    public class IDDictionary
    {
        /// <summary>
        /// IDと文書フルパスの対応管理Dictionary。keyがファイルフルパス。valueがElasticsearchに割り振ったID
        /// </summary>
        private Dictionary<string, string> _docID = new Dictionary<string, string>();

        private bool _idLoaded = false;

        /// <summary>
        /// IDのロードが完了しているかどうかの取得
        /// </summary>
        public bool IDLoaded
        {
            get { return this._idLoaded; }
        }

        /// <summary>
        /// 読み書きバッファサイズ
        /// </summary>
        private const int BUFFER = 1024;

        private static IDDictionary _self = new IDDictionary();

        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <returns></returns>
        public static IDDictionary GetInstanse()
        {
            return _self;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private IDDictionary()
        {

        }

        /// <summary>
        /// ID、文書ファイルフルパスの追加
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fileFullPath"></param>
        /// <param name="save"></param>
        public void AddID(string fileFullPath, string id, bool save)
        {
            if (this._docID.ContainsKey(fileFullPath))
                return;

            this._docID.Add(fileFullPath, id);

            if (save)
                SaveData(fileFullPath, id);
        }

        /// <summary>
        /// 文書ファイルに割り当てたElasticsearchのIDを返す
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        public string GetElasticsearchID(string fileFullPath)
        {
            if (!this._docID.ContainsKey(fileFullPath))
                return null;

            string eID = this._docID[fileFullPath];

            return eID;
        }

        /// <summary>
        /// IDと文書ファイルフルパスの関係を保存。追記保存する。
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async void SaveData(string fileFullPath, string id)
        {
            await Task.Run(() =>
            {
                string saveFile = CommonParameters.DicFileNameFullPath;
                StreamWriter sw = new StreamWriter(saveFile, true, Encoding.UTF8);

                string writeData = fileFullPath + "\t" + id;

                try
                {
                    sw.WriteLine(writeData);
                }
                catch (Exception ex)
                {
                    // 後ほどログ出力ロジックを入れる
                }
                finally
                {
                    sw.Close();
                }
            });
        }

        public event System.EventHandler SaveFinished;
        public event System.EventHandler LoadFinished;

        /// <summary>
        /// ID管理Dictionaryファイルの保存。普段は使わないが、Dictionaryの内容を一気に保存したい場合に明示的に呼び出す。
        /// </summary>
        public async void Save()
        {
            await Task.Run(() =>
            {
                string saveFile = CommonParameters.DicFileNameFullPath;

                FileStream fs = new FileStream(saveFile, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8, BUFFER);

                int i = 0;

                try
                {
                    foreach (string key in this._docID.Keys)
                    {
                        string value = this._docID[key];
                        string writeData = key + "\t" + value;

                        sw.WriteLine(writeData);

                        if (i >= BUFFER)
                            sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    // 後ほど、ログ出力ロジックを入れる
                }
                finally
                {
                    sw.Close();
                    fs.Close();
                }
                
            });

            SaveFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// ID管理Dictionaryファイルの読み込み（非同期実行）
        /// </summary>
        public async void LoadAsync()
        {
            await Task.Run(() =>
            {
                Load();
            });

            LoadFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// ID管理Dictionaryファイルの読み込み
        /// </summary>
        public void Load()
        {
            string readFile = CommonParameters.DicFileNameFullPath;
            char[] delimiter = { '\t' };

            FileStream fs = new FileStream(readFile, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8, true);

            try
            {
                while (sr.Peek() >= 0)
                {
                    string record = sr.ReadLine();
                    string[] data = record.Split(delimiter);

                    if (data.Length >= 2)
                    {
                        AddID(data[0], data[1], false);
                    }
                }

                LoadFinish();
            }
            catch (Exception ex)
            {
                // 後ほどログ出力ロジックを入れる
            }
            finally
            {
                sr.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// ロード完了フラグを立てる
        /// </summary>
        private void LoadFinish()
        {
            this._idLoaded = true;
        }
    }
}
