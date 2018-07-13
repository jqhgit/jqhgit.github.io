using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    public class NetCommonIO : INetIO
    {
        private INetSelector m_net_selector = null;

        public SelectorType NetSelectorType()
        {
            if (m_net_selector == null)
                return SelectorType.Unknown;
            else
                return m_net_selector.Type();
        }
        public INetSelector Selector()
        {
            return m_net_selector;
        }

        public void Initialize(SelectorType type, int timeout_ms = 500)
        {
            switch (type)
            {
                case SelectorType.IOCP:
                    m_net_selector = new SelectorIocp();
                    m_net_selector.Initlalize(timeout_ms);
                    break;
                default:
                    break;
            }
        }

        public void Start()
        {
            if (null != m_net_selector)
            {
                m_net_selector.Start();
            }
        }

        public void Stop()
        {
            if (null != m_net_selector)
            {
                m_net_selector.Stop();
            }
        }
    }
}
