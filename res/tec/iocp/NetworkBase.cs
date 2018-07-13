using AsyncIO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Network
{
    public class NetUtil
    {
        public static string ToIportString(IPEndPoint point)
        {
            return $"{point.Address.ToString()}:{point.Port}";
        }
    }
    public class NetDef
    {
        public static int MAX_PACKAGE_SIZE = 1024;
        
    }
    public class NetData
    {
        public static Dictionary<String, Queue<byte[]>> m_ip_point_datas = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<String, AsyncSocket> m_ip_port_sockets = new Dictionary<string, AsyncSocket>();

        private static object m_lock = new object();

        public static void RemoveSocket(String ip_port)
        {
            lock(m_lock)
            {
                m_ip_point_datas.Remove(ip_port);
                m_ip_port_sockets.Remove(ip_port);
            }
        }
        public static void ReceiveData(String ip_port)
        {
            if (!m_ip_port_sockets.ContainsKey(ip_port))
            {
                Console.WriteLine("ReceiveData error");
                return;
            }

            lock(m_lock)
            {
                AsyncSocket socket = m_ip_port_sockets[ip_port];
                byte[] receive_buffer = new byte[NetDef.MAX_PACKAGE_SIZE];
                if (NetData.m_ip_point_datas.ContainsKey(ip_port))
                {
                    NetData.m_ip_point_datas[ip_port].Enqueue(receive_buffer);
                }
                else
                {
                    Queue<byte[]> queue = new Queue<byte[]>();
                    queue.Enqueue(receive_buffer);
                    NetData.m_ip_point_datas[ip_port] = queue;
                }
                socket.Receive(receive_buffer);
            }
        }
        public static void ReceiveData(AsyncSocket socket)
        {
            string ip_port = NetUtil.ToIportString(socket.RemoteEndPoint);
            lock(m_lock)
            {
                m_ip_port_sockets[ip_port] = socket;
            }
            ReceiveData(ip_port);
        }

        //test beg
        public static int error_count = 0;
        public static void printfun()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"receive no data ... count: {error_count}");
            }
        }
        //test end
        public static Thread m_thread = null;
        public static bool GetReceivedData(out INetPackage pkg, String ip_port, int bytes)
        {
            lock (m_lock)
            {
                if (NetData.m_ip_point_datas.ContainsKey(ip_port))
                {
                    if (NetData.m_ip_point_datas[ip_port].Count > 0)
                    {
                        String data = Encoding.UTF8.GetString(NetData.m_ip_point_datas[ip_port].Dequeue(), 0, bytes);
                        StringPackage newpkg = new StringPackage();
                        newpkg.Data = data;
                        pkg = newpkg;
                        return true;
                    }
                    else
                    {
                        //test beg
                        ++error_count;
                        Console.WriteLine($"receive no data ... count: {error_count}");
                        if (null == m_thread)
                        {
                            m_thread = new Thread(printfun);
                            m_thread.Start();
                        }
                        //test end
                    }
                }
            }

            pkg = new StringPackage();
            return false;
        }
    }
    public class StringPackage : INetPackage
    {
        public string Data
        {
            get;
            set;
        }

        public string Description()
        {
            return Data;
        }
    }

    public interface INetPackage
    {
        string Description();
    }

    public delegate void Accepted(IPEndPoint local_point, IPEndPoint remote_point);
    public delegate void Connected(IPEndPoint local_point, IPEndPoint remote_point);
    public delegate void Sent(IPEndPoint local_point, IPEndPoint remote_point, INetPackage package);
    public delegate void Received(IPEndPoint local_point, IPEndPoint remote_point, INetPackage package);
    public delegate void Disconnected(IPEndPoint local_point, IPEndPoint remote_point);

    public enum SelectorType
    {
        Unknown = 0,
        IOCP,
    }
    public interface INetSelector
    {
        SelectorType Type();

        void Initlalize(int timeout_ms);

        void Start();

        void Stop();

        void AssociateSocket(IDisposable socket, object state = null);

        void RegAcceptedDelegate(Accepted accepted);

        void RegConnectedDelegate(Connected connected);

        void RegSentDelegate(Sent sent);

        void RegReceivedDelegate(Received received);

        void RegDisconnectedDelegate(Disconnected disconnected);
    }

    public interface INetIO
    {
        SelectorType NetSelectorType();

        INetSelector Selector();

        void Initialize(SelectorType type, int timeout_ms = 500);

        void Start();

        void Stop();
    }
}
