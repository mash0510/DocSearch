using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.CommonLogic
{
    /// <summary>
    /// ページ表示ロジック
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// データの総件数の設定と取得
        /// </summary>
        public int TotalDataNum { set; get; }

        /// <summary>
        /// 1ページに表示するデータ件数の設定と取得
        /// </summary>
        public int PageSize { set; get; }

        /// <summary>
        /// データのどこからどこまでを表示させるか
        /// </summary>
        public struct DataRange
        {
            public int Start;
            public int End;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Pagination()
        {

        }

        /// <summary>
        /// 全体のページ数の取得
        /// </summary>
        /// <returns></returns>
        public int GetTotalPageNum()
        {
            if (TotalDataNum <= 0)
                return 1;

            if (PageSize <= 0)
                return 1;

            int pageNum = TotalDataNum / PageSize;
            int increment = TotalDataNum % PageSize > 0 ? 1 : 0;

            pageNum = pageNum + increment;

            return pageNum;
        }

        /// <summary>
        /// そのページに表示するデータが何番目から何番目のデータなのかを返す。
        /// </summary>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public DataRange GetDataRange(int pageNo)
        {
            int page = pageNo - 1;
            if (page < 0) page = 0;

            DataRange retval = new DataRange();

            retval.Start = page * PageSize + 1;
            retval.End = retval.Start + PageSize - 1;

            return retval;
        }

        /// <summary>
        /// ページ番号のリストを取得
        /// </summary>
        /// <returns></returns>
        public List<int> GetPageList()
        {
            int pageNum = GetTotalPageNum();

            List<int> pageList = new List<int>();

            for(int i = 1; i <= pageNum; i++)
            {
                pageList.Add(i);
            }

            return pageList;
        }
    }
}