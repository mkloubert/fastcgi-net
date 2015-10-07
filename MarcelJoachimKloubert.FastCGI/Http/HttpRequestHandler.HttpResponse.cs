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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarcelJoachimKloubert.FastCGI.Http
{
    partial class HttpRequestHandler
    {
        /// <summary>
        /// A HTTP response context.
        /// </summary>
        protected class HttpResponse : FastCGIObject, IHttpResponse
        {
            #region Fields (1)

            /// <summary>
            /// The header name for the content type.
            /// </summary>
            public const string HEADER_CONTENT_TYPE = "Content-type";

            #endregion Fields (1)

            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="HttpResponse" /> class.
            /// </summary>
            /// <param name="context">The value for the <see cref="HttpResponse.Context" /> property.</param>
            /// <param name="invokeOnInit">
            /// Invoke <see cref="HttpResponse.OnInit()" /> method (<see langword="true" />) or not (<see langword="false" />).
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="context" /> is <see langword="null" />.
            /// </exception>
            public HttpResponse(IRequestContext context, bool invokeOnInit = true)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                this.Context = context;

                if (invokeOnInit)
                {
                    this.OnInit();
                }
            }

            #endregion Constructors (1)

            #region Properties (13)

            /// <summary>
            /// Gets or sets the content type.
            /// </summary>
            public int? Code { get; set; }

            /// <summary>
            /// Gets or sets the content type.
            /// </summary>
            public long? ContentLength { get; set; }

            /// <summary>
            /// Gets or sets the content type.
            /// </summary>
            public string ContentType
            {
                get
                {
                    string result;
                    this.Headers.TryGetValue(HEADER_CONTENT_TYPE, out result);

                    return result;
                }

                set
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        CollectionHelper.AddOrSet(this.Headers, HEADER_CONTENT_TYPE, value.ToLower().Trim());
                    }
                    else
                    {
                        this.Headers.Remove(HEADER_CONTENT_TYPE);
                    }
                }
            }

            /// <summary>
            /// <see cref="IHttpResponse.Context" />
            /// </summary>
            public IRequestContext Context
            {
                get;
                private set;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Encoding" />
            /// </summary>
            public Encoding Encoding { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Headers" />
            /// </summary>
            public IDictionary<string, string> Headers { get; protected set; }

            /// <summary>
            /// <see cref="IHttpResponse.NotFound" />
            /// </summary>
            public bool NotFound { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.NotImplemented" />
            /// </summary>
            public bool NotImplemented { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.ReadBufferSize" />
            /// </summary>
            public int? ReadBufferSize { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Status" />
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Stream" />
            /// </summary>
            public Stream Stream { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Version" />
            /// </summary>
            public Version Version { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.WriteBufferSize" />
            /// </summary>
            public int? WriteBufferSize { get; set; }

            #endregion Properties (13)

            #region Methods (12)

            /// <summary>
            /// Returns the encoder to use for string operations.
            /// </summary>
            /// <returns>A non <see langword="null" /> encoder.</returns>
            protected virtual Encoding GetEncoder()
            {
                return this.Encoding ?? Encoding.UTF8;
            }

            /// <summary>
            /// Initializes that class.
            /// </summary>
            protected virtual void OnInit()
            {
                this.Headers = new Dictionary<string, string>(new CaseInsensitiveComparer());
            }

            /// <summary>
            /// <see cref="IHttpResponse.SetupForHtml()" />
            /// </summary>
            public IHttpResponse SetupForHtml()
            {
                this.ContentType = "text/html";
                this.Encoding = Encoding.UTF8;

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.SetupForJson()" />
            /// </summary>
            public IHttpResponse SetupForJson()
            {
                this.ContentType = "application/json";
                this.Encoding = Encoding.UTF8;

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.SetupForXml()" />
            /// </summary>
            public IHttpResponse SetupForXml()
            {
                this.ContentType = "text/xml";
                this.Encoding = Encoding.UTF8;

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Write(IEnumerable{char})" />
            /// </summary>
            public IHttpResponse Write(IEnumerable<char> chars)
            {
                var str = StringHelper.AsString(chars);
                if (!string.IsNullOrEmpty(str))
                {
                    this.Write(data: this.GetEncoder().GetBytes(str));
                }

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Write(IEnumerable{byte})" />
            /// </summary>
            public IHttpResponse Write(IEnumerable<byte> data)
            {
                var blob = BitHelper.AsArray(data);
                if (blob != null)
                {
                    using (var temp = new MemoryStream(blob, false))
                    {
                        this.Write(stream: temp);
                    }
                }

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Write(Stream, int?)" />
            /// </summary>
            public IHttpResponse Write(Stream stream, int? bufferSize = null)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                if (bufferSize < 1)
                {
                    throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Less than 1!");
                }

                bufferSize = bufferSize ?? this.WriteBufferSize;

                if (!bufferSize.HasValue)
                {
                    stream.CopyTo(this.Stream);
                }
                else
                {
                    stream.CopyTo(this.Stream, bufferSize.Value);
                }

                return this;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Write(object)" />
            /// </summary>
            public IHttpResponse Write(object obj)
            {
                return this.Write(chars: StringHelper.AsString(obj));
            }

            /// <summary>
            /// <see cref="IHttpResponse.WriteFormat(IEnumerable{char}, IEnumerable{object})" />
            /// </summary>
            public IHttpResponse WriteFormat(IEnumerable<char> format, IEnumerable<object> argList)
            {
                return this.WriteFormat(format: format,
                                        args: CollectionHelper.AsArray(argList));
            }

            /// <summary>
            /// <see cref="IHttpResponse.WriteFormat(IEnumerable{char}, object[])" />
            /// </summary>
            public IHttpResponse WriteFormat(IEnumerable<char> format, params object[] args)
            {
                return this.Write(string.Format(StringHelper.AsString(format, true),
                                                args ?? new object[] { null }));
            }

            /// <summary>
            /// <see cref="IHttpResponse.WriteJson(object)" />
            /// </summary>
            public IHttpResponse WriteJson(object obj)
            {
                var serializer = new JsonSerializer();

                using (var temp = new MemoryStream())
                {
                    using (var textWriter = new StreamWriter(temp, this.GetEncoder()))
                    {
                        using (var jsonWriter = new JsonTextWriter(textWriter))
                        {
                            serializer.Serialize(jsonWriter, obj);

                            textWriter.Flush();
                            temp.Position = 0;

                            return this.Write(stream: temp);
                        }
                    }
                }
            }

            #endregion Methods (12)
        }
    }
}