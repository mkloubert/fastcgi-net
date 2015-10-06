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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MarcelJoachimKloubert.FastCGI.Http
{
    partial class HttpRequestHandler
    {
        /// <summary>
        /// A HTTP request context.
        /// </summary>
        protected class HttpRequest : IHttpRequest
        {
            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="HttpRequest" /> class.
            /// </summary>
            /// <param name="context">The value for the <see cref="HttpRequest.Context" /> property.</param>
            /// <param name="invokeOnInit">
            /// Invoke <see cref="HttpRequest.OnInit()" /> method (<see langword="true" />) or not (<see langword="false" />).
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="context" /> is <see langword="null" />.
            /// </exception>
            public HttpRequest(IRequestContext context, bool invokeOnInit = true)
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

            #region Properties (4)

            /// <summary>
            /// <see cref="IHttpRequest.Context" />
            /// </summary>
            public IRequestContext Context
            {
                get;
                private set;
            }

            /// <summary>
            /// <see cref="IHttpRequest.Headers" />
            /// </summary>
            public IDictionary<string, string> Headers { get; protected set; }

            /// <summary>
            /// <see cref="IHttpRequest.Query" />
            /// </summary>
            public IDictionary<string, string> Query { get; protected set; }

            /// <summary>
            /// <see cref="IHttpRequest.Uri" />
            /// </summary>
            public Uri Uri { get; protected set; }

            #endregion Properties (4)

            #region Methods (1)

            /// <summary>
            /// Initializes that class.
            /// </summary>
            protected virtual void OnInit()
            {
                var headers = new Dictionary<string, string>(new CaseInsensitiveComparer());
                var query = new Dictionary<string, string>(new CaseInsensitiveComparer());

                if (this.Context.Parameters != null)
                {
                    var @params = this.Context.Parameters.Parameters;
                    if (@params != null)
                    {
                        foreach (var entry in @params)
                        {
                            var key = (entry.Key ?? string.Empty).ToUpper().Trim();

                            if (key.StartsWith("HTTP_"))
                            {
                                var headerName = key.Substring(5).ToLower().Trim();
                                headerName = headerName.Replace('_', '-')
                                                       .Replace("\t", "    ")
                                                       .Replace(' ', '-');

                                var headerValue = Encoding.ASCII.GetString(entry.Value ?? new byte[0]);

                                if (!headers.ContainsKey(headerName))
                                {
                                    headers.Add(headerName, headerValue);
                                }
                                else
                                {
                                    headers[headerName] = headerValue;
                                }
                            }
                            else if (key == "QUERY_STRING")
                            {
                                var queryString = Encoding.UTF8.GetString(entry.Value ?? new byte[0]).TrimStart();
                                if (!string.IsNullOrWhiteSpace(queryString))
                                {
                                    var parts = queryString.Split('&');
                                    foreach (var p in parts)
                                    {
                                        if (string.IsNullOrWhiteSpace(p))
                                        {
                                            continue;
                                        }

                                        string queryKey = null;
                                        string queryValue = null;

                                        var queryKeyAndValue = p.Split('=');

                                        if (queryKeyAndValue.Length > 0)
                                        {
                                            queryKey = (queryKeyAndValue[0] ?? string.Empty).ToLower().Trim();
                                        }

                                        if (queryKeyAndValue.Length > 1)
                                        {
                                            queryValue = Uri.UnescapeDataString(string.Join("=",
                                                                                            queryKeyAndValue.Skip(1)));
                                        }

                                        if (!query.ContainsKey(queryKey))
                                        {
                                            query.Add(queryKey, queryValue);
                                        }
                                        else
                                        {
                                            query[queryKey] = queryValue;
                                        }
                                    }
                                }
                            }
                        }

                        // uri
                        {
                            byte[] temp;

                            string scheme = null;
                            if (@params.TryGetValue("REQUEST_SCHEME", out temp))
                            {
                                scheme = (Encoding.UTF8.GetString(temp ?? new byte[0]) ?? string.Empty).ToLower().Trim();
                            }

                            if (string.IsNullOrEmpty(scheme))
                            {
                                scheme = "http";
                            }

                            string host = null;
                            if (headers.TryGetValue("Host", out host))
                            {
                                host = (host ?? string.Empty).ToLower().Trim();
                            }

                            if (string.IsNullOrEmpty(host))
                            {
                                var ip = this.Context.Address ?? IPAddress.Loopback;
                                var port = this.Context.Port;

                                host = string.Format("{0}:{1}", ip, port);
                            }

                            string requestUri = null;
                            if (@params.TryGetValue("REQUEST_URI", out temp))
                            {
                                requestUri = (Encoding.UTF8.GetString(temp ?? new byte[0]) ?? string.Empty).TrimStart();
                            }

                            if (string.IsNullOrWhiteSpace(requestUri))
                            {
                                requestUri = "/";
                            }

                            while (requestUri.StartsWith("//"))
                            {
                                requestUri = requestUri.Substring(1);
                            }

                            while (requestUri.EndsWith("//"))
                            {
                                requestUri = requestUri.Substring(0, requestUri.Length - 1);
                            }

                            Uri newUri;
                            if (Uri.TryCreate(string.Format("{0}://{1}{2}",
                                                            scheme, host, requestUri), UriKind.Absolute, out newUri))
                            {
                                this.Uri = newUri;
                            }
                        }
                    }
                }

                this.Headers = headers;
                this.Query = query;
            }

            #endregion Methods (1)
        }
    }
}