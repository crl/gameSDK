namespace foundation
{
    public interface ISocketSender:IEventDispatcher
    {
        bool send(IMessageExtensible message);

        bool connected { get; }
        void close();

        SocketRouter router
        {
            get; set; }

        bool checkRetry(IMessageExtensible remoteMessage);
    }
}