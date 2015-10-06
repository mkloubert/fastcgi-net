﻿/**********************************************************************************************************************
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MarcelJoachimKloubert.FastCGI
{
    partial class Server
    {
        internal class RequestContext : IRequestContext
        {
            #region Constructors (1)

            internal RequestContext(RequestHandler handler)
            {
                this.Handler = handler;
            }

            #endregion Constructors (1)

            #region Properties (5)

            public IPAddress Address
            {
                get;
                internal set;
            }

            internal RequestHandler Handler
            {
                get;
                private set;
            }

            public IRequestParameters Parameters
            {
                get;
                internal set;
            }

            public int Port
            {
                get;
                internal set;
            }

            #endregion Properties (5)

            #region Methods (2)

            public void End()
            {
                var builder = new RecordBuilder();
                builder.RequestId = this.Handler.Id;
                builder.Type = RecordType.FCGI_END_REQUEST;

                var recordData = builder.Build();

                this.Handler.Stream
                            .Write(recordData, 0, recordData.Length);
            }

            public IRequestContext Write(IEnumerable<byte> data)
            {
                using (var temp = new MemoryStream(BitHelper.AsArray(data, true), false))
                {
                    var buffer = new byte[1024];

                    int bytesRead;
                    while ((bytesRead = temp.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var builder = new RecordBuilder();
                        builder.Content = buffer.Take(bytesRead).ToArray();
                        builder.RequestId = this.Handler.Id;
                        builder.Type = RecordType.FCGI_STDOUT;

                        var recordData = builder.Build();

                        this.Handler.Stream
                                    .Write(recordData, 0, recordData.Length);
                    }
                }

                // this.Handler.HandleNext();

                return this;
            }

            #endregion Methods (2)
        }
    }
}