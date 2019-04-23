using System;
using System.Collections;
using foundation;

namespace gameSDK
{
    public class AbstractItemVO:ConfigVO<string>
    {
        public long guid;
        public int count;

        public int slot;
        public int maxstack;

        /// <summary>
        /// 物品获得时间
        /// </summary>
        public long addTime;

        /// <summary>
        /// 物品名字
        /// <summary>
        public string name;

        /// <summary>
        /// 是否为新获得的物品
        /// </summary>
        public bool isNew=false;

        /// <summary>
        /// 是否绑定
        /// </summary>
        public bool bind = false;

        /// <summary>
        /// 物品拥有者guid
        /// </summary>
        public long ownerId;

        /// <summary>
        /// 道具过期时间
        /// </summary>
        public long expireTime;



        //道具是否上锁
        public bool isLock=false;

        public AbstractItemVO Clone()
        {
            AbstractItemVO result = (AbstractItemVO) Activator.CreateInstance(this.GetType());
            ObjectUtils.copyFrom(result, this);
            return result;

            //ByteArray bytes = new ByteArray();
            //bytes.WriteObject(this);
            //bytes.Position = 0;
            //return (AbstractItemVO)bytes.ReadObject();
        }


        public virtual void decode(IList remoteList, int type)
        {

        }

        public virtual void execute(params object[] args)
        {

        }

    }
}
