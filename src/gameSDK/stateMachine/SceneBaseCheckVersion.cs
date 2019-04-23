using clayui;
using foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 自动更新流程 调用堆栈
    /// awken->preCheckPlatform->[forecIOSMode]->checkUnZip->doNext->doStart->(*)checkResourceVersion->downloader.start->versionUpdateComplete
    /// </summary>
    public abstract class SceneBaseCheckVersion : SceneBase
    {
        public const string NAME = "SceneCheckVersion";
        public const string UNZIP_VERSION = "unzipVersion";
       
        private UpdateDownloader downloader;
        public SceneBaseCheckVersion() : base(NAME)
        {
            downloader = UpdateDownloader.getInstance();
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void preAwaken()
        {
            if (Application.isEditor)
            {
                string operatingSystem = SystemInfo.operatingSystem.ToLower();
                DebugX.Log("operatingSystem:" + operatingSystem);
                if (operatingSystem.IndexOf("mac") != -1)
                {
                    forceIOSMode();
                }
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                forceIOSMode();
            }
        }
        protected virtual void forceIOSMode()
        {
            ApplicationVersion.platform = "apple";
            ApplicationVersion.isDebug = false;
            PathDefine.forceSetPlatformFolderName = "iOS";
        }

        public override void awaken()
        {
            preAwaken();
            string v =
                StringUtil.substitute(
                    "platform:{0}\tapkVersion:{1}\tisDebug:{2}\tcodeVersion:{3}\tisMobilePlatform:{4}",
                    ApplicationVersion.platform,
                    ApplicationVersion.version,
                    ApplicationVersion.isDebug,
                    gameSDK.CodeVersion.version,
                    Application.isMobilePlatform
                );

            v += StringUtil.substitute(
                "\tdeviceModel:{0}\tgraphicsDeviceName:{1}\tgraphicsDeviceType:{2}\tsystemMemorySize:{3}\tgraphicsMultiThreaded:{4}\tsupportedRenderTargetCount:{5}\t:screenW:{6}\t:screenH:{7}",
                SystemInfo.deviceModel,
                SystemInfo.graphicsDeviceName,
                SystemInfo.graphicsDeviceType,
                SystemInfo.systemMemorySize,
                SystemInfo.graphicsMultiThreaded,
                SystemInfo.supportedRenderTargetCount,
                Screen.width,
                Screen.height
            );

            v += StringUtil.substitute("\t localPath:{0}", PathDefine.getPersistentLocal(""));
            DebugX.Log(v);

            showLoading("检查更新配置");

            if (ApplicationVersion.isFileServerOk)
            {
                string url = getUpdateXMLURL();
                DebugX.Log("加载updateXML:" + url);
                AssetResource resource = AssetsManager.getResource(url, LoaderXDataType.BYTES);
                AssetsManager.bindEventHandle(resource, onUpdateXMLHandle);
                resource.addEventListener(EventX.PROGRESS, eventProgressHandle);
                resource.timeout = 7;
                resource.isForceRemote = true;
                resource.load();
            }
            else
            {
                doNext();
            }
        }

        protected void onUpdateXMLHandle(EventX e)
        {
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, onUpdateXMLHandle, false);
            resource.removeEventListener(EventX.PROGRESS, eventProgressHandle);

            if (e.type == EventX.COMPLETE)
            {
                XmlDocument doc = new XmlDocument();
                MemoryStream stream = new MemoryStream((byte[]) resource.data);
                try
                {
                    doc.Load(stream);
                    XmlNode node = doc.SelectSingleNode("config");
                    ConfigurationUtil.initConfigXML(node.ChildNodes);
                }
                catch (Exception ex)
                {
                    DebugX.Log("updateXML格式错误:" + ex.Message);
                    onUpdateXMLFaildHandle();
                }
            }
            else
            {
                onUpdateXMLFaildHandle();
                DebugX.LogWarning("updateXML 加载失败");
            }
            doNext();
        }

        protected virtual void onUpdateXMLFaildHandle()
        {

        }

        /// <summary>
        /// 复制列表
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="error"></param>
        private void onPackageVersionHandle(EventX e)
        {
            AssetResource versionResource = e.target as AssetResource;
            AssetsManager.bindEventHandle(versionResource, onPackageVersionHandle);
            versionResource.addEventListener(EventX.PROGRESS, eventProgressHandle);
            string versionName = PathDefine.getPlatformVersionFileName();

            if (e.type!=EventX.COMPLETE)
            {
                DebugX.Log(versionName + " Error:" + e.data+ " ApplicationVersion:" + ApplicationVersion.version);
                doNext();
                return;
            }
            showLoading("开始解压资源");

            string versionV = Encoding.UTF8.GetString((byte[]) e.data);
            Dictionary<string, HashSizeFile> localMapping = new Dictionary<string, HashSizeFile>();

            string path = PathDefine.getStreamingAssetsLocal(versionName);

            VersionLoaderFactory.ParserVersion(versionV, localMapping);

            AutoCopyer autoCopyer = new AutoCopyer();
            autoCopyer.addEventListener(EventX.COMPLETE, unZipCompleteHandle);
            autoCopyer.addEventListener(EventX.PROGRESS, eventProgressHandle);
            autoCopyer.copyFromTO(PathDefine.getStreamingAssetsLocal("", true), PathDefine.getPersistentLocal());
            autoCopyer.start(localMapping);
        }


        private void alertAPKHandle(AlertResult code)
        {
            if (code!= AlertResult.OK)
            {
                DebugX.Log("取消了下载");
                return;
            }
            string url = ConfigurationUtil.getResource("apk");
            DebugX.Log("下载APK:" + url);

            if (string.IsNullOrEmpty(url))
            {
                if (url.IndexOf("http://")!=-1)
                {
                    Application.OpenURL(url);
                }
                PlatformSDK.Call("home", url);
            }
            else
            {
                UITip.Show("版本配置错误,请联系客服人员!");
            }
        }

        private void eventProgressHandle(EventX e)
        {
            setLoadingProgress((float)e.data);
        }

        private void unZipCompleteHandle(EventX e)
        {
            GC.Collect();
            AutoCopyer autoCopyer = e.target as AutoCopyer;
            autoCopyer.removeEventListener(EventX.COMPLETE, unZipCompleteHandle);
            autoCopyer.removeEventListener(EventX.PROGRESS, eventProgressHandle);

            PlayerPrefs.SetString(UNZIP_VERSION, ApplicationVersion.version);
            PlayerPrefs.Save();

            string versionName = PathDefine.getPlatformVersionFileName();
            string fullPath = PathDefine.getPersistentLocal(versionName);
            string versionV = FileHelper.GetUTF8Text(fullPath);
            VersionLoaderFactory.SavePackageVersion(versionV);

            doNext();
        }
        protected virtual void doNext()
        {
            if (PlayerPrefs.GetString(VersionLoaderFactory.KEY) == ApplicationVersion.version)
            {
                VersionLoaderFactory.GetInstance().initLocal();
                doStart();
            }
            else
            {
                string versionName = PathDefine.getPlatformVersionFileName();
                string fullPath =PathDefine.getStreamingAssetsLocal(versionName,true);
                AssetResource resource = AssetsManager.getResource(fullPath, LoaderXDataType.BYTES);
                DebugX.Log("localVersion "+resource.url);
                AssetsManager.bindEventHandle(resource, localVersionHandle);
                resource.addEventListener(EventX.PROGRESS, eventProgressHandle);
                resource.load();
            }
         
        }

        protected void localVersionHandle(EventX e)
        {
            ///永远写入
            PlayerPrefs.SetString(VersionLoaderFactory.KEY, ApplicationVersion.version);

            AssetResource resource = (AssetResource)e.target;
            AssetsManager.bindEventHandle(resource, localVersionHandle,false);
            resource.removeEventListener(EventX.PROGRESS, eventProgressHandle);
            if (e.type == EventX.COMPLETE)
            {
                DebugX.Log("localVersionHandle complete");
                string versionV = Encoding.UTF8.GetString((byte[]) e.data);
                VersionLoaderFactory.SavePackageVersion(versionV);
            }

            VersionLoaderFactory.GetInstance().initLocal();
           
            doStart();
        }

        protected virtual void doStart()
        {
            string versionHttpPrefix = ConfigurationUtil.getPrefix("resourceServer");
            string preHash = ConfigurationUtil.getPrefix("preHash");
            checkResourceVersion(versionHttpPrefix, preHash);
        }

        protected void checkResourceVersion(string versionHttpPrefix, string preHash)
        {
            VersionLoaderFactory.SetPreHashValue(preHash);
            UpdateDownloader.SetVersionHttpPrefix(versionHttpPrefix);
            string v = "versionHttpPrefix:" + versionHttpPrefix + " \tpreHash:" + preHash;
            DebugX.Log(v);

            showLoading("检查版本文件");

            if (ConfigurationUtil.getPrefix("unzipPack") == "1")
            {
                if (PlayerPrefs.GetString(UNZIP_VERSION) != ApplicationVersion.version)
                {
                    DebugX.LogWarning("开始解压资源");
                    ///收集PAK复制列表
                    string versionName = PathDefine.getPlatformVersionFileName();
                    string zipPath = PathDefine.getStreamingAssetsLocal(versionName, true);

                    AssetResource versionResource = AssetsManager.getResource(zipPath, LoaderXDataType.BYTES);
                    AssetsManager.bindEventHandle(versionResource, onPackageVersionHandle);
                    versionResource.addEventListener(EventX.PROGRESS, eventProgressHandle);
                    versionResource.load();
                    return;
                }
            }

            if (Application.isMobilePlatform)
            {
                AbstractSection section = ConfigurationUtil.getResourceSection("apk");
                if (section != null)
                {
                    string remoteVersion = section.version;
                    if (VersionUtils.Get(remoteVersion) > VersionUtils.Get(ApplicationVersion.version))
                    {
                        apkVersionHandle(remoteVersion);
                    }
                }
            }

            if (ApplicationVersion.isFileServerOk)
            {
                string uri = versionHttpPrefix + PathDefine.getPlatformVersionFileName();
                AssetResource resource = AssetsManager.getResource(uri, LoaderXDataType.BYTES);
                AssetsManager.bindEventHandle(resource, remoteVersionHandle);
                resource.addEventListener(EventX.PROGRESS, eventProgressHandle);
                resource.isForceRemote = true;
                resource.timeout = 6;
                resource.load(0, true);
            }
            else
            {
                versionUpdateComplete();
            }
        }
        protected void remoteVersionHandle(EventX e)
        {
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, remoteVersionHandle, false);
            resource.addEventListener(EventX.PROGRESS, eventProgressHandle);
            if (e.type != EventX.COMPLETE)
            {
                DebugX.Log("remote v.dat error:" + e.data);
                versionUpdateComplete();
                return;
            }
            byte[] bytes = e.data as byte[];
            string v = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrEmpty(v) == false)
            {
                VersionLoaderFactory.GetInstance().initRemote(v);
            }

            showLoading("下载更新资源...");
            downloader.addEventListener(EventX.FAILED, versionUpdateFailed);
            downloader.addEventListener(EventX.PROGRESS, versionProgressHandle);
            downloader.addEventListener(EventX.COMPLETE, versionUpdateComplete);

            //todo 手机上如果卡住 还是退回1;
            int thread = CoreLoaderQueue.CONCURRENCE;
            if (Application.isEditor)
            {
                thread = 20;
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    thread = 5;
                }
            }
            downloader.start(thread);
        }

        private void versionProgressHandle(EventX e)
        {
            string v = getLoadedBytesFormat(downloader.loadedBytes, downloader.totalBytes);
            setLoadingProgress((float) e.data);
            setLoadingProgressText(v);
        }

        private void versionUpdateFailed(EventX e)
        {
            string v = (string)e.data;
            setLoadingProgressText(v);
        }
        protected virtual string getLoadedBytesFormat(float loadedBytes, float totalBytes)
        {
            loadedBytes = loadedBytes / 1024f / 1024f;
            totalBytes = totalBytes / 1024f / 1024f;
            return "更新(" + loadedBytes.ToString("f1") + "M/" + totalBytes.ToString("f1") + "M)";
        }

     
        protected void apkVersionHandle(string remoteVersion)
        {
            DebugX.Log("版本检查:" + remoteVersion + "/" + ApplicationVersion.version);

            Alert alert = Alert.getInstance();
            Alert.ShowOKOrNO("检测到新版本:" + remoteVersion, alertAPKHandle);
        }
        protected virtual void versionUpdateComplete(EventX e = null)
        {
            this.sleep();
        }

        /// <summary>
        /// 更新的配置文件
        /// </summary>
        /// <returns></returns>
        protected abstract string getUpdateXMLURL();
        protected abstract void showLoading(string v);
        protected abstract void setLoadingProgress(float v);
        protected abstract void setLoadingProgressText(string v);
    }
}