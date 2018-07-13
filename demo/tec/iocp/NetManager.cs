using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using AsyncIO;

namespace Network
{
    class NetManager
    {
        public void PrintLog(String description, IPEndPoint local_point, IPEndPoint remote_point, INetPackage package = null)
        {
            if (null != local_point)
            {
                Console.Write( $"{description}: local:{local_point.Address.ToString()}:{local_point.Port} ");
            }
            if (null != remote_point)
            {
                Console.Write($"remote: {remote_point.Address.ToString()}:{remote_point.Port}");
            }
            if (null != package)
            {
                Console.Write($"data: {package.Description()}");
            }
            Console.Write(Environment.NewLine);
        }

        public void PrintAcceptLog(IPEndPoint local_point, IPEndPoint remote_point)
        {
            PrintLog("Accepted", local_point, remote_point);
        }
        public void PrintConnectedLog(IPEndPoint local_point, IPEndPoint remote_point)
        {
            PrintLog("Connected", local_point, remote_point);
        }
        public void PrintDisconnectedLog(IPEndPoint local_point, IPEndPoint remote_point)
        {
            PrintLog("Disconnected", local_point, remote_point);
        }
        public void PrintSentLog(IPEndPoint local_point, IPEndPoint remote_point, INetPackage package)
        {
            PrintLog("Sent", local_point, remote_point, package);
        }
        public void PrintReceiveLog(IPEndPoint local_point, IPEndPoint remote_point, INetPackage package)
        {
            PrintLog("Received", local_point, remote_point, package);
        }

        public void Initialize(bool isServer = false)
        {
            NetCommonIO netIO = new NetCommonIO();
            netIO.Initialize(SelectorType.IOCP, 500);

            INetSelector selector =  netIO.Selector();
            if (null != selector)
            {
                selector.RegAcceptedDelegate(PrintAcceptLog);
                selector.RegConnectedDelegate(PrintConnectedLog);
                selector.RegSentDelegate(PrintSentLog);
                selector.RegReceivedDelegate(PrintReceiveLog);
                selector.RegDisconnectedDelegate(PrintDisconnectedLog);
                selector.Start();

                if (!isServer)
                {
                    //client
                    List<ClientThreadT> client_threads = new List<ClientThreadT>();

                    Random rand = new Random(Guid.NewGuid().GetHashCode());
                    for (int i = 0; i < 100; ++i)
                    {
                        SelItem clientItem = new SelItem();
                        AsyncSocket clientSocket = AsyncSocket.CreateIPv4Tcp();
                        selector.AssociateSocket(clientSocket, clientItem);
                        clientSocket.Bind(IPAddress.Any, 0);
                        Console.WriteLine($"Client ip:{NetUtil.ToIportString(clientSocket.LocalEndPoint)}");

                        int name = rand.Next();
                        ClientThreadT thread = new ClientThreadT(clientSocket, clientItem, name.ToString());
                        client_threads.Add(thread);
                    }

                    Console.WriteLine("Wait client threads to be end ...");

                    bool finished = false;
                    while (!finished)
                    {
                        finished = true;
                        int count = 0;
                        foreach (var thread in client_threads)
                        {
                            ++count;
                            if (!thread.finished())
                            {
                                finished = false;
                                Thread.Sleep(50);
                                break;
                            }
                        }
                        if (finished)
                            break;

                    }

                    Console.WriteLine("Client threads all finished.");
                    selector.Stop();
                }
                else
                {
                    //server
                    try
                    {
                        SelItem accept_item = new SelItem();
                        AsyncSocket listener = AsyncSocket.CreateIPv4Tcp();
                        selector.AssociateSocket(listener, accept_item);
                        listener.Bind(IPAddress.Any, 56665);
                        listener.Listen(int.MaxValue);

                        Console.WriteLine("Server started :");

                        //begin to accept
                        while (true)
                        {
                            listener.Accept();
                            accept_item.Handle.WaitOne();

                            var serverSocket = listener.GetAcceptedSocket();
                            SelItem server_item = new SelItem();
                            selector.AssociateSocket(serverSocket, server_item);

                            NetData.ReceiveData(serverSocket);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }

    public class ClientThreadT
    {
        //test beg
        public static int active_thread_count = 0;
        public static int waiting_send_count = 0;
        public static int send_failed = 0;
        public static int wait_connect_count = 0;
        public static int connect_falied = 0;
        //test end

        public Thread m_thread;
        public AsyncSocket m_clientSocket;
        public SelItem m_clientItem;
        public string m_name;

        public ClientThreadT(AsyncSocket clientSocket, SelItem clientItem, String name)
        {
            m_clientSocket = clientSocket;
            m_clientItem = clientItem;
            m_name = name;

            m_thread = new Thread(thread_fun);
            m_thread.IsBackground = true;
            m_thread.Start();
        }
        public void thread_fun()
        {
            ++active_thread_count;
           
            m_clientSocket.Connect("127.0.0.1", 56665);
            ++wait_connect_count;
            if (!m_clientItem.Handle.WaitOne(1000))
            {
                ++connect_falied;
                --wait_connect_count;
                --active_thread_count;
                m_clientItem.Active = false;
                return;
            }
            --wait_connect_count;


            Console.WriteLine(m_name + "connected to server");

            for (int i = 0; i < 5; ++i)
            {
                var buffter = Encoding.UTF8.GetBytes("thread message:" + m_name);
                try
                {
                    m_clientSocket.Send(buffter);
                    ++waiting_send_count;
                    Console.WriteLine($"to send data:{m_name}");
                    if (!m_clientItem.Handle.WaitOne(5000)) // wait for data to be send: 500ms
                    {
                        ++send_failed;
                        break;
                    }
                    --waiting_send_count;
                    Console.WriteLine($"send data:{m_name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //m_clientItem.Handle.WaitOne(); // wait for data to be send
                Console.WriteLine(m_name + "data sended.");

                Thread.Sleep(1000);
            }

            Console.WriteLine("client loop ended");
            --active_thread_count;
        }

        public bool finished()
        {
            return !m_thread.IsAlive;
        }
    }
}
