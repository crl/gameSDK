using foundation;
using System;
using System.Collections.Generic;

namespace gameSDK
{
    public class ApplicationVersion
    {
        public static string version = "1.0.0";
        public static string platform = "local";
        public static string configKey = "";
        public static string code = "";
        public static string apkName = "";
        public static int channelID = 0;
        public static string channelName = "";

        public static bool isDebug = true;
        public static bool isFileServerOk = true;

        /// <summary>
        /// 是否为版署版本
        /// </summary>
        public static bool isBanshu
        {
            get { return platform == "banshu"; }
        }

        protected static Dictionary<string, string> shellDic;
        internal static void InjectByShell(Dictionary<string, string> dic)
        {
            try
            {
                dic.TryGetValue("version", out version);
                dic.TryGetValue("platform", out platform);
                dic.TryGetValue("configKey", out configKey);

                string value = null;
                dic.TryGetValue("isDebug", out value);
                //DebugX.Log("isDebug:" + value);
                isDebug = (int.Parse(value) == 1);

                dic.TryGetValue("isFileServerOk", out value);
                //DebugX.Log("isFileServerOk:" + value);
                isFileServerOk = (int.Parse(value) == 1);

                dic.TryGetValue("apkName", out apkName);

                shellDic = dic;
            }
            catch (Exception ex)
            {
                DebugX.Log("InjectByShell:" + ex.Message);
            }
        }
    }
}