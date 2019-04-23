using foundation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public interface IPlatformSDKImp
    {
        bool call(string key, string value = "");
    }

    public class PlatformSDK
    {
        public static bool _initailized=false;
        public static IPlatformSDKImp platformSDKImp;
        private static Dictionary<string, List<Action<string>>> listenerMap =
            new Dictionary<string, List<Action<string>>>();

        public static bool Initialization(string gameVer = "1.0", string platformID = "zq", string code = "")
        {
            if (_initailized|| Application.isEditor)
            {
                return false;
            }
            _initailized = true;
            bool b=Call("init", gameVer + "|" + platformID + "|" + code);

            AddListener("exception", exceptionHandle);
            AddListener("log", logHandle);
            return b;
        }

        public static bool Call(string key, string value =null)
        {
            if (_initailized == false)
            {
                DebugX.Log("PlatformSDK not initailized! {0}:{1}",key,value);
                return false;
            }

            if (platformSDKImp == null)
            {
                DebugX.Log("PlatformSDK not implement! {0}:{1}", key, value);
                return false;
            }

            if (value == null)
            {
                value = "";
            }

            key = key.ToLower();

            DebugX.Log("callSDK:" + key + " value:" + value);
            return platformSDKImp.call(key, value);
        }
        public static bool Pay(string json)
        {
            GC.Collect();
            return Call("pay", json);
        }
        public static bool Login(string json="")
        {
            return Call("login", json);
        }

        public static bool Logout(string json="")
        {
            return Call("logout", json);
        }

        public static bool OpenWeb(string url, string json = "")
        {
            return Call("openWeb", url);
        }

        private static void exceptionHandle(string value)
        {
            DebugX.LogError("sdkError:" + value);
        }

        private static void logHandle(string value)
        {
            DebugX.Log("sdkLog:" + value);
        }
        public static bool AddListener(string cmd, Action<string> handler)
        {
            List<Action<string>> map = null;
            cmd=cmd.ToLower();
            if (listenerMap.TryGetValue(cmd, out map) == false)
            {
                map = new List<Action<string>>();
                listenerMap.Add(cmd, map);
            }

            if (map.IndexOf(handler) == -1)
            {
                map.Add(handler);
                return true;
            }

            return false;
        }

        public static bool RemoveListener(string cmd, Action<string> handler)
        {
            List<Action<string>> map = null;
            cmd = cmd.ToLower();
            if (listenerMap.TryGetValue(cmd, out map) == false)
            {
                return false;
            }

            int index = map.IndexOf(handler);
            if (index == -1)
            {
                map.RemoveAt(index);
                return true;
            }

            return false;
        }

        public static bool Receive(string key, string args)
        {
            List<Action<string>> map = null;
            key = key.ToLower();
            if (listenerMap.TryGetValue(key, out map) == false)
            {
                return false;
            }

            foreach (Action<string> action in map)
            {
                action(args);
            }

            return true;
        }
    }
}