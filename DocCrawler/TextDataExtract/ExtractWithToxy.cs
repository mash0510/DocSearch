using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toxy;

namespace FolderCrawler.TextDataExtract
{
    public class ExtractWithToxy : ITextExtract
    {
        /// <summary>
        /// テキストデータの抽出
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string Extract(string fileName, Encoding encode)
        {
            ParserContext context = new ParserContext(fileName, encode);
            ITextParser extractParser = ParserFactory.CreateText(context);
            
            string extractedData = extractParser.Parse();

            return extractedData;
        }
    }
}
