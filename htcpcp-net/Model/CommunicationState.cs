using System;
using System.Collections.Generic;
using System.Text;

namespace htcpcp_net.Model
{
    internal enum Method
    {
        Brew,
        Get,
        When
    }

    internal enum Stage
    {
        Initial,
        Headers,
        Body,
        Complete
    }

    internal class CommunicationState
    {
        public bool IsValid { get; set; } = true;
        public Uri Uri { get; set; }
        public Method Method { get; set; }
        public Stage Stage { get; set; }
        public Dictionary<string, List<string>> KeyValues { get; private set; } = new Dictionary<string, List<string>>();
        public string Body { get; set; }
    }
}
