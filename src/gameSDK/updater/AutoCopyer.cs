using foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace gameSDK
{
    public class AutoCopyer : EventDispatcher
    {
        public static int CONCURRENCE = 4;
        public static float timeOutEachCheck = 8.0f;
        private int _threadCount = -1;
        private string src;
        private string desc;

        private List<HashSizeFile> timeOutList = new List<HashSizeFile>();
        private Queue<HashSizeFile> needLoadList = new Queue<HashSizeFile>();
        private ASDictionarySet<UnityWebRequest, HashSizeFile> requestList = new ASDictionarySet<UnityWebRequest, HashSizeFile>();
        private int total = 0;

        public void copyFromTO(string src, string desc)
        {
            this.src = src;
            this.desc = desc;
        }

        public void start(Dictionary<string, HashSizeFile> localMapping, int threadCount = -1)
        {
            if (threadCount == -1)
            {
                threadCount = Mathf.Max(CoreLoaderQueue.CONCURRENCE, CONCURRENCE);
            }
            _threadCount = threadCount;
            Application.backgroundLoadingPriority = ThreadPriority.High;

            needLoadList.Clear();
            timeOutList.Clear();
            requestList.Clear();

            foreach (HashSizeFile item in localMapping.Values)
            {
                needLoadList.Enqueue(item);
            }
            total = needLoadList.Count;
            checkComplete();

        }

        private void checkComplete()
        {
            if (needLoadList.Count == 0 && requestList.Count == 0)
            {
                if (timeOutList.Count > 0)
                {
                    foreach (HashSizeFile s in timeOutList)
                    {
                        needLoadList.Enqueue(s);
                    }
                    total = needLoadList.Count;
                    timeOutList.Clear();

                    GC.Collect();
                    CallLater.Add(checkComplete,1.0f);
                    return;
                }
                DebugX.Log("AutoCopyer complete");
                TickManager.Remove(tick);
                this.simpleDispatch(EventX.COMPLETE);
                return;
            }
            _doNext();
        }

        private void _doNext()
        {
            if (requestList.Count < _threadCount && needLoadList.Count > 0)
            {
                HashSizeFile item = needLoadList.Dequeue();
                string uri = item.uri;
                string fullPath = src + uri;

                UnityWebRequest loader =UnityWebRequest.Get(fullPath);
                requestList.Add(loader, item);

                TickManager.AddAntTick(tick);
            }
        }

        private void tick(float deltaTime)
        {
            List<UnityWebRequest> webRequests = requestList.UnsafeKeys;
            int len = webRequests.Count;
            bool isTimeOut = false;
            bool hasComplete = false;
            for (int i = 0; i < len; i++)
            {
                UnityWebRequest request = webRequests[i];
                HashSizeFile item = requestList.UnsafeValues[i];
                if (!request.isDone)
                {
                    continue;
                }
                string uri = item.uri;
                if (isTimeOut == false)
                {
                    timeOutList.Add(item);
                    DebugX.LogWarning("timeOut:" + uri + " b:" + request.downloadedBytes);
                }
                else
                {
                    if (request.isNetworkError)
                    {
                        string error = request.error;
                        timeOutList.Add(item);
                        DebugX.LogWarning("AutoCopyer loadFileError:" + uri + " error:" + error);
                    }
                    else
                    {
                        byte[] bytes = request.downloadHandler.data;
                        string filePath = desc + uri;
                        try
                        {
                            if (bytes.Length == item.size)
                            {
                                FileHelper.AutoCreateDirectory(filePath);
                                File.WriteAllBytes(filePath, bytes);
                            }
                            else
                            {
                                DebugX.LogWarning("FileSizeError:" + bytes.Length + "!=" + item.size);
                                timeOutList.Add(item);
                            }
                        }
                        catch (Exception e)
                        {
                            timeOutList.Add(item);
                            DebugX.LogWarning("writeFileError:" + e.Message);
                        }
                    }
                }

                requestList.Remove(request);
                len--;
                hasComplete = true;
            }

            if (hasComplete)
            {
                int index = Mathf.Max(1, total - needLoadList.Count);
                this.simpleDispatch(EventX.PROGRESS, index/(float) total);
                checkComplete();
            }
            else 
            {
                _doNext();
            }
        }
    }
}