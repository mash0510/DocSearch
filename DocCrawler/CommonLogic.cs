using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler
{
    /// <summary>
    /// 共通ロジック
    /// </summary>
    public static class CommonLogic
    {
        /// <summary>
        /// ファイルのバックアップを取る
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="backupFile"></param>
        public static void FileBackup(string sourceFile, string backupFile)
        {
            try
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
            catch (Exception ex)
            {
                // 後ほどログ出力をする
            }
        }

        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static void SafeCreateDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    return;

                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                // 後ほどログ出力をする
            }
        }
    }
}
