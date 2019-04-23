using foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 远程更新的下载管理类
    /// </summary>
    public class UpdateDownloader : EventDispatcher
    {
        public static bool isDebug = true;
        private static UpdateDownloader instance;
        public static int CONCURRENCE = 4;
        private VersionLoaderFactory factory;
        public Func<string, bool> outDownloadFliter;

        private static string _VersionHttpPrefix;
        public static void SetVersionHttpPrefix(string value)
        {
            _VersionHttpPrefix = value;
        }
        public static float timeOutEachCheck = 8.0f;
        public static float timeOutEachCheckMinBytes = 1.0f;
        private int _total;
        private int _loaded;

        /// <summary>
        /// 默认为1 不出现除0的情况
        /// </summary>
        private int _needLoadListTotalByte = 1;
        private int _loadedTotalBytes = 1;

        private int _threadCount = 4;

        public int total
        {
            get { return _total; }
        }

        public int totalBytes
        {
            get { return _needLoadListTotalByte; }
        }
        public int loadedBytes
        {
            get
            {
                int loadingBytes = 0;
                foreach (DownloadItem downloadItem in loadingList)
                {
                    loadingBytes += downloadItem.downloadedBytes;
                }

                return _loadedTotalBytes+ loadingBytes;
            }
        }

        public static UpdateDownloader getInstance()
        {
            if (instance == null)
            {
                instance = new UpdateDownloader();
            }
            return instance;
        }

        protected virtual void addNeedByURI(HashSizeFile remoteHashSizeFile)
        {
            needLoadList.Add(remoteHashSizeFile);
        }

        private List<HashSizeFile> needLoadList = new List<HashSizeFile>();
        private List<DownloadItem> loadingList = new List<DownloadItem>();
        private List<HashSizeFile> timeOutList = new List<HashSizeFile>();

        /// <summary>
        ///  启动下载
        /// </summary>
        /// <param name="threadCount">几个文件同时开始</param>
        public void start(int threadCount = -1)
        {
            if (threadCount == -1)
            {
                threadCount = Mathf.Max(CoreLoaderQueue.CONCURRENCE, CONCURRENCE);
            }
            _threadCount = threadCount;
            factory = VersionLoaderFactory.GetInstance();

            needLoadList.Clear();
            loadingList.Clear();
            timeOutList.Clear();

            Func<string, bool> fliter = outDownloadFliter;
            if (fliter == null)
            {
                fliter = downloadFliter;
            }

            string prefix = PathDefine.getPersistentLocal();
            string streamingPrefix = PathDefine.getStreamingAssetsLocal();
            foreach (string uri in factory.remoteMapping.Keys)
            {
                if (fliter(uri) == false)
                {
                    continue;
                }
                HashSizeFile remoteMd5 = factory.remoteMapping[uri];
                HashSizeFile localMd5;
                if (factory.localMapping.TryGetValue(uri, out localMd5) == false)
                {
                    addNeedByURI(remoteMd5);
                }
                else if (remoteMd5.hash != localMd5.hash)
                {
                    addNeedByURI(remoteMd5);
                }
                else
                {
                    string filePath = prefix + uri;
                    string streamPath = streamingPrefix + uri;
                    if (File.Exists(filePath) == false && VersionLoaderFactory.GetInstance().localMapping.ContainsKey(uri) == false)
                    {
                        addNeedByURI(remoteMd5);
                    }
                }
            }

            if (needLoadList.Count == 0)
            {
                allComplete();
                return;
            }
            initNeedLoadList();
        }

        protected virtual bool downloadFliter(string uri)
        {
            return true;
        }

        private void initNeedLoadList()
        {
            _loaded = 0;
            _total = needLoadList.Count;

            _needLoadListTotalByte = 1;
            _loadedTotalBytes = 1;

            foreach (HashSizeFile sortFile in needLoadList)
            {
                _needLoadListTotalByte += sortFile.size;
            }

            needLoadList.SortDESC();

            showAlertNeedDownload(total, _needLoadListTotalByte);
        }

        protected virtual void showAlertNeedDownload(int total, int need)
        {
            float m = getByteM(need);

            string msg = "有" + total + "个资源(" + m + "M)文件需要联网更新,是否更新";
            Debug.Log(msg);

            TickManager.Add(Update);
        }

        protected virtual float getByteM(int value)
        {
            float m = (value / 1024f) / 1024f;
            return ((int)m * 1000) / 1000f;
        }

        private void Update(float t)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                this.simpleDispatch(EventX.FAILED, "网络不可访问,请重新检查更新!");
                TickManager.Remove(Update);
                AutoNetWorkListener.Start(() => { TickManager.Add(Update); });
                return;
            }

            if (needLoadList.Count == 0 && loadingList.Count == 0)
            {
                allComplete();
                return;
            }
            while (loadingList.Count < _threadCount && needLoadList.Count > 0)
            {
                HashSizeFile hashSizeFile = needLoadList.Shift();

                string url = getFullPathBy(hashSizeFile.uri) + "?h=" + hashSizeFile.hash;
                DownloadItem downloadItem = new DownloadItem(hashSizeFile,url);
                downloadItem.addEventListener(EventX.PROGRESS, itemProgressHandle);
                downloadItem.addEventListener(EventX.COMPLETE, itemHandle);
                downloadItem.addEventListener(EventX.FAILED, itemHandle);
                loadingList.Add(downloadItem);

                if (isDebug)
                {
                    float m = getByteM(hashSizeFile.size);
                    DebugX.Log("down:" + hashSizeFile.uri + " size: ~" + m + "M");
                }
                downloadItem.start();
            }
        }

        private string getFullPathBy(string uri)
        {
            return _VersionHttpPrefix + uri.Substring(1); //+"?t="+time;
        }

        private void itemProgressHandle(EventX e)
        {
            this.simpleDispatch(EventX.PROGRESS, loadedBytes / (float)_needLoadListTotalByte);
        }

        private void itemHandle(EventX e)
        {
            DownloadItem downloadItem = (DownloadItem)e.target;
            downloadItem.removeEventListener(EventX.COMPLETE, itemHandle);
            downloadItem.removeEventListener(EventX.FAILED, itemHandle);
            HashSizeFile hashSizeFile = downloadItem.HashSizeFile;
            loadingList.Remove(downloadItem);

            string uri = hashSizeFile.uri;
            if (e.type != EventX.COMPLETE)
            {
                string error = (string)e.data;
                //不存在的文件就不管了
                if (error != "404")
                {
                    timeOutList.Add(hashSizeFile);
                }
                DebugX.LogWarning("updater:" + uri + " error:" + error);
                return;
            }

            byte[] bytes = (byte[])e.data;
            if (bytes.Length > 0)
            {
                _loadedTotalBytes += bytes.Length;
                string localPath = factory.getLocalPathByURL(getFullPathBy(uri), true);
                string hashKey = "/" + localPath;
                string filePath = PathDefine.getPersistentLocal(localPath);
                try
                {
                    FileHelper.AutoCreateDirectory(filePath);
                    File.WriteAllBytes(filePath, bytes);

                    string md5 = MD5Util.MD5Byte9(bytes);
                    HashSizeFile localHashSizeFile = null;
                    if (factory.localMapping.TryGetValue(hashKey, out localHashSizeFile)==false)
                    {
                        localHashSizeFile=new HashSizeFile(hashKey);
                        factory.localMapping.Add(hashKey,localHashSizeFile);
                    }
                    localHashSizeFile.hash = md5;
                }
                catch (Exception ex)
                {
                    timeOutList.Add(hashSizeFile);
                    DebugX.LogWarning("writeFileError:" + ex.Message);
                }
            }
            else
            {
                timeOutList.Add(hashSizeFile);
                DebugX.LogWarning("writeFileError:文件大小为0");
            }

            this.simpleDispatch(EventX.PROGRESS, loadedBytes / (float)_needLoadListTotalByte);

            _loaded++;
            if (_loaded % 10 == 0)
            {
                factory.saveLocalMapping();
            }
            else
            {
                factory.isInvalidate = true;
            }
        }
        private void allComplete()
        {
            factory.saveLocalMapping();

            if (timeOutList.Count > 0)
            {
                foreach (HashSizeFile s in timeOutList)
                {
                    needLoadList.Add(s);
                }
                timeOutList.Clear();
                return;
            }

            TickManager.Remove(Update);

            BaseApp.ClearMemory(true);

            this.simpleDispatch(EventX.COMPLETE);
            DebugX.Log("updateDownloader all complete!");
        }
    }
}