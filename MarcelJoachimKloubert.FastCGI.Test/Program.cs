/**********************************************************************************************************************
 * fastcgi-net (https://github.com/mkloubert/fastcgi-net)                                                             *
 *                                                                                                                    *
 * Copyright (c) 2015, Marcel Joachim Kloubert <marcel.kloubert@gmx.net>                                              *
 * All rights reserved.                                                                                               *
 *                                                                                                                    *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the   *
 * following conditions are met:                                                                                      *
 *                                                                                                                    *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the          *
 *    following disclaimer.                                                                                           *
 *                                                                                                                    *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the       *
 *    following disclaimer in the documentation and/or other materials provided with the distribution.                *
 *                                                                                                                    *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote    *
 *    products derived from this software without specific prior written permission.                                  *
 *                                                                                                                    *
 *                                                                                                                    *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, *
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  *
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, *
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR    *
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  *
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE   *
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                                           *
 *                                                                                                                    *
 **********************************************************************************************************************/

using System;
using FastCGIHttpRequestHandler = MarcelJoachimKloubert.FastCGI.Http.HttpRequestHandler;
using FastCGIServer = MarcelJoachimKloubert.FastCGI.Server;
using FastCGISettings = MarcelJoachimKloubert.FastCGI.Settings;

namespace MarcelJoachimKloubert.FastCGI.Test
{
    internal static class Program
    {
        private static void InvokeForConsoleColor(Action action, ConsoleColor? foreColor = null, ConsoleColor? bgColor = null)
        {
            var oldBGColor = Console.BackgroundColor; 
            var oldFGColor = Console.ForegroundColor;

            try
            {
                if (foreColor.HasValue)
                {
                    Console.ForegroundColor = foreColor.Value;
                }

                if (bgColor.HasValue)
                {
                    Console.BackgroundColor = bgColor.Value;
                }

                action();
            }
            finally
            {
                Console.BackgroundColor = oldBGColor;
                Console.ForegroundColor = oldFGColor;
            }
        }

        private static void Main(string[] args)
        {
            try
            {
                var handler = new FastCGIHttpRequestHandler();
                handler.Request += (sender, e) =>
                    {
                        e.Response.NotFound = false;

                        var obj = new
                        {
                            hello = "World!",
                        };

                        e.Response.SetupForJson();

                        e.Response.WriteJson(obj);
                    };

                var settings = new FastCGISettings()
                    {
                        Handler = handler,
                        Port = 9002,
                    };

                using (var server = new FastCGIServer(settings))
                {
                    server.Connected += (sender, e) =>
                        {
                            InvokeForConsoleColor(() =>
                                {
                                    Console.WriteLine("[New connection] Connected with '{0}'.", e.Client.Address);
                                }, ConsoleColor.White, ConsoleColor.Black);
                        };
                    server.Disconnected += (sender, e) =>
                        {
                            InvokeForConsoleColor(() =>
                                {
                                    Console.WriteLine("[Connection closed] Connection with '{0}' has been closed.", e.Client.Address);
                                });
                        };
                    server.Disposing += (sender, e) =>
                        {
                            Console.WriteLine("Disposing...");
                        };
                    server.Disposed += (sender, e) =>
                        {
                            Console.WriteLine("Disposed.");
                        };
                    server.Error += (sender, e) =>
                        {
                            InvokeForConsoleColor(() =>
                                {
                                    Console.WriteLine("[ERROR!] {0}", e.Error);
                                }, ConsoleColor.Red, ConsoleColor.Black);
                        };
                    server.Starting += (sender, e) =>
                        {
                            Console.WriteLine("Starting...");
                        };
                    server.Started += (sender, e) =>
                        {
                            Console.WriteLine("Started.");
                        };
                    server.Stopping += (sender, e) =>
                        {
                            Console.WriteLine("Stopping...");
                        };
                    server.Stopped += (sender, e) =>
                        {
                            Console.WriteLine("Stopped.");
                        };

                    server.Start();

                    Console.WriteLine();
                    Console.WriteLine("Close with ENTER ...");
                    Console.WriteLine();
                    Console.WriteLine();

                    Console.ReadLine();

                    server.Stop();
                }
            }
            catch (Exception ex)
            {
                InvokeForConsoleColor(() =>
                    {
                        Console.WriteLine("[FATAL ERROR!!!] {0}", ex);
                    }, ConsoleColor.Yellow, ConsoleColor.Red);
            }

#if DEBUG
            global::System.Console.WriteLine();
            global::System.Console.WriteLine();
            global::System.Console.WriteLine("===== ENTER =====");
            global::System.Console.ReadLine();
#endif
        }
    }
}