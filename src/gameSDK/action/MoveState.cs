namespace gameSDK
{
    public enum MoveState
    {
        /// <summary>
        /// 现在不可以移动
        /// </summary>
        NOW_LOCK,

        /// <summary>
        /// 在目标点不用寻路 
        /// </summary>
        AT_RESULT,

        /// <summary>
        /// 不可到达 
        /// </summary>
        UNREACHABLE,

        /// <summary>
        /// 开始寻路 
        /// </summary>
        START_MOVE

    }
}