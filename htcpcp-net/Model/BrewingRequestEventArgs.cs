using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace htcpcp_net.Model
{
    public enum AdditionType
    {
        Unknown,
        Milk,
        Syrup,
        Alcohol,
        Sweetener,
        Spice
    }

    public class Addition
    {
        public AdditionType Type { get; set; }
        public string Name { get; set; }
    }

    public enum RequestType
    {
        Start,
        Stop
    }

    public class BrewingRequestEventArgs : EventArgs
    {
        public const string ACCEPT_ADDITIONS_HEADER_NAME = "accept-additions";

        public string Scheme { get; private set; }
        public int PotNumber { get; private set; }
        public ICollection<Addition> Additions { get; private set; }
        public RequestType RequestType { get; private set; }

        internal BrewingRequestEventArgs(CommunicationState state)
        {
            Scheme = state.Uri.Scheme;
            PotNumber = Convert.ToInt32(state.Uri.Segments[1]);
            RequestType = (RequestType)Enum.Parse(typeof(RequestType), state.Body, true);
            
            ParseAdditions(state);
        }

        private void ParseAdditions(CommunicationState state)
        {
            var valuesFromHeader = state.KeyValues.ContainsKey(ACCEPT_ADDITIONS_HEADER_NAME)
                ? state.KeyValues[ACCEPT_ADDITIONS_HEADER_NAME]
                : new List<string>();

            var valuesFromUrl = state.Uri.Query.Length > 1
                ? state.Uri.Query.Substring(1).Split(',')
                    .Select(x => x.Trim())
                    .ToList()

                : new List<string>();

            var additions = new List<Addition>();

            foreach(var value in Enumerable.Concat(valuesFromHeader, valuesFromUrl))
            {
                var parts = value.Split(';')
                   .Select(s => s.Trim())
                   .ToArray();

                if (parts.Length != 2)
                    continue;

                int dashPosition = parts[0].IndexOf('-');
                if (dashPosition == -1)
                    continue;

                Enum.TryParse(parts[0].Substring(0, dashPosition), true, out AdditionType type);

                additions.Add(new Addition
                {
                    Type = type,
                    Name = parts[1]
                });
            }

            Additions = additions;
        }
    }
}
