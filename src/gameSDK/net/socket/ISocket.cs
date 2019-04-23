using System;

namespace foundation
{
    public interface ISocket
    {
        bool send(IMessageExtensible message);

        bool isConnected { get; }
        void close();

        void addListener(int CMD, Action<IMessageExtensible> handler);

        void addListenerOnce(int CMD, Action<IMessageExtensible> handler);

        void removeListener(int CMD, Action<IMessageExtensible> handler);

        void route(IMessageExtensible msg);

    }
}
