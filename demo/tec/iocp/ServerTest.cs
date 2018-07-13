using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    class ServerTest
    {
        public static int Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-c")
            {
                NetManager manager = new NetManager();
                manager.Initialize(false);
            }
            else
            {
                NetManager manager = new NetManager();
                manager.Initialize(true);
            }
            return 0;
        }
    }
}
