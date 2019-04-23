namespace gameSDK
{
    public class PlatformPayData
    {
        /// <summary>
        /// 平台码
        /// </summary>
        public string platform;
        public string platformCode;
        /// <summary>
        /// 服务器信息
        /// </summary>
        public string serverId;
        public string userId;
        public string roleId;

        /// <summary>
        /// 商品信息
        /// </summary>
        public string shangpin_id;
        public int rmb;
        public string name;
        public string orderId;

        /// <summary>
        /// 苹果物品id;
        /// </summary>
        public string apple_id;

        /// <summary>
        /// 购买回调地址
        /// </summary>
        public string callbackURL;
    }
}