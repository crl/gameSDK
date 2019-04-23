using System;
using foundation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace gameSDK
{


    public class UITools
    {

        private static Dictionary<int, Sprite> btnBackGround = new Dictionary<int, Sprite>();

        private static Dictionary<int, Sprite> moneySprite = new Dictionary<int, Sprite>();

        private static string[] numStr = new string[] {"零", "一", "二", "三", "四", "五", "六", "七", "八", "九"};

        /// <summary>
        /// 添加公用icon image背景
        /// </summary>
        public static void AddBackGroundSprite(int i, Sprite sprite)
        {
            if (btnBackGround.ContainsKey(i) == false)
            {
                btnBackGround.Add(i, sprite);
            }
        }

        /// <summary>
        /// 获得公用icon image背景
        /// </summary>
        /// <param name="quatity"></param>
        /// <returns></returns>
        public static Sprite GetBackGroundSprite(int quatity)
        {
            if (btnBackGround.ContainsKey(quatity))
            {
                return btnBackGround[quatity];
            }

            return null;
        }

        public static void AddMoneySprite(int type, Sprite sprite)
        {
            if (moneySprite.ContainsKey(type) == false)
            {
                moneySprite.Add(type, sprite);
            }
        }

        public static Sprite GetMoneySprite(int type)
        {
            if (moneySprite.ContainsKey(type) == true)
            {
                return moneySprite[type];
            }

            return null;
        }

        public static string NumToString(long a, bool showFloat = true)
        {
            string coinMes = a.ToString();
            if (a >= 10000)
            {
                coinMes = a / 10000 + "";
                long b = a % 10000 / 100;
                if (b > 0 && showFloat)
                {
                    coinMes += "." + (b >= 10 ? b + "" : "0" + b);
                }

                coinMes += "万";
            }

            if (a >= 100000000)
            {
                coinMes = a / 100000000 + "";
                long b = a % 100000000 / 1000000;
                if (b > 0 && showFloat)
                {
                    coinMes += "." + (b >= 10 ? b + "" : "0" + b);
                }

                coinMes += "亿";
            }

            return coinMes;
        }

        /// <summary>
        /// 设置一个物体变灰与否
        /// </summary>
        public static void SetImageGray(GameObject skin, bool includeChild, bool isGray)
        {
            if (includeChild)
            {
                Image[] temp = skin.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i].material = UIUtils.CreatShareGrayMaterial();
                    temp[i].color = new Color(isGray ? 0 : 1, 1, 1, isGray ? 0.7f : 1);
                }
            }
            else
            {
                Image temp = skin.GetComponent<Image>();
                temp.material = UIUtils.CreatShareGrayMaterial();
                temp.color = new Color(isGray ? 0 : 1, 1, 1, isGray ? 0.7f : 1);
            }
        }

        private static List<RaycastResult> raycastResult = new List<RaycastResult>();

        /// <summary>
        /// 是否点击到了该gameobject
        /// </summary>
        /// <returns></returns>
        public static bool IsClickSkin(Vector3 position, GameObject go)
        {
            EventSystem eventSystem = EventSystem.current;

            GraphicRaycaster ray = go.GetComponent<GraphicRaycaster>();
            if (ray == null)
            {
                ray = BaseApp.GraphicRaycaster;
            }
            PointerEventData pointData = new PointerEventData(eventSystem);
            pointData.position = position;
            pointData.pressPosition = position;
            raycastResult.Clear();
            ray.Raycast(pointData, raycastResult);

            if (raycastResult.Count == 0)
            {
                return false;
            }

            GameObject clickObj = raycastResult[0].gameObject;
            bool isCkickSkin = false;
            Transform parent = clickObj.transform;
            while (parent != null)
            {
                if (parent.gameObject == go)
                {
                    isCkickSkin = true;
                    break;
                }
                else
                {
                    parent = parent.parent;
                }
            }

            return isCkickSkin;
        }

        /// <summary>
        /// 获取字符串的字节长度，汉子占2字节
        /// </summary>
        /// <returns></returns>
        public static int GetTextLength(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            int len = System.Text.Encoding.UTF8.GetBytes(str).Length;
            if (len <= 0)
            {
                len = str.Length;
            }

            return len;
        }

        /// <summary>
        /// ugui 文本变色处理，color 16进制颜色值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetColorHtml(string str, string color)
        {
            if (color.IndexOf("#") != 0)
            {
                color = "#" + color;
            }

            return StringUtil.substitute("<color={0}>{1}</color>", color, str);
        }

        /// <summary>
        /// 图文混排，超链接文本解析，actionStr 超链接点击文本返回，color 超链接颜色
        /// </summary>
        /// <param name="str"></param>
        /// <param name="actionStr"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetHrefHtml(string str, string actionStr, string color)
        {
            if (color.IndexOf("#") != 0)
            {
                color = "#" + color;
            }

            return "<a href=[" + actionStr + "]><color=" + color + ">" + str + "</color></a>";
        }

        /// <summary>
        /// 阿拉伯数字转换中文数字
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetChineseNum(int number)
        {
            string res = "";
            string str = number.ToString();
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                int c = int.Parse(str[i].ToString());
                string s = numStr[c];
                if (c != 0)
                {
                    switch (length - i)
                    {
                        case 2:
                        case 6:
                            if (c == 1 && str.Length == 2)
                            {
                                s = "";
                            }

                            s += "十";
                            break;
                        case 3:
                        case 7:
                            s += "百";
                            break;
                        case 4:
                        case 8:
                            s += "千";
                            break;
                        case 5:
                            s += "万";
                            break;
                        case 9:
                            s += "亿";
                            break;
                        default:
                            s += "";
                            break;
                    }
                }

                if (s != "零" || res.Length == 0 || res[res.Length - 1] != '零')
                {
                    res += s;
                }
            }

            while (res.Length > 1 && res[res.Length - 1] == '零')
            {
                res = res.Substring(0, res.Length - 1);
            }

            return res;
        }

        /// <summary>
        /// 将格式转换为货币各式：1,000,000
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string GetCurrentFormat(long num)
        {
            return string.Format("{0:N0}", num);
        }


        public static string GetNumberFormat(long size)
        {
            float num = 1024.00f; //byte

            if (size < num)
                return size + "";
            if (size < Math.Pow(num, 2))
                return (size / num).ToString("f2") + "K"; //kb
            if (size < Math.Pow(num, 3))
                return (size / Math.Pow(num, 2)).ToString("f2") + "M"; //M
            if (size < Math.Pow(num, 4))
                return (size / Math.Pow(num, 3)).ToString("f2") + "G"; //G

            return (size / Math.Pow(num, 4)).ToString("f2") + "T"; //T
        }
    }
}