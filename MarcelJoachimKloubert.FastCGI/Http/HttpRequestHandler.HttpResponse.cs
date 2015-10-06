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

namespace MarcelJoachimKloubert.FastCGI.Http
{
    partial class HttpRequestHandler
    {
        /// <summary>
        /// A HTTP response context.
        /// </summary>
        protected class HttpResponse : IHttpResponse
        {
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

            #region Properties (4)

            /// <summary>
            /// <see cref="IHttpResponse.Code" />
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Context" />
            /// </summary>
            public IRequestContext Context
            {
                get;
                private set;
            }

            /// <summary>
            /// <see cref="IHttpResponse.Status" />
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// <see cref="IHttpResponse.Version" />
            /// </summary>
            public Version Version { get; set; }

            #endregion Properties (4)

            #region Methods (1)

            /// <summary>
            /// Initializes that class.
            /// </summary>
            protected virtual void OnInit()
            {
            }

            #endregion Methods (1)
        }
    }
}