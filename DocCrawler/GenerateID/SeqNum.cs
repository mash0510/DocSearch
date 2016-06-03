using FolderCrawler.GenerateID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCrawler.GenerateID
{
    public class SeqNum : IGenerateID
    {
        private static int _count = 0;

        public string GetID(Dictionary<object, object> data)
        {
            _count++;
            return _count.ToString();
        }
    }
}
