using System.Collections;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace clayui
{
    public class Page:AbstractPage
    {

        protected int _length;
        protected List<object> _currentPageData;
        public Page(int pageSize = 5)
        {
            _currentPageData=new List<object>();
            this.pageSize = pageSize;
        }

        public override IList getCurrentPageData()
        {
            return _currentPageData;
        }

        private IList _dataProvider;
        public IList dataProvider
        {
            get { return _dataProvider; }
            set
            {
                if (value == null)
                {
                    value = EMPTY_OBJECTS;
                }
                _dataProvider = value;
                _length = _dataProvider.Count;
                this.invalidate();
            }
        }

        public int dataLength
        {
            get
            {
                if (_dataProvider == null)
                {
                    return 0;
                }
                return _dataProvider.Count;
            }
        }

        public override void invalidate()
        {
            _currentPageData.Clear();

            this._totalPage = (int)Mathf.Ceil((float)_length/_pageSize);

            if (_totalPage < 1)
            {
                this.simpleDispatch(EventX.CHANGE);
                return;
            }

            if (_currentPage > _totalPage - 1)
            {
                _currentPage = 0;
            }

            int _current;
            int _last;

            _current = _currentPage * pageSize;
            if (_currentPage >= _totalPage - 1)
            {
                _last = _length;
            }
            else {
                _last = _current + _pageSize;
            }

            for (int i = _current ; i < _last; i++){
                _currentPageData.Add(_dataProvider[i]);
            }

            if (hasEventListener(EventX.CHANGE))
            {
                this.simpleDispatch(EventX.CHANGE);
            }
        }
        
    }
}