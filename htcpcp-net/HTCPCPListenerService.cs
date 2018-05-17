﻿using htcpcp_net.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace htcpcp_net
{
    public class HTCPCPListenerService
    {
        private const string BAD_REQUEST_RESPONSE = "HTCPCP/1.0 400 BAD REQUEST";
        private const string OK_RESPONSE = "HTCPCP/1.0 200 OK";
        private const string NOT_ACCEPTABLE_RESPONSE = "HTCPCP/1.0 406 NOT ACCEPTABLE";

        private readonly string _host;
        private readonly int _port;

        public event EventHandler<BrewingRequestEventArgs> BrewingRequestRecieved = delegate { };

        public HTCPCPListenerService(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Listen()
        {
            Listen(CancellationToken.None);
        }

        public void Listen(CancellationToken token)
        {
            var ipAddres = IPAddress.Parse(_host);
            var listener = new TcpListener(ipAddres, _port);

            listener.Start();

            while (true)
            {
                while (!listener.Pending() && !token.IsCancellationRequested)
                    Thread.Sleep(10);

                if (token.IsCancellationRequested)
                {
                    break;
                }

                var client = listener.AcceptTcpClient();
                Task.Run(() => ProcessRequest(client));
            }

            listener.Stop();
        }

        private void ProcessRequest(TcpClient client)
        {
            var state = new CommunicationState();
            var stream = client.GetStream();

            using (var writer = new StreamWriter(stream))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (client.Connected)
                    {
                        var line = reader.ReadLine()?.Trim().ToLower() ?? "";
                        var output = ProcessCommand(line, state);

                        Console.WriteLine($"Input: {line}");

                        if (!string.IsNullOrEmpty(output))
                        {
                            Console.WriteLine($"Output: {output}");
                            writer.WriteLine(output);
                            writer.Flush();
                        }

                        Console.WriteLine($"Stage: {state.Stage}");

                        if (!state.IsValid || state.Stage == Stage.Complete)
                        {
                            if(!state.IsValid)
                                Console.WriteLine($"State is invalid.. closing");

                            client.Close();
                        }

                    }
                }
            }

            if (state.Stage == Stage.Complete)
            {
                BrewingRequestRecieved(this, new BrewingRequestEventArgs(state));
            }
            else
            {
                Console.WriteLine("Connection closed before complete request");
            }
        }

        private string ProcessCommand(string line, CommunicationState state)
        {
            switch (state.Stage)
            {
                case Stage.Initial:
                    if (line.StartsWith("post ") || line.StartsWith("brew "))
                        state.Method = Method.Brew;
                    else if (line.StartsWith("when "))
                        state.Method = Method.When;
                    else if (line.StartsWith("get "))
                        state.Method = Method.Get;
                    else
                    {
                        state.IsValid = false;
                        return BAD_REQUEST_RESPONSE;
                    }

                    var parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (!Uri.TryCreate(parts[1], UriKind.Absolute, out Uri uri))
                    {
                        state.IsValid = false;
                        return BAD_REQUEST_RESPONSE;
                    }

                    state.Uri = uri;
                    state.Stage = Stage.Headers;
                    break;
                case Stage.Headers:
                    if (string.IsNullOrEmpty(line))
                    {
                        state.Stage = Stage.Body;
                    }
                    else
                    {
                        var header = line.Split(new char[] { ':' }, 2)
                            .Select(x => x.Trim())
                            .ToArray();

                        if(header.Length != 2)
                        {
                            state.IsValid = false;
                            return BAD_REQUEST_RESPONSE;
                        }

                        var headerValues = header[1]
                            .Split(',')
                            .Select(s => s.Trim())
                            .ToList();

                        if (state.KeyValues.ContainsKey(header[0]))
                        {
                            state.KeyValues[header[0]].AddRange(headerValues);
                        }
                        else
                        {
                            state.KeyValues[header[0]] = headerValues;
                        }
                    }
                    break;
                case Stage.Body:
                    if (string.IsNullOrEmpty(line))
                    {
                        state.Stage = Stage.Complete;
                    }

                    break;
                case Stage.Complete:
                    break;
            }

            return string.Empty;
        }
    }
}