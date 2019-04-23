namespace foundation
{
    public interface ILoadingUI
    {
        void show();

        /// <summary>
        /// 前缀
        /// </summary>
        /// <param name="value"></param>
        void setPrefix(string value);

        /// <summary>
        /// 后缀
        /// </summary>
        /// <param name="value"></param>
        void setSuffix(string value);

        void progress(float v);
        void hide();
    }
}