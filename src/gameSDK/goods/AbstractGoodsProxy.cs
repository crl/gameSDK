using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class AbstractGoodsProxy:Proxy
    {
        protected GoodsModelEvent item_count_change_event =new GoodsModelEvent(GoodsModelEvent.ITEM_COUNT_CHANGE);
        protected GoodsModelEvent item_add_event =new GoodsModelEvent(GoodsModelEvent.ITEM_ADD);
        protected GoodsModelEvent arrangement_event =new GoodsModelEvent(GoodsModelEvent.ARRANGEMENT);

        /// <summary>
        /// 物品列表;
        /// </summary>
        public ASDictionary<long,AbstractItemVO> _goods;

        /// <summary>
        ///  插槽对应表;
        /// </summary>
        public ASDictionary<int,AbstractItemVO> _slots;

        /// <summary>
        /// 类型分组; 
        /// </summary>
        protected ASDictionary<int,GoodsType> goodsTypes;
		
		public AbstractGoodsProxy(string name=""):base(name)
        { 
            _goods = new ASDictionary<long, AbstractItemVO>();
            _slots = new ASDictionary<int, AbstractItemVO>();
		    goodsTypes = new ASDictionary<int, GoodsType>();
        }

        public bool addGoodsType(int type, GoodsType goodsType)
        {
            if (goodsTypes.ContainsKey(type))
            {
                goodsTypes[type] = goodsType;
            }
            else
            {
                goodsTypes.Add(type,goodsType);
            }
            return true;
        }

   
        public GoodsType getGoodsType(int type)
        {
            GoodsType goodsType=null;
			goodsTypes.TryGetValue(type,out goodsType);

            return goodsType;
        }

        public AbstractItemVO getSlot(int slot)
        {
            AbstractItemVO abstractItemVo = null;
            _slots.TryGetValue(slot, out abstractItemVo);

            return abstractItemVo;
        }

        public AbstractItemVO getGUID(long guid)
        {
            AbstractItemVO abstractItemVo;
            _goods.TryGetValue(guid, out abstractItemVo);

            return abstractItemVo;
        }


        /// <summary>
        /// 取得类型仓库中 一类物品拥有的数量 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual int getCountByItemID(string itemID, int type = 0)
        {
            int count = 0;

            GoodsType goodsType = goodsTypes[type];
            if (goodsType == null)
            {
                return count;
            }
            int beginSlot = goodsType.beginSlot;
            int lockSlot = goodsType.realLockSlot;

            foreach (long key in _goods)
            {
                AbstractItemVO item = _goods[key];
                if (item.id != itemID || item.slot < beginSlot || item.slot > lockSlot)
                {
                    continue;
                }
                count += item.count;
            }
            return count;
        }
       
        /**
		 * 以物品类型来查看某类仓库的第一个物品
		 * @param itemID
		 * @param type
		 * @return 
		 * 
		 */
        public virtual AbstractItemVO getFirstItemByItemID(string itemID, int type= 0){
            GoodsType goodsType =goodsTypes[type];
            int beginSlot =goodsType.beginSlot;
            int lockSlot=goodsType.realLockSlot;
			
			AbstractItemVO item;
			for(int i=beginSlot;i<lockSlot;i++){
				item=_slots[i];
			    if (item != null && itemID == item.id)
			    {
			        return item;
			    }
			}
			return null;
		}
		
		
		/**
		 * 检查是否可拾取 
		 * @param itemID
		 * @param type
		 * @return 
		 * 
		 */		
		public bool checkPickupEnabledByItemID(string itemID, int type= 0)
		{
            GoodsType goodsType =goodsTypes[type];
			int beginSlot=goodsType.beginSlot;
			int lockSlot=goodsType.realLockSlot;
			
			AbstractItemVO itemVO;
			for(int i=beginSlot;i<lockSlot;i++){
				itemVO=_slots[i];
				if(itemVO==null){
					return true;//还有剩余的插槽;
				}
				if(itemVO.id !=itemID){
					continue;
				}
				if(itemVO.count<itemVO.maxstack){
					return true;	//还有可叠加的同类物品;
				}
			}
			return false;
		}
		
		public List<AbstractItemVO> filters(Func<AbstractItemVO,bool>func, int type= 0)
		{
            GoodsType goodsType =goodsTypes[type];
			int beginSlot=goodsType.beginSlot;
			int lockSlot=goodsType.realLockSlot;
			
			List<AbstractItemVO> result=new List<AbstractItemVO>();
            AbstractItemVO vo;
			for(int i=beginSlot;i<lockSlot;i++){
				vo=_slots[i];
				if(vo==null)continue;
				if(func(vo))result.Add(vo);
			}
			return result;
		}
		
		public virtual bool addItem(AbstractItemVO vo, bool fireEvent= true)
		{
			if (vo.count == 0)
			{
				return false;
			}
			long guid =vo.guid;
			AbstractItemVO oldvo = _goods[guid];
			if (oldvo!=null && oldvo.slot !=vo.slot)
			{
				//如果有旧的数据,就会有定位的信息;
				int slot=oldvo.slot;	
				_slots[oldvo.slot] = null;
				_slots.Remove(slot);
			}
			_goods[guid] = vo;
			_slots[vo.slot] = vo;
			if (fireEvent){

                invalidate();
			}
			
			//如果旧guid不存在,说明是新物品
			if(oldvo==null && hasEventListener(GoodsModelEvent.ITEM_ADD)){
				item_add_event.guid=guid;
				item_add_event.id=vo.id;

                dispatchEvent(item_add_event);
			}
			
			if(hasEventListener(GoodsModelEvent.ITEM_COUNT_CHANGE)){
				item_count_change_event.guid=guid;
				item_count_change_event.id=vo.id;

                dispatchEvent(item_count_change_event);
			}
			
			return true;
		}


        ///添加列表; 	
        public void addItems(List<AbstractItemVO>list, bool fireEvent= true){
		    foreach (AbstractItemVO vo in list)
		    {
		        addItem(vo, false);
		    }
		    if (fireEvent){
                invalidate();
			}
		}


        public void invalidate()
        {
            CallLater.Add(render);
        }

        public void render()
        {
            if (this.hasEventListener(EventX.CHANGE))
            {
                this.simpleDispatch(EventX.CHANGE);
            }
        }


        public virtual AbstractItemVO removeGUID(long guid, bool fireEvent = true)
        {
            AbstractItemVO vo = getGUID(guid);
            if (vo == null)
            {
                return null;
            }

            int slot = vo.slot;

            _goods.Remove(guid);
            _slots.Remove(slot);

            if (fireEvent)
            {
                invalidate();
            }
            if (hasEventListener(GoodsModelEvent.ITEM_COUNT_CHANGE))
            {
                item_count_change_event.guid = guid;
                item_count_change_event.id = vo.id;

                dispatchEvent(item_count_change_event);
            }
            return vo;
        }


        /// <summary>
        /// 通过整理 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool addItemWithZL(List<AbstractItemVO> value, int type = 0)
        {
            AbstractItemVO vo;
            long guid;

            GoodsType goodsType = goodsTypes[type];
            int beginSlot = goodsType.beginSlot;
            int lockSlot = goodsType.realLockSlot;

            //先做清理;
            for (int i = beginSlot; i < lockSlot; i++)
            {
                vo = _slots[i];
                if (vo == null)
                {
                    continue;
                }
                guid = vo.guid;
                _goods.Remove(guid);

                _slots.Remove(i);
            }

            foreach (AbstractItemVO item in value)
            {
                if (item.count < 1)
                {
                    continue;
                }
                _slots[item.slot] = item;
                _goods[item.guid] = item;
            }

            invalidate();

            if (hasEventListener(GoodsModelEvent.ARRANGEMENT))
            {
                this.dispatchEvent(arrangement_event);
            }

            return true;
        }



        public bool setItemCount(long guid, int count, bool fireEvent = true)
        {

            AbstractItemVO vo = _goods[guid];
            if (vo == null)
            {
                return false;
            }

            if (count <= 0)
            {
                count = 0;

                removeGUID(vo.guid, false);
            }
            else
            {
                vo.count = count;
            }

            if (fireEvent)
            {

                invalidate();
            }

            if (hasEventListener(GoodsModelEvent.ITEM_COUNT_CHANGE))
            {
                item_count_change_event.guid = guid;
                item_count_change_event.id = vo.id;

                this.dispatchEvent(item_count_change_event);
            }
            return true;
        }

       /// <summary>
       /// 取得最靠前的一个空槽位
       /// </summary>
       /// <param name="type"></param>
       /// <param name="offset"></param>
       /// <returns></returns>
		public int getMinEmptySlot(int type= 0, int offset= 0)
		{
            GoodsType goodsType =goodsTypes[type];
			int beginSlot=goodsType.beginSlot+offset;
            int lockSlot = goodsType.realLockSlot;

            for (int i=beginSlot;i<lockSlot;i++){
				if(_slots[i]==null)return i;
			}
			return -1;
		}
		
		/**
		 * 获取空的格位数 
		 * @param type
		 * @return 
		 * 
		 */		
		public int getEmptySlotCount(int type= 0)
		{
            GoodsType goodsType =goodsTypes[type];
			int beginSlot=goodsType.beginSlot;
            int lockSlot = goodsType.realLockSlot;
			int count=0;
			for (int i=beginSlot;i<lockSlot;i++){
				if(_slots[i]==null)count++;
			}
			return count;
		}
		
		
		/**
		 * 取得一个类型中的分页数据中的某一页数据(含有空数据格);
		 * @param index -1为罗列全部
		 * @param pageCount
		 * @param type
		 * @return 
		 * 
		 */		
		public ASList<AbstractItemVO> getPageData(int index, int pageCount, int type= 0){
            GoodsType goodsType =goodsTypes[type];
			
			int beginSlot;
			int endSlot;
			int lockSlot;
			
			beginSlot=goodsType.beginSlot+index* pageCount;
		    lockSlot = goodsType.realLockSlot;
			
			endSlot=Mathf.Min(lockSlot,beginSlot+pageCount);
			
			tempVOVector.Clear();
			int i;
			for (i = beginSlot; i<endSlot; i++)
			{
				tempVOVector.Add(_slots[i]);	
			}
			
			endSlot=Mathf.Min(goodsType.endSlot,beginSlot+pageCount);
			if(endSlot>i){
				for(;i<endSlot;i++){
					tempVOVector.Add(null);
				}
			}
			
			return tempVOVector;
		}
		
		protected ASList<AbstractItemVO> tempVOVector=new ASList<AbstractItemVO>();

        ///
        /// 取得类型数据(不包含空数据格)	
        public ASList<AbstractItemVO> getTypeData(int type= 0, Func<AbstractItemVO,bool>filter= null){
			int beginSlot;
			int lockSlot;

            GoodsType goodsType =goodsTypes[type];
			beginSlot=goodsType.beginSlot;
			lockSlot=goodsType.realLockSlot;
			
			tempVOVector.Clear();
			int i;
			AbstractItemVO vo;
			
			if(filter !=null){
				for (i = beginSlot; i<lockSlot; i++)
				{
					vo=_slots[i];
					if(vo!=null && filter(vo)){
						tempVOVector.Add(vo);	
					}
				}
			}else{
				for (i = beginSlot; i<lockSlot; i++)
				{
					vo=_slots[i];
					if(vo!=null){
						tempVOVector.Add(vo);	
					}
				}
			}
			return tempVOVector;
		}

        public void clearAll()
        {
            _goods.Clear();
            _slots.Clear();

            invalidate();
        }
    }


}