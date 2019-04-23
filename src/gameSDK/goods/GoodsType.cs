using System.Collections.Generic;

namespace gameSDK
{
    public class GoodsType
    {
        public string name;

        /// <summary>
        /// 当前开通的结束槽位;
        /// </summary>
        protected int _lockSlot = -1;

        protected uint _len = 0;

        public List<object> limits;

        /// <summary>
        /// 开始槽位; 
        /// </summary>
        public int beginSlot
        {
            get { return _beginSlot; }
        }

        protected int _beginSlot;


        /// <summary>
        ///  当前开通的结束槽位;(锁定的位置; (默认为-1为没有锁定位置);) 
        /// </summary>
        public int lockSlot
        {
            get { return _lockSlot; }
            set { _lockSlot = value; }
        }


        public int realLockSlot
        {
            get
            {
                if (_lockSlot == -1)
                {
                    return endSlot + 1;
                }
                return _lockSlot;
            }
        }


        /// <summary>
        /// 最后一个槽位值; 
        /// </summary>
        public int endSlot
        {
            get { return _endSlot; }
        }

        protected int _endSlot;
        //分类id;	
        public int type;

        public GoodsType(int begin, uint len)
        {
            _beginSlot = begin;
            setLength(len);
        }

        public void setLength(uint value)
        {
            if (value == 0)
            {
                return;
            }

            _len = value;

            _endSlot = _beginSlot + (int) _len - 1;
        }

        public uint length
        {
            get { return _len; }
        }

        public int getCurrentMaxSlot()
        {
            return (int) _len - 1;
        }

        /// <summary>
        /// 已开启的格子总数
        /// </summary>
        /// <returns></returns>
        public int getTotalSlots()
        {
            return endSlot - beginSlot + 1;
        }
    }
}