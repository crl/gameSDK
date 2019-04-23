using foundation;
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class PlatformReporter
    {
        public const string KEY = "PlatformReporter";
        public static string StepKey = "node";
        private static string GateURL;
        public static bool Enabled=false;
        private static string Version;
        private static HashSet<int> HasSet=new HashSet<int>();
        public static void Init(string gateURL,string version="1.0")
        {
            GateURL = gateURL;
            Version = version;
            if (PlayerPrefs.GetString(KEY) != version)
            {
                Enabled = true;
            }
        }
        public static void Step(int key, string value,bool forceReplace=false)
        {
            if (Enabled)
            {
                if (HasSet.Contains(key) && forceReplace == false)
                {
                    return;
                }
                HasSet.Add(key);

                string url = StringUtil.substitute("{0}?{1}={2}&{3}&v={4}", GateURL,StepKey, key, value, Version);
                DebugX.LogWarning("sendReporter:{0}", url);

                AssetResource resource = AssetsManager.getResource(url, LoaderXDataType.GET);
                AssetsManager.bindEventHandle(resource, completeHandle);
                resource.isForceRemote = true;
                resource.timeout = 3;
                resource.load();
            }
        }

        private static void completeHandle(EventX e)
        {
            AssetResource resource =e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, completeHandle,false);

            AssetsManager.dispose(resource.url);
        }

        public static void End(bool saveIt=true)
        {
            if (saveIt)
            {
                Save();
            }
            HasSet.Clear();
            Enabled = false;
        }
        public static void Save()
        {
            if (Enabled)
            {
                PlayerPrefs.SetString(KEY, Version);
            }
        }
    }
}