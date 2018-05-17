using System;
using System.Collections.Generic;
using System.Text;

namespace htcpcp_net.Model
{
    public enum AdditionType
    {
        Milk,
        Syrup,
        Alcohol
    }

    public class Addition
    {
        public AdditionType Type { get; set; }
        public string Name { get; set; }
    }

    public class BrewingRequestEventArgs : EventArgs
    {
        public string Scheme { get; set; }
        public int PotNumber { get; set; }
        public ICollection<Addition> Additions { get; set; }

        internal BrewingRequestEventArgs(CommunicationState state)
        {

        }
    }
}
