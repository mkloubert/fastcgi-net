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

using MarcelJoachimKloubert.FastCGI.Collections;
using MarcelJoachimKloubert.FastCGI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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
        protected class HttpRequest : FastCGIObject, IHttpRequest
        {
            #region Fields (1)

            private Lazy<IDictionary<string, string>> _postVars;

            #endregion Fields (1)

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

            #region Properties (9)

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
            /// <see cref="IHttpRequest.IsMethodAllowed" />
            /// </summary>
            public bool IsMethodAllowed
            {
                get
                {
                    return this.SupportedMethods
                               .Select(x => (x ?? string.Empty).ToUpper().Trim())
                               .Contains((this.Method ?? string.Empty).ToUpper().Trim());
                }
            }

            /// <summary>
            /// <see cref="IHttpRequest.KnownMethod" />
            /// </summary>
            public HttpMethod? KnownMethod
            {
                get
                {
                    HttpMethod result;
                    if (Enum.TryParse<HttpMethod>(this.Method, true, out result))
                    {
                        return result;
                    }

                    return null;
                }
            }

            /// <summary>
            /// <see cref="IHttpRequest.Headers" />
            /// </summary>
            public string Method { get; protected set; }

            /// <summary>
            /// <see cref="IHttpRequest.PostVars" />
            /// </summary>
            public IDictionary<string, string> PostVars
            {
                get { return this._postVars.Value; }
            }

            /// <summary>
            /// <see cref="IHttpRequest.QueryVars" />
            /// </summary>
            public IDictionary<string, string> QueryVars { get; protected set; }

            /// <summary>
            /// <see cref="IHttpRequest.QueryVars" />
            /// </summary>
            public IList<string> SupportedMethods { get; protected set; }

            /// <summary>
            /// <see cref="IHttpRequest.Uri" />
            /// </summary>
            public Uri Uri { get; protected set; }

            #endregion Properties (9)

            #region Methods (4)

            /// <summary>
            /// Extracts variables from a string and writes it to a dictionary.
            /// </summary>
            /// <param name="chars">The string / chars with the variables.</param>
            /// <param name="target">The target dictionary.</param>
            protected void GetVariables(IEnumerable<char> chars, IDictionary<string, string> target)
            {
                using (var reader = new StringReader(StringHelper.AsString(chars, true)))
                {
                    this.GetVariables(reader, target);
                }
            }

            /// <summary>
            /// Extracts variables from a <see cref="TextReader" /> and writes it to a dictionary.
            /// </summary>
            /// <param name="reader">The reader with the variables.</param>
            /// <param name="target">The target dictionary.</param>
            protected void GetVariables(TextReader reader, IDictionary<string, string> target)
            {
                if (reader == null)
                {
                    return;
                }

                const int STATE_WHITESPACE = 0;
                const int STATE_NAME = 1;
                const int STATE_VALUE = 2;

                var state = STATE_WHITESPACE;

                string currentVarName = null;
                string currentVarValue = null;

                Action reset = () =>
                    {
                        currentVarName = null;
                        currentVarValue = null;
                    };

                Action appendNext = () =>
                    {
                        if (!string.IsNullOrWhiteSpace(currentVarName))
                        {
                            currentVarName = currentVarName.Trim();

                            if (!string.IsNullOrEmpty(currentVarValue))
                            {
                                currentVarValue = global::System.Uri.UnescapeDataString(currentVarValue);
                            }
                            else
                            {
                                currentVarValue = null;
                            }

                            if (target != null)
                            {
                                CollectionHelper.AddOrSet(target,
                                                          currentVarName, currentVarValue);
                            }
                        }

                        reset();
                    };

                Action<char> handleChar = (c) =>
                    {
                        switch (state)
                        {
                            case STATE_NAME:
                                if (c == '=')
                                {
                                    // next is the value
                                    state = STATE_VALUE;
                                }
                                else if (c == '&')
                                {
                                    // start with new variable

                                    appendNext();
                                    state = STATE_NAME;
                                }
                                else
                                {
                                    currentVarName += c;
                                }
                                break;

                            case STATE_VALUE:
                                if (c == '&')
                                {
                                    // start with new variable

                                    appendNext();
                                    state = STATE_NAME;
                                }
                                else
                                {
                                    currentVarValue += c;
                                }
                                break;
                        }
                    };

                var buffer = new char[1];
                int charsRead;
                while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (var i = 0; i < charsRead; i++)
                    {
                        var c = buffer[i];

                        switch (state)
                        {
                            case STATE_WHITESPACE:
                                if (char.IsWhiteSpace(c) ||
                                    (c == '?') ||
                                    (c == '&'))
                                {
                                    // leading "whitespace" character
                                    continue;
                                }
                                else
                                {
                                    // begin

                                    reset();
                                    state = STATE_NAME;

                                    handleChar(c);
                                }
                                break;

                            default:
                                handleChar(c);
                                break;
                        }
                    }
                }

                appendNext();
            }

            /// <summary>
            /// Initializes that class.
            /// </summary>
            protected virtual void OnInit()
            {
                var headers = new Dictionary<string, string>(new CaseInsensitiveComparer());
                var queryVars = new Dictionary<string, string>(new CaseInsensitiveComparer());
                string method = null;

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
                                    this.GetVariables(queryString, queryVars);
                                }
                            }
                            else if (key == "REQUEST_METHOD")
                            {
                                method = Encoding.ASCII.GetString(entry.Value ?? new byte[0]);
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

                method = (method ?? string.Empty).ToUpper().Trim();
                if (method == "")
                {
                    method = "GET";
                }

                this._postVars = new Lazy<IDictionary<string, string>>(() =>
                    {
                        var postVars = new Dictionary<string, string>(new CaseInsensitiveComparer());

                        if ("POST" == method)
                        {
                            try
                            {
                                int? readBuffferSize = null;
                                using (var reader = new StreamReader(this.Context.GetBodyStream(ref readBuffferSize), Encoding.UTF8))
                                {
                                    this.GetVariables(reader, postVars);
                                }
                            }
                            catch
                            {
                                // ignore errors
                            }
                        }

                        return new ReadOnlyDictionary<string, string>(postVars);
                    });

                this.Headers = new ReadOnlyDictionary<string, string>(headers);
                this.Method = method;
                this.QueryVars = new ReadOnlyDictionary<string, string>(queryVars);
                this.SupportedMethods = new List<string>();
            }

            #endregion Methods (4)
        }
    }
}