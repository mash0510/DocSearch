using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.GenerateID
{
    public class GetGUID : IGenerateID
    {
        /// <summary>
        /// GUIDを返す
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetID(Dictionary<object, object> data)
        {
            string guid = Guid.NewGuid().ToString("N");

            return guid;
        }
    }
}
