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
using System.Text;

namespace MarcelJoachimKloubert.FastCGI.Http
{
    /// <summary>
    /// A HTTP request handler.
    /// </summary>
    public partial class HttpRequestHandler : FastCGIObject, IRequestHandler
    {
        #region Fields (4)

        /// <summary>
        /// Stores the header name for the content length.
        /// </summary>
        public const string HEADER_CONTENT_LENGTH = "Content-length";

        /// <summary>
        /// The header name for the content type.
        /// </summary>
        public const string HEADER_CONTENT_TYPE = "Content-type";

        /// <summary>
        /// Extra header: powered by
        /// </summary>
        public const string HEADER_X_POWERED_BY = "X-Powered-By";

        /// <summary>
        /// The header name for the status.
        /// </summary>
        public const string HEADER_STATUS = "Status";

        #endregion Fields (4)

        #region Events (11)

        /// <summary>
        /// Is invoked AFTER a request has been handled (or not).
        /// </summary>
        public event EventHandler<HttpAfterRequestEventArgs> AfterRequest;

        /// <summary>
        /// Is raised for authorize credentials.
        /// </summary>
        public event EventHandler<HttpAuthorizeEventArgs> Authorize;

        /// <summary>
        /// Is invoked BEFORE a request is handled.
        /// </summary>
        public event EventHandler<HttpBeforeRequestEventArgs> BeforeRequest;

        /// <summary>
        /// Is raised if a request failed (500).
        /// </summary>
        public event EventHandler<HttpRequestServerErrorEventArgs> Error;

        /// <summary>
        /// Is raised if a request is forbidden (403).
        /// </summary>
        public event EventHandler<HttpRequestClientErrorEventArgs> Forbidden;

        /// <summary>
        /// Is raised if a method is NOT allowed (405).
        /// </summary>
        public event EventHandler<HttpRequestClientErrorEventArgs> NotAllowed;

        /// <summary>
        /// Is raised if a resource was not found (404).
        /// </summary>
        public event EventHandler<HttpRequestClientErrorEventArgs> NotFound;

        /// <summary>
        /// Is raised if a method is not implemented (501).
        /// </summary>
        public event EventHandler<HttpRequestServerErrorEventArgs> NotImplemented;

        /// <summary>
        /// Is raised when an HTTP request is made.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> Request;

        /// <summary>
        /// Is raised if a request is unauthorized (401).
        /// </summary>
        public event EventHandler<HttpRequestClientErrorEventArgs> Unauthorized;

        #endregion Events (11)

        #region Methods (9)

        /// <summary>
        /// Checks credentials.
        /// </summary>
        /// <param name="request">The underlying request.</param>
        /// <returns>Credentials are valid (<see langword="true" />) or not (<see langword="false" />).</returns>
        protected virtual bool CheckCredentials(IHttpRequest request)
        {
            string username = null;
            string password = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                username = null;
            }
            else
            {
                username = username.Trim();
            }

            if (string.IsNullOrEmpty(password))
            {
                password = null;
            }

            var e = new HttpAuthorizeEventArgs(username, password);

            if (!this.RaiseEventHandler(this.Authorize, e))
            {
                e.IsAuthorized = true;
            }

            return e.IsAuthorized;
        }

        /// <summary>
        /// Creates a request context.
        /// </summary>
        /// <param name="context">The underlying FastCGI context.</param>
        /// <returns>The created instance.</returns>
        protected virtual IHttpRequest CreateRequest(IRequestContext context)
        {
            var result = new HttpRequest(context);

            // allowed methods
            result.SupportedMethods.Add("GET");
            result.SupportedMethods.Add("POST");
            result.SupportedMethods.Add("OPTIONS");
            result.SupportedMethods.Add("HEAD");
#if DEBUG
            result.SupportedMethods.Add("TRACE");
#endif

            return result;
        }

        /// <summary>
        /// Creates a response context.
        /// </summary>
        /// <param name="context">The underlying FastCGI context.</param>
        /// <returns>The created instance.</returns>
        protected virtual IHttpResponse CreateResponse(IRequestContext context)
        {
            var result = new HttpResponse(context);
            result.SetupForHtml();
            result.Code = 200;
            result.NotFound = true;
            result.NotImplemented = false;
            result.Status = "OK";
            result.Version = new Version(1, 1);

            int? readBufferSize = null;
            int? writeBufferSize = null;
            result.Stream = context.CreateOutputStream(ref readBufferSize, ref writeBufferSize);

            if (readBufferSize < 1)
            {
                readBufferSize = 10240;
            }

            if (writeBufferSize < 1)
            {
                writeBufferSize = 10240;
            }

            result.ReadBufferSize = readBufferSize;
            result.WriteBufferSize = writeBufferSize;

            return result;
        }

        /// <summary>
        /// Gets the default content type.
        /// </summary>
        /// <param name="request">The underlying request context.</param>
        /// <param name="response">The underlying response context.</param>
        /// <returns>The content type.</returns>
        protected virtual IEnumerable<char> GetDefaultContentType(IHttpRequest request, IHttpResponse response)
        {
            return "application/octet-stream";
        }

