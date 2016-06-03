using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.GenerateID
{
    /// <summary>
    /// Elasticsearchへのデータ登録時に付与するIDの生成
    /// </summary>
    public interface IGenerateID
    {
        /// <summary>
        /// IDの取得
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string GetID(Dictionary<object, object> data);
    }
}
