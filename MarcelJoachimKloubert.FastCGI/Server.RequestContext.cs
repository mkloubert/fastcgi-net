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

using MarcelJoachimKloubert.FastCGI.Helpers;
using MarcelJoachimKloubert.FastCGI.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MarcelJoachimKloubert.FastCGI
{
    partial class Server
    {
        internal class RequestContext : FastCGIObject, IRequestContext
        {
            #region Fields (1)

            private IRequestParameters _parameters;

            #endregion Fields (1)

            #region Constructors (1)

            internal RequestContext(RequestHandler handler)
            {
                this.Handler = handler;
            }

            #endregion Constructors (1)

            #region Events (1)

            internal event EventHandler Ended;

            #endregion Events (1)

            #region Properties (6)

            public IPAddress Address
            {
                get;
                internal set;
            }

            internal Stream BodyStream
            {
                get;
                set;
            }

            internal RequestHandler Handler
            {
                get;
                private set;
            }

            public IRequestParameters Parameters
            {
                get { return this._parameters; }

                internal set
                {
                    if (value == null)
                    {
                        return;
                    }

                    var @params = value.Parameters;
                    if (@params == null)
                    {
                        return;
                    }

                    if (this._parameters == null)
                    {
                        this._parameters = value;
                    }
                    else
                    {
                        // merge old with new

                        var curParams = this._parameters.Parameters ?? new Dictionary<string, byte[]>(new CaseInsensitiveComparer());

                        var newParams = new RequestParameters();
                        newParams.Parameters = new Dictionary<string, byte[]>(curParams, new CaseInsensitiveComparer());

                        foreach (var entry in @params)
                        {
                            if (!newParams.Parameters.ContainsKey(entry.Key))
                            {
                                newParams.Parameters.Add(entry);
                            }
                            else
                            {
                                newParams.Parameters[entry.Key] = entry.Value;
                            }
                        }

                        this._parameters = newParams;
                    }
                }
            }

            public int Port
            {
                get;
                internal set;
            }

            internal int? ReadBufferSize
            {
                get;
                set;
            }

            internal int? WriteBufferSize
            {
                get;
                set;
            }

            #endregion Properties (6)

            #region Methods (6)

            public Stream CreateInputStream(ref int? readBufferSize, ref int? writeBufferSize)
            {
                return this.Handler.Server.CreateInputStream(this, ref readBufferSize, ref writeBufferSize);
            }

            public Stream CreateOutputStream(ref int? readBufferSize, ref int? writeBufferSize)
            {
                return this.Handler.Server.CreateOutputStream(this, ref readBufferSize, ref writeBufferSize);
            }

            public byte[] GetBody()
            {
                int? readBufferSize = null;
                using (var stream = this.GetBodyStream(ref readBufferSize))
                {
                    if (readBufferSize < 1)
                    {
                        readBufferSize = 10240;
                    }

                    return BitHelper.ToByteArray(stream, readBufferSize);
                }
            }

            public Stream GetBodyStream(ref int? readBufferSize)
            {
                var stream = this.BodyStream;
                if (stream == null)
                {
                    readBufferSize = null;
                    return new MemoryStream();
                }

                int? writeBufferSize = null;
                var result = this.CreateInputStream(ref readBufferSize, ref writeBufferSize);

                readBufferSize = this.ReadBufferSize;

                var bufferSize = readBufferSize;
                if (bufferSize > writeBufferSize)
                {
                    bufferSize = writeBufferSize;
                }

                long? oldPosition = null;
                try
                {
                    if (stream.CanSeek)
                    {
                        oldPosition = stream.Position;
                        stream.Position = 0;
                    }

                    BitHelper.CopyTo(stream, result, bufferSize);
                }
                finally
                {
                    if (oldPosition.HasValue)
                    {
                        stream.Position = oldPosition.Value;
                    }
                }

                if (result.CanSeek)
                {
                    result.Position = 0;
                }

                return result;
            }

            public bool End()
            {
                try
                {
                    var builder = new EndRequestRecordBuilder();
                    builder.RequestId = this.Handler.RequestId;
                    builder.Status = ProtocolStatus.FCGI_REQUEST_COMPLETE;

                    var recordData = builder.Build();

                    this.Handler.Stream
                                .Write(recordData, 0, recordData.Length);

                    try
                    {
                        using (this.Handler.Stream)
                        {
                            this.Handler.Stream.Close();

                            this.Handler.Server
                                        .RaiseClientDisconnected(this.Handler.Handler.RemoteClient);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Handler.Server.RaiseError(ex);
                    }

                    this.RaiseEventHandler(this.Ended);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Handler.Server.RaiseError(ex);
                    return false;
                }
            }

            public IRequestContext Write(IEnumerable<byte> data)
            {
                using (var temp = new MemoryStream(BitHelper.AsArray(data, true), false))
                {
                    var buffer = new byte[this.WriteBufferSize ?? 10240];

                    int bytesRead;
                    while ((bytesRead = temp.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var builder = new RecordBuilder();
                        builder.Content = buffer.Take(bytesRead).ToArray();
                        builder.RequestId = this.Handler.RequestId;
                        builder.Type = RecordType.FCGI_STDOUT;

                        var recordData = builder.Build();

                        this.Handler.Stream
                                    .Write(recordData, 0, recordData.Length);
                    }
                }

                return this;
            }

            #endregion Methods (6)
        }
    }
}