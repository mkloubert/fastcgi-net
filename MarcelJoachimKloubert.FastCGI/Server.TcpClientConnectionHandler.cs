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

using MarcelJoachimKloubert.FastCGI.Records;
using System;
using System.Net.Sockets;

namespace MarcelJoachimKloubert.FastCGI
{
    partial class Server
    {
        /// <summary>
        /// Handles the connection with a TCP client.
        /// </summary>
        public class TcpClientConnectionHandler : DisposableBase
        {
            #region Fields (3)

            /// <summary>
            /// Stores the underlying remote client.
            /// </summary>
            public readonly RemoteClient _REMOTE_CLIENT;

            /// <summary>
            /// Stores the underlying server.
            /// </summary>
            protected readonly Server _SERVER;

            /// <summary>
            /// Stores the underlyng network stream.
            /// </summary>
            protected readonly NetworkStream _STREAM;

            #endregion Fields (3)

            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="TcpClientConnectionHandler" /> class.
            /// </summary>
            /// <param name="server">The underlying server.</param>
            /// <param name="client">The underlying client.</param>
            /// <exception cref="ArgumentNullException">
            /// At least one argument is <see langword="null" />.
            /// </exception>
            public TcpClientConnectionHandler(Server server, RemoteClient client)
            {
                if (server == null)
                {
                    throw new ArgumentNullException("server");
                }

                if (client == null)
                {
                    throw new ArgumentNullException("client");
                }

                this._STREAM = new NetworkStream(client.Client.Client, true);

                this._REMOTE_CLIENT = client;
                this._SERVER = server;

                this.CloseConnection = true;
            }

            #endregion Constructors (1)

            #region Properties (6)

            /// <summary>
            /// Gets the underlying TCP client.
            /// </summary>
            public TcpClient Client
            {
                get { return this._REMOTE_CLIENT.Client; }
            }

            /// <summary>
            /// Gets or sets if the connection should be closed (<see langword="true" />)
            /// or not (<see langword="false" />) after a request was handled.
            /// </summary>
            public bool CloseConnection
            {
                get;
                protected set;
            }

            /// <summary>
            /// Gets the underlying remote client.
            /// </summary>
            public RemoteClient RemoteClient
            {
                get { return this._REMOTE_CLIENT; }
            }

            /// <summary>
            /// Get the underlying server.
            /// </summary>
            public Server Server
            {
                get { return this._SERVER; }
            }

            /// <summary>
            /// Gets the underlying network stream.
            /// </summary>
            public NetworkStream Stream
            {
                get { return this._STREAM; }
            }

            #endregion Properties (6)

            #region Methods (5)

            /// <summary>
            /// Begins a request.
            /// </summary>
            /// <param name="record">The record.</param>
            /// <param name="level">The level.</param>
            public void BeginRequest(BeginRequestRecord record, int level = 0)
            {
                this.CloseConnection = record.CloseConnection ?? true;

                var handler = new RequestHandler(this, record, level);
                handler.HandleNext();
            }

            /// <summary>
            /// Handles the next request.
            /// </summary>
            public void HandleNext()
            {
                foreach (var request in UnknownRecord.FromStream(this.Stream))
                {
                    this.InvokeForRequest(request, true);
                }
            }

            /// <summary>
            /// Invokes an action for a request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="throwException">
            /// Rethrow exception (<see langword="true" />) or not (<see langword="false" />).
            /// </param>
            /// <returns>
            /// Operation was successfull (<see langword="true" />) or not <see langword="false" />.
            /// <see langword="null" /> indicates that <paramref name="request" /> is <see langword="null" />.
            /// </returns>
            protected bool? InvokeForRequest(UnknownRecord request, bool throwException = false)
            {
                if (request == null)
                {
                    return null;
                }

                try
                {
                    if (request is BeginRequestRecord)
                    {
                        this.BeginRequest(request as BeginRequestRecord);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, throwException);
                }

                return false;
            }

            /// <summary>
            /// <see cref="DisposableBase.OnDispose(bool, ref bool)" />
            /// </summary>
            protected override void OnDispose(bool disposing, ref bool isDisposed)
            {
                try
                {
                    using (this.Stream)
                    {
                        this.Stream.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, disposing);
                }
            }

            /// <summary>
            /// <see cref="global::MarcelJoachimKloubert.FastCGI.Server.RaiseError(Exception, bool)" />
            /// </summary>
            protected bool? RaiseError(Exception ex, bool rethrow = false)
            {
                return this.Server.RaiseError(ex, rethrow);
            }

            #endregion Methods (5)
        }
    }
}