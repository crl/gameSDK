using System.Collections;
using foundation;
using UnityEngine;

namespace clayui
{
    public abstract class AbstractPage:EventDispatcher
    {
        protected static object[] EMPTY_OBJECTS = new object[0];
        protected int _pageSize = 1;
        protected int _currentPage = 0;
        protected int _totalPage = 0;

        public int totalPage
        {
            get { return _totalPage; }
        }

        /// <summary>
        ///  上一页 
        /// </summary>
        public void previousPage()
        {
            this.currentPage--;
        }

        /// <summary>
        /// 下一页; 
        /// </summary>
        public void nextPage()
        {
            currentPage++;
        }

        public int pageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public int currentPage
        {
            set
            {
                value = Mathf.Max(0, Mathf.Min(value, _totalPage - 1));
                if (value != _currentPage)
                {
                    _currentPage = value;
                    invalidate();
                }
            }
            get { return _currentPage; }
        }

        public virtual void invalidate()
        {
        }

        /// <summary>
        /// 是否有下一页;
        /// </summary>
        public bool hasNextPage
        {
            get { return this._currentPage < this._totalPage - 1; }
        }

        public abstract IList getCurrentPageData();

        /// <summary>
        /// 是否有上一页; 
        /// </summary>
        public bool hasPreviousPage
        {
            get { return this._currentPage > 0; }
        }

        /// <summary>
        /// 当前页码第一项索引 
        /// </summary>
        /// <returns></returns>
        public int getFirstIndex(){
			return _currentPage* pageSize;
}
    }
}