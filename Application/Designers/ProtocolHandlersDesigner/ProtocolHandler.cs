using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace ProtocolHandlersDesigner
{
    public partial class ProtocolHandler : Component
    {
        IsWiXProtocolHandler _protocolHandler;

        public ProtocolHandler()
        {
            InitializeComponent();
        }

        public ProtocolHandler(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public void Read(IsWiXProtocolHandler protocolHandler)
        {
        }

    }
}
