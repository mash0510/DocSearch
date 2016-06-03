using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.TextDataExtract
{
    public interface ITextExtract
    {
        /// <summary>
        /// テキストデータの抽出
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string Extract(string fileName, Encoding encode);
    }
}
