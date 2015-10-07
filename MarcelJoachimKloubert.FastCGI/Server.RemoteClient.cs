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
using System.Net;
using System.Net.Sockets;

namespace MarcelJoachimKloubert.FastCGI
{
    partial class Server
    {
        /// <summary>
        /// A remote client.
        /// </summary>
        public class RemoteClient : FastCGIObject, IClient
        {
            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="RemoteClient" /> class.
            /// </summary>
            /// <param name="server">The value for the <see cref="RemoteClient.Server" /> property.</param>
            /// <param name="client">The value for the <see cref="RemoteClient.Client" /> property.</param>
            /// <exception cref="ArgumentNullException">
            /// At least one argument is <see langword="null" />.
            /// </exception>
            public RemoteClient(Server server, TcpClient client)
            {
                if (server == null)
                {
                    throw new ArgumentNullException("server");
                }

                if (client == null)
                {
                    throw new ArgumentNullException("client");
                }

                this.Client = client;
                this.Server = server;

                this.Address = (IPEndPoint)client.Client.RemoteEndPoint;
            }

            #endregion Constructors (1)

            #region Properties (3)

            /// <summary>
            /// <see cref="IClient.Address" />
            /// </summary>
            public IPEndPoint Address
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the underlying TCP client.
            /// </summary>
            public TcpClient Client
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the underlying server.
            /// </summary>
            public Server Server
            {
                get;
                private set;
            }

            #endregion Properties (3)
        }
    }
}