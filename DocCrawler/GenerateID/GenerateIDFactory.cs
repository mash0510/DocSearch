using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.GenerateID
{
    /// <summary>
    /// Elasticsearchへのデータ挿入時に付与するID生成クラスの取得
    /// </summary>
    public class GenerateIDFactory<T> where T : class, IGenerateID 
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
