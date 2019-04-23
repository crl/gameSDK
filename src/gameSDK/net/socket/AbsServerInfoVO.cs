using System;
using UnityEngine;

namespace foundation
{
    public class AbsServerInfoVO
    {
        public AbsServerInfoVO()
        {
        }
        public string version="1.0";
		protected float _local2Server_basetime;
		private float _openServiceTime;
		
		public float local2Server_basetime
		{
		    get { return _local2Server_basetime; }
		}

        //服务器时间;
        public float serverTimer
        {
            get { return _local2Server_basetime/1000.0f + Time.realtimeSinceStartup; }
        }

        private DateTime _serverDate;
        private DateTime serverDate
        {
            get
            {
                _serverDate = new DateTime((long)serverTimer*10000000);
                return _serverDate;
            }
        }

        /**
		 * 开服时间
		 */
		public float openServiceTime
		{
		    get { return _openServiceTime; }
		    set
		    {
                this._openServiceTime = value;
            }
		}
    }
}