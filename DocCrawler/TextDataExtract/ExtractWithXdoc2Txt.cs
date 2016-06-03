using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.TextDataExtract
{
    public class ExtractWithXdoc2Txt : ITextExtract
    {
        [DllImport(@"C:\DocSearch\bin\xd2txlib.dll", CharSet = CharSet.Unicode,
        CallingConvention = CallingConvention.Cdecl)]
        public static extern int ExtractText([MarshalAs(UnmanagedType.BStr)] String lpFileName,
        bool bProp,
        [MarshalAs(UnmanagedType.BStr)] ref String lpFileText);

        /// <summary>
        /// テキストデータの抽出
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public string Extract(string fileName, Encoding encode)
        {
            string extractedText = null;
            ExtractText(fileName, false, ref extractedText);

            return extractedText;
        }
    }
}
