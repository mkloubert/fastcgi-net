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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MarcelJoachimKloubert.FastCGI
{
    partial class Server
    {
        /// <summary>
        /// A request handler.
        /// </summary>
        public class RequestHandler : FastCGIObject
        {
            #region Fields (1)

            private readonly RequestContext _CONTEXT;

            #endregion Fields (1)

            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="RequestHandler" /> class.
            /// </summary>
            /// <param name="handler">The underlying client connection handler.</param>
            /// <param name="record">The value for the <see cref="RequestHandler.BaseRecord" /> property.</param>
            /// <exception cref="ArgumentNullException">
            /// At least one parameter is <see langword="null" />.
            /// </exception>
            public RequestHandler(TcpClientConnectionHandler handler, BeginRequestRecord record)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException("handler");
                }

                if (record == null)
                {
                    throw new ArgumentNullException("record");
                }

                this.Handler = handler;
                this.BaseRecord = record;

                this._CONTEXT = new RequestContext(this);
                this._CONTEXT.Address = this.Server._SETTINGS.LocalAddress ?? IPAddress.Loopback;
                this._CONTEXT.Port = this.Server._SETTINGS.Port;

                int? readBufferSize = null;
                int? writeBufferSize = null;
                this._CONTEXT.BodyStream = this._CONTEXT.CreateInputStream(ref readBufferSize, ref writeBufferSize);

                this._CONTEXT.ReadBufferSize = readBufferSize;
                if (this._CONTEXT.ReadBufferSize < 1)
                {
                    this._CONTEXT.ReadBufferSize = 10240;
                }

                this._CONTEXT.WriteBufferSize = writeBufferSize;
                if (this._CONTEXT.WriteBufferSize < 1)
                {
                    this._CONTEXT.WriteBufferSize = 10240;
                }

                this._CONTEXT.Ended += this._CONTEXT_Ended;
            }

            #endregion Constructors (1)

            #region Properties (6)

            /// <summary>
            /// Gets the base record.
            /// </summary>
            public BeginRequestRecord BaseRecord
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the underlying client handler.
            /// </summary>
            public TcpClientConnectionHandler Handler
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets if the request has been ended (<see langword="true" />) or not (<see langword="false" />).
            /// </summary>
            public bool HasEnded
            {
                get;
                protected set;
            }

            /// <summary>
            /// Gets the ID of the request.
            /// </summary>
            public ushort RequestId
            {
                get { return this.BaseRecord.RequestId; }
            }

            /// <summary>
            /// Gets the underlying server.
            /// </summary>
            public Server Server
            {
                get { return this.Handler.Server; }
            }

            /// <summary>
            /// Gets the underlying stream.
            /// </summary>
            public NetworkStream Stream
            {
                get { return this.Handler.Stream; }
            }

            #endregion Properties (6)

            #region Methods (7)

            private void _CONTEXT_Ended(object sender, EventArgs e)
            {
                this.HasEnded = true;

                if (this.Handler.CloseConnection)
                {
                    try
                    {
                        try
                        {
                            this.Handler.Dispose();
                        }
                        finally
                        {
                            if (!this.Handler.RemoteClient.Client.Connected)
                            {
                                this.Server
                                    .RaiseClientDisconnected(this.Handler.RemoteClient);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Server.RaiseError(ex);
                    }
                }
                else
                {
                    this.HandleNext();
                }
            }

            /// <summary>
            /// Ends the request.
            /// </summary>
            /// <returns>Operation was successfull (<see langword="true" />) or not (<see langword="false" />).</returns>
            public bool End()
            {
                if (this.HasEnded)
                {
                    return true;
                }

                return this.HasEnded = this._CONTEXT.End();
            }

            /// <summary>
            /// handle next steps.
            /// </summary>
            public void HandleNext()
            {
                foreach (var request in UnknownRecord.FromStream(this.Stream))
                {
                    this.InvokeForRequest(request);
                }
            }

            /// <summary>
            /// Handles input data from client.
            /// </summary>
            /// <param name="record">The underlying record.</param>
            protected void HandleInputData(InputRecord record)
            {
                var maxBodyLen = this.Server._SETTINGS.MaxBodyLength;
                if (maxBodyLen < 0)
                {
                    maxBodyLen = 0;
                }

                var dataToWriteCount = record.Data.Length;
                if (maxBodyLen.HasValue)
                {
                    if ((this._CONTEXT.BodyStream.Length + dataToWriteCount) > maxBodyLen)
                    {
                        // truncate
                        dataToWriteCount = (int)(maxBodyLen.Value - this._CONTEXT.BodyStream.Length);
                    }
                }

                if (dataToWriteCount > 0)
                {
                    var dataToWrite = record.Data;
                    if (dataToWrite.Length > dataToWriteCount)
                    {
                        dataToWrite = dataToWrite.Take(dataToWriteCount).ToArray();
                    }

                    using (var temp = new MemoryStream(dataToWrite, false))
                    {
                        if (!this._CONTEXT.WriteBufferSize.HasValue)
                        {
                            temp.CopyTo(this._CONTEXT.BodyStream);
                        }
                        else
                        {
                            temp.CopyTo(this._CONTEXT.BodyStream, this._CONTEXT.WriteBufferSize.Value);
                        }
                    }
                }
                else
                {
                    var handler = this.Server._SETTINGS.Handler;
                    if (handler != null)
                    {
                        handler.HandleRequest(this._CONTEXT);
                    }
                    else
                    {
                        this._CONTEXT.End();
                    }

                    if (!this.HasEnded)
                    {
                        this.HandleNext();
                    }
                }
            }

            /// <summary>
            /// Handles parameters.
            /// </summary>
            /// <param name="record">The underlying record.</param>
            protected void HandleParameters(ParameterRecord record)
            {
                try
                {
                    this._CONTEXT.Parameters = record.Parameters;

                    this.HandleNext();
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex);
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
                        this.Handler.BeginRequest(request as BeginRequestRecord);
                        return true;
                    }

                    if (request.RequestId == this.BaseRecord.RequestId)
                    {
                        if (this.HasEnded)
                        {
                            return false;
                        }

                        if (request is ParameterRecord)
                        {
                            this.HandleParameters(request as ParameterRecord);
                            return true;
                        }

                        if (request is InputRecord)
                        {
                            this.HandleInputData(request as InputRecord);
                            return true;
                        }
                    }
                    else
                    {
                        this.End();
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, throwException);
                }

                return false;
            }

            /// <summary>
            /// <see cref="global::MarcelJoachimKloubert.FastCGI.Server.RaiseError(Exception, bool)" />
            /// </summary>
            protected bool? RaiseError(Exception ex, bool rethrow = false)
            {
                return this.Server.RaiseError(ex, rethrow);
            }

            #endregion Methods (7)
        }
    }
}