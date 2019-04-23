using System;
using System.Collections;
using clayui;
using UnityEngine;
using foundation;

namespace gameSDK
{
    /// <summary>
    /// 定位信息
    /// </summary>
    public class LocationService:EventDispatcher
    {
        protected Coroutine coroutine;
        protected float tickTime = 5f;
        protected static LocationService instance;
        public static LocationService GetInstance()
        {
            if (instance == null)
            {
                instance=new LocationService();
            }
            return instance;
        }

        public void start(float tickTime=5f)
        {
            if (tickTime < 2.0f)
            {
                tickTime = 2.0f;
            }
            this.tickTime = tickTime;

            if (Input.location.isEnabledByUser == false)
            {
                UITip.Show("请先为应用开启定位");
                CallLater.Add(checkNetWork, 5f);
                return;
            }

            if (coroutine == null)
            {
                coroutine = BaseApp.Instance.StartCoroutine(check());
            }
        }

        private void checkNetWork()
        {
            if (Input.location.isEnabledByUser == false)
            {
                CallLater.Add(checkNetWork, 5f);
            }
            else
            {
                start(tickTime);
            }
        }

        public void stop()
        {
            if (coroutine != null)
            {
                BaseApp.Instance.StopCoroutine(coroutine);
                coroutine = null;
                // Stop service if there is no need to query location updates continuously
                Input.location.Stop();
            }
            CallLater.Remove(checkNetWork);
        }

        IEnumerator check()
        {
            // Start service before querying location
            Input.location.Start();
            // Wait until service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                simpleDispatch(EventX.FAILED, "timeout");
                stop();
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                simpleDispatch(EventX.FAILED, LocationServiceStatus.Failed);
                stop();
                yield break;
            }

            while (true)
            {
                LocationInfo lastData = Input.location.lastData;
                // Access granted and location value could be retrieved
               
                simpleDispatch(EventX.CHANGE, lastData);

                yield return new WaitForSeconds(tickTime);
            }
        }
    }
}