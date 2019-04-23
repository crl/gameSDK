using foundation;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace gameSDK
{
    public class DownloadItem:EventDispatcher
    {
        public HashSizeFile HashSizeFile;
        public int downloadedBytes;
        protected string url;
        private float preTime = 0;
        public DownloadItem(HashSizeFile hashSizeFile, string url)
        {
            this.url = url;
            this.HashSizeFile = hashSizeFile;
        }

        public void start()
        {
            BaseApp.Instance.StartCoroutine(doLoad());
        }

        private IEnumerator doLoad()
        {
            preTime = Time.realtimeSinceStartup;
            UnityWebRequest request = UnityWebRequest.Get(url);
            AsyncOperation operation = request.Send();
            bool isTimeout = false;
            while (!operation.isDone)
            {
                if (checkTimeout(request))
                {
                    isTimeout = true;
                    break;
                }
                yield return null;
            }

            if (isTimeout)
            {
                this.simpleDispatch(EventX.FAILED, "timeOut");
            }
            else
            {
                long responseCode = request.responseCode;
                if (responseCode == 404)
                {
                    this.simpleDispatch(EventX.FAILED, 404);
                }else if (request.isNetworkError)
                {
                    this.simpleDispatch(EventX.FAILED, request.error);
                }
                else
                {
                    this.simpleDispatch(EventX.COMPLETE, request.downloadHandler.data);
                }
            }

            request.Dispose();
            request = null;
        }

        private float preProgress = 0.0f;
        private int checkCount = 0;
        private bool checkTimeout(UnityWebRequest request)
        {
            float progress = request.downloadProgress;
            float time = Time.realtimeSinceStartup;
            if (time - preTime < 2.0f)
            {
                return false;
            }
            preTime = time;

            if (progress == preProgress)
            {
                checkCount++;

                if (checkCount > 2)
                {
                    return true;
                }
            }

            downloadedBytes = (int)request.downloadedBytes;
            preProgress = progress;
            this.simpleDispatch(EventX.PROGRESS, progress);


            return false;
        }
    }
}