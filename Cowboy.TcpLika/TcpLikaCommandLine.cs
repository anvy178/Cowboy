﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using Cowboy.CommandLines;

namespace Cowboy.TcpLika
{
    public class TcpLikaCommandLine : CommandLine
    {
        private TcpLikaCommandLineOptions _options;

        public TcpLikaCommandLine(string[] args)
          : base(args)
        {
        }

        public override void Execute()
        {
            base.Execute();

            var singleOptions = TcpLikaOptions.GetSingleOptions();
            var getOptions = CommandLineParser.Parse(this.Arguments.ToArray<string>(), singleOptions.ToArray());
            _options = ParseOptions(getOptions);
            ValidateOptions(_options);

            if (_options.IsSetHelp)
            {
                RaiseCommandLineUsage(this, TcpLikaOptions.Usage);
            }
            else if (_options.IsSetVersion)
            {
                RaiseCommandLineUsage(this, this.Version);
            }
            else
            {
                StartEngine();
            }

            Terminate();
        }

        private void StartEngine()
        {
            try
            {
                var engine = new TcpLikaEngine(_options,
                    (string log) => OutputText(string.Format("{0}|{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), log)));
                engine.Start();
            }
            catch (CommandLineException ex)
            {
                RaiseCommandLineException(this, ex);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static TcpLikaCommandLineOptions ParseOptions(CommandLineOptions commandLineOptions)
        {
            if (commandLineOptions == null)
                throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                  "Option used in invalid context -- {0}", "must specify a <host:port>."));

            var options = new TcpLikaCommandLineOptions();

            if (commandLineOptions.Arguments.Any())
            {
                foreach (var arg in commandLineOptions.Arguments.Keys)
                {
                    var optionType = TcpLikaOptions.GetOptionType(arg);
                    if (optionType == TcpLikaOptionType.None)
                        throw new CommandLineException(
                          string.Format(CultureInfo.CurrentCulture, "Option used in invalid context -- {0}",
                          string.Format(CultureInfo.CurrentCulture, "cannot parse the command line argument : [{0}].", arg)));

                    switch (optionType)
                    {
                        case TcpLikaOptionType.Threads:
                            {
                                options.IsSetThreads = true;
                                int threads;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out threads))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of threads option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (threads < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of threads option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.Threads = threads;
                            }
                            break;
                        case TcpLikaOptionType.Nagle:
                            {
                                options.IsSetNagle = true;
                                var nagle = commandLineOptions.Arguments[arg].ToString().ToUpperInvariant();
                                if (nagle != "ON" && nagle != "OFF")
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of nagle option (ON|OFF) -- {0}.", commandLineOptions.Arguments[arg]));
                                options.Nagle = nagle == "ON";
                            }
                            break;
                        case TcpLikaOptionType.ReceiveBufferSize:
                            {
                                options.IsSetReceiveBufferSize = true;
                                int bufferSize;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out bufferSize))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of receive buffer size option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (bufferSize < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of receive buffer size option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.ReceiveBufferSize = bufferSize;
                            }
                            break;
                        case TcpLikaOptionType.SendBufferSize:
                            {
                                options.IsSetSendBufferSize = true;
                                int bufferSize;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out bufferSize))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of send buffer size option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (bufferSize < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of send buffer size option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.SendBufferSize = bufferSize;
                            }
                            break;
                        case TcpLikaOptionType.Connections:
                            {
                                options.IsSetConnections = true;
                                int connections;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out connections))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of connections option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (connections < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of connections option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.Connections = connections;
                            }
                            break;
                        case TcpLikaOptionType.ConnectTimeout:
                            {
                                options.IsSetConnectTimeout = true;
                                int milliseconds;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out milliseconds))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of connect timeout [milliseconds] option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (milliseconds < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of connect timeout [milliseconds] option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.ConnectTimeout = TimeSpan.FromMilliseconds(milliseconds);
                            }
                            break;
                        case TcpLikaOptionType.ConnectionLifetime:
                            {
                                options.IsSetChannelLifetime = true;
                                int milliseconds;
                                if (!int.TryParse(commandLineOptions.Arguments[arg], out milliseconds))
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of channel lifetime [milliseconds] option -- {0}.", commandLineOptions.Arguments[arg]));
                                if (milliseconds < 1)
                                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                        "Invalid formats of channel lifetime [milliseconds] option -- {0}.", commandLineOptions.Arguments[arg]));
                                options.ChannelLifetime = TimeSpan.FromMilliseconds(milliseconds);
                            }
                            break;
                        case TcpLikaOptionType.WebSocket:
                            options.IsSetWebSocket = true;
                            break;
                        case TcpLikaOptionType.Help:
                            options.IsSetHelp = true;
                            break;
                        case TcpLikaOptionType.Version:
                            options.IsSetVersion = true;
                            break;
                    }
                }
            }

            if (commandLineOptions.Parameters.Any())
            {
                try
                {
                    foreach (var item in commandLineOptions.Parameters)
                    {
                        var splits = item.Split(':');
                        if (splits.Length < 2)
                            throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                                "{0} is not well formatted as <host:port>.", item));

                        var host = IPAddress.Parse(splits[0]);
                        var port = int.Parse(splits[1]);
                        var endpoint = new IPEndPoint(host, port);
                        options.RemoteEndPoints.Add(endpoint);
                    }
                }
                catch (Exception ex)
                {
                    throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                        "Invalid formats of endpoints -- {0}", ex.Message), ex);
                }
            }

            return options;
        }

        private static void ValidateOptions(TcpLikaCommandLineOptions options)
        {
            if (options.IsSetHelp || options.IsSetVersion)
                return;

            if (!options.RemoteEndPoints.Any())
            {
                throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
                  "Option used in invalid context -- {0}", "must specify a <host:port>."));
            }
        }
    }
}
