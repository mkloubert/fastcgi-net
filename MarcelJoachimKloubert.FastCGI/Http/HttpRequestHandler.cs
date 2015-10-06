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
using System.Text;

namespace MarcelJoachimKloubert.FastCGI.Http
{
    /// <summary>
    /// A HTTP request handler.
    /// </summary>
    public partial class HttpRequestHandler : IRequestHandler
    {
        #region Events (1)

        /// <summary>
        /// Is raised when an HTTP request is made.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> Request;

        #endregion Events (1)

        #region Methods (6)

        /// <summary>
        /// Creates a request context.
        /// </summary>
        /// <param name="context">The underlying FastCGI context.</param>
        /// <returns>The created instance.</returns>
        protected virtual IHttpRequest CreateRequest(IRequestContext context)
        {
            return new HttpRequest(context);
        }

        /// <summary>
        /// Creates a response context.
        /// </summary>
        /// <param name="context">The underlying FastCGI context.</param>
        /// <returns>The created instance.</returns>
        protected virtual IHttpResponse CreateResponse(IRequestContext context)
        {
            return new HttpResponse(context);
        }

        /// <summary>
        /// <see cref="IRequestHandler.HandleRequest(IRequestContext)" />
        /// </summary>
        public void HandleRequest(IRequestContext context)
        {
            if (context == null)
            {
                return;
            }

            this.OnHandleRequest(context);
        }

        /// <summary>
        /// The logic for the <see cref="HttpRequestHandler.HandleRequest(IRequestContext)" /> method.
        /// </summary>
        /// <param name="context">The current context.</param>
        protected virtual void OnHandleRequest(IRequestContext context)
        {
            var request = this.CreateRequest(context);

            var response = this.CreateResponse(context);
            response.Code = 200;
            response.Status = "OK";
            response.Version = new Version(1, 1);

            try
            {
                if (this.RaiseRequest(request, response))
                {

                }
                else
                {
                    response.Code = 501;
                    response.Status = "Not Implemented";
                }
            }
            catch
            {
                response.Code = 500;
                response.Status = "Internal Server Error";
            }

            var status = (response.Status ?? string.Empty).Trim();
            if (status != "")
            {
                status = " " + status;
            }

            var version = response.Version ?? new Version(1, 0);

            context.Write(Encoding.ASCII.GetBytes(string.Format("HTTP/{0} {1}{2}\r\n",
                                                                version, response.Code, status)));
            context.Write(Encoding.ASCII.GetBytes("Content-type: text/html\r\n"));
            context.Write(Encoding.ASCII.GetBytes("Content-length: 18\r\n"));
            context.Write(Encoding.ASCII.GetBytes("\r\n"));
            context.Write(Encoding.ASCII.GetBytes("<html>WORX!</html>"));

            context.End();
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <typeparam name="TArgs">Type of the event arguments.</typeparam>
        /// <param name="handler">The handler to raise.</param>
        /// <param name="e">The arguments for the event.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseEventHandler<TArgs>(EventHandler<TArgs> handler, TArgs e)
            where TArgs : global::System.EventArgs
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (handler != null)
            {
                handler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="HttpRequestHandler.Request" /> event.
        /// </summary>
        /// <param name="request">The request context.</param>
        /// <param name="response">The response context.</param>
        /// <returns>
        /// Event was raised (<see langword="true" />) or not (<see langword="false" />).
        /// </returns>
        protected bool RaiseRequest(IHttpRequest request, IHttpResponse response)
        {
            return this.RaiseEventHandler(this.Request,
                                          new HttpRequestEventArgs(request, response));
        }

        #endregion Methods (5)
    }
}