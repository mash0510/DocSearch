using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.TextDataExtract
{
    public class TextExtractFactory<T> where T : class, ITextExtract
    {
        public T Create()
        {
            Type t = typeof(T);
            var obj = Activator.CreateInstance(t);
            T retInstance = obj as T;

            return retInstance;
        }
    }
}