        /// <summary>
        /// Gets an expression that represents the separator for a new line.
        /// </summary>
        /// <param name="request">The underlying request context.</param>
        /// <param name="response">The underlying response context.</param>
        /// <returns>The &quot;new line&quot; expression.</returns>
        protected virtual IEnumerable<char> GetNewLine(IHttpRequest request, IHttpResponse response)
        {
            return null;
        }

        /// <summary>
        /// Gets the encoder for the response data.
        /// </summary>
        /// <param name="request">The underlying request context.</param>
        /// <param name="response">The underlying response context.</param>
        /// <returns>The encoder.</returns>
        protected virtual Encoding GetResponseEncoder(IHttpRequest request, IHttpResponse response)
        {
            return null;
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

            response.IsAllowed = request.IsMethodAllowed;

            Exception error = null;
            Stream outputStream = null;
            bool? skipped = null;

            Func<bool> handleForbidden = () =>
                {
                    response.Code = 403;
                    response.Status = "Forbidden";

                    var e = new HttpRequestClientErrorEventArgs(request, response);
                    e.Handled = true;

                    if (!this.RaiseEventHandler(this.Forbidden, e))
                    {
                        e.Handled = false;
                    }

                    return e.Handled;
                };

            Func<bool> handleNotAllowed = () =>
                {
                    response.Code = 405;
                    response.Status = "Method Not Allowed";

                    var e = new HttpRequestClientErrorEventArgs(request, response);
                    e.Handled = true;

                    if (!this.RaiseEventHandler(this.NotAllowed, e))
                    {
                        e.Handled = false;
                    }

                    return e.Handled;
                };

            Func<bool> handleNotFound = () =>
                {
                    response.Code = 404;
                    response.Status = "Not Found";

                    var e = new HttpRequestClientErrorEventArgs(request, response);
                    e.Handled = true;

                    if (!this.RaiseEventHandler(this.NotFound, e))
                    {
                        e.Handled = false;
                    }

                    if (!e.Handled)
                    {
                        outputStream = null;
                    }

                    return e.Handled;
                };

            Func<bool> handleUnauthorized = () =>
                {
                    response.Code = 401;
                    response.Status = "Unauthorized";

                    var e = new HttpRequestClientErrorEventArgs(request, response);
                    e.Handled = true;

                    if (!this.RaiseEventHandler(this.Unauthorized, e))
                    {
                        e.Handled = false;
                    }

                    return e.Handled;
                };

            try
            {
                if (request.IsMethodAllowed)
                {
                    response.IsAuthorized = this.CheckCredentials(request);

                    if (response.IsAuthorized)
                    {
                        var bre = new HttpBeforeRequestEventArgs(request, response);
                        bre.Skip = false;

                        this.RaiseEventHandler(this.BeforeRequest, bre);

                        skipped = bre.Skip;

                        if (!response.IsAllowed)
                        {
                            handleNotAllowed();
                        }
                        else if (!response.IsAuthorized)
                        {
                            handleUnauthorized();
                        }
                        else if (response.IsForbidden)
                        {
                            handleForbidden();
                        }
                        else if (false == skipped)
                        {
                            bool notImplemented;
                            if (this.RaiseRequest(request, response))
                            {
                                outputStream = response.Stream;

                                if (response.NotFound)
                                {
                                    handleNotFound();
                                }

                                notImplemented = response.NotImplemented;
                            }
                            else
                            {
                                notImplemented = true;
                            }

                            if (notImplemented)
                            {
                                response.Code = 501;
                                response.Status = "Not Implemented";

                                var e = new HttpRequestServerErrorEventArgs(request, response);
                                e.Handled = true;

                                if (!this.RaiseEventHandler(this.NotImplemented, e))
                                {
                                    e.Handled = false;
                                }

                                if (!e.Handled)
                                {
                                    outputStream = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        handleUnauthorized();
                    }
                }
                else
                {
                    handleNotAllowed();
                }
            }
            catch (Exception ex)
            {
                error = ex;

                response.Code = 500;
                response.Status = "Internal Server Error";

                var e = new HttpRequestServerErrorEventArgs(request, response, ex);
                e.Handled = true;

                this.RaiseEventHandler(this.Error, e);

                if (!e.Handled)
                {
                    throw ex;
                }
            }
            finally
            {
                var e = new HttpAfterRequestEventArgs(request, response, skipped, error);
                e.Handled = true;

                this.RaiseEventHandler(this.AfterRequest, e);

                if (!e.Handled)
                {
                    if (e.Error != null)
                    {
                        throw e.Error;
                    }
                }
            }

            var encoder = this.GetResponseEncoder(request, response) ?? Encoding.ASCII;
            var newLine = StringHelper.AsString(this.GetNewLine(request, response)) ?? "\r\n";

            var statusDescription = (response.Status ?? string.Empty).Trim();
            if (statusDescription != "")
            {
                statusDescription = " " + statusDescription;
            }

            var code = response.Code;
            var contentLength = response.ContentLength;
            var headers = response.Headers;
            var version = response.Version;
            int? bufferReadSize = null;
            var updateHeaders = true;

            Action<string, object> sendHeader = (headerName, headerValue) =>
                {
                    context.Write(encoder.GetBytes(string.Format("{0}: {1}{2}",
                                                                 headerName, headerValue,
                                                                 newLine)));
                };

            Action unsetOutputStream = () =>
                {
                    outputStream = null;

                    //TODO dispose
                };

            Action sendOutputStream = () =>
                {
                    if (outputStream == null)
                    {
                        return;
                    }

                    long? oldPosition = null;

                    try
                    {
                        if (outputStream.CanSeek)
                        {
                            oldPosition = outputStream.Position;
                            outputStream.Position = 0;
                        }

                        var buffer = new byte[response.ReadBufferSize ?? 10240];

                        int bytesRead;
                        while ((bytesRead = outputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            var dataToWrite = buffer;
                            if (bytesRead != buffer.Length)
                            {
                                dataToWrite = CollectionHelper.AsArray(buffer.Take(bytesRead));
                            }

                            response.Context.Write(dataToWrite);
                        }
                    }
                    finally
                    {
                        if (oldPosition.HasValue)
                        {
                            outputStream.Position = oldPosition.Value;
                        }
                    }
                };

            Action sendResponse = () =>
                {
                    try
                    {
                        var forcedHeaders = new Dictionary<string, string>(new CaseInsensitiveStringComparer());
                        forcedHeaders.Add(HEADER_X_POWERED_BY, "fastcgi-net");

                        var status = string.Format("{0}{1}",
                                                   code ?? 200, statusDescription);

                        forcedHeaders.Add(HEADER_STATUS, status);

                        context.Write(encoder.GetBytes(string.Format("HTTP/{0} {1}" + newLine,
                                                                     version ?? new Version(1, 0),
                                                                     status)));

                        // custom headers
                        if (headers != null)
                        {
                            foreach (var entry in headers)
                            {
                                if (forcedHeaders.ContainsKey(HEADER_STATUS))
                                {
                                    // Status:
                                    continue;
                                }

                                var headerName = (entry.Key ?? string.Empty).Replace("\t", "    ")
                                                                            .Replace(" ", "-")
                                                                            .Trim();
                                if (headerName == "")
                                {
                                    continue;
                                }

                                var headerValue = entry.Value;

                                if (headerName.ToLower() == HEADER_CONTENT_TYPE.ToLower())
                                {
                                    var enc = response.Encoding;

                                    headerValue = (headerValue ?? string.Empty).ToLower().Trim();
                                    if (headerValue == "")
                                    {
                                        headerValue = StringHelper.AsString(this.GetDefaultContentType(request, response));
                                    }

                                    if (enc != null)
                                    {
                                        // append charset
                                        headerValue += "; charset=" + enc.WebName;
                                    }
                                }

                                sendHeader(headerName, headerValue);
                            }
                        }

                        // forced headers
                        foreach (var entry in forcedHeaders)
                        {
                            sendHeader(entry.Key, entry.Value);
                        }

                        // update headers
                        if (updateHeaders)
                        {
                            // content length
                            {
                                if (!contentLength.HasValue)
                                {
                                    if (outputStream != null)
                                    {
                                        try
                                        {
                                            if (outputStream.CanSeek)
                                            {
                                                contentLength = outputStream.Length;
                                            }
                                        }
                                        catch
                                        {
                                            // ignore errors
                                        }
                                    }
                                }

                                if (contentLength.HasValue)
                                {
                                    sendHeader(HEADER_CONTENT_LENGTH, contentLength);
                                }
                            }
                        }

                        // separator between header and body
                        context.Write(encoder.GetBytes(newLine));

                        sendOutputStream();
                    }
                    finally
                    {
                        context.End();
                    }
                };

            if (request.IsMethodAllowed)
            {
                switch (request.KnownMethod)
                {
                    case HttpMethod.HEAD:
                        unsetOutputStream();
                        break;

                    case HttpMethod.OPTIONS:
                        code = 200;

                        var supportedMethods = request.SupportedMethods ?? new List<string>();

                        headers = new Dictionary<string, string>();
                        headers.Add("Allow",
                                    string.Join(",", supportedMethods.Select(x => (x ?? string.Empty).ToUpper().Trim())
                                                                     .Where(x => x != string.Empty)
                                                                     .Distinct()));

                        unsetOutputStream();
                        break;

                    case HttpMethod.TRACE:
                        unsetOutputStream();

                        headers = request.Headers;
                        outputStream = context.GetBodyStream(ref bufferReadSize);
                        updateHeaders = false;
                        break;
                }
            }

            sendResponse();
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

        #endregion Methods (9)
    }
}