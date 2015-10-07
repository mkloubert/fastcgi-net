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
        #region Fields (2)

        /// <summary>
        /// Stores the header name for the content length.
        /// </summary>
        public const string HEADER_CONTENT_LENGTH = "Content-length";

        /// <summary>
        /// The header name for the content type.
        /// </summary>
        public const string HEADER_CONTENT_TYPE = "Content-type";

        #endregion Fields (2)

        #region Events (6)

        /// <summary>
        /// Is invoked AFTER a request has been handled (or not).
        /// </summary>
        public event EventHandler<HttpAfterRequestEventArgs> AfterRequest;

        /// <summary>
        /// Is invoked BEFORE a request is handled.
        /// </summary>
        public event EventHandler<HttpBeforeRequestEventArgs> BeforeRequest;

        /// <summary>
        /// Is raised if a request failed (500).
        /// </summary>
        public event EventHandler<HttpRequestServerErrorEventArgs> Error;

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

        #endregion Events (6)

        #region Methods (9)

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

            Exception error = null;
            Stream outputStream = null;
            bool? skipped = null;

            try
            {
                var bre = new HttpBeforeRequestEventArgs(request, response);
                bre.Skip = false;

                this.RaiseEventHandler(this.BeforeRequest, bre);

                skipped = bre.Skip;

                if (false == skipped)
                {
                    bool notImplemented;
                    if (this.RaiseRequest(request, response))
                    {
                        outputStream = response.Stream;

                        if (response.NotFound)
                        {
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

            var contentLength = response.ContentLength;

            var status = (response.Status ?? string.Empty).Trim();
            if (status != "")
            {
                status = " " + status;
            }

            context.Write(encoder.GetBytes(string.Format("HTTP/{0} {1}{2}" + newLine,
                                                         response.Version ?? new Version(1, 0),
                                                         response.Code ?? 200, status)));

            if (response.Headers != null)
            {
                foreach (var entry in response.Headers)
                {
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

                    context.Write(encoder.GetBytes(string.Format("{0}: {1}{2}",
                                                                 headerName, headerValue,
                                                                 newLine)));
                }
            }

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
                    context.Write(encoder.GetBytes(string.Format("{0}: {1}{2}",
                                                                 HEADER_CONTENT_LENGTH, contentLength,
                                                                 newLine)));
                }
            }

            // separator between header and body
            context.Write(encoder.GetBytes(newLine));

            if (outputStream != null)
            {
                this.OutputStream(outputStream, request, response);
            }

            context.End();
        }

        /// <summary>
        /// Outputs a stream.
        /// </summary>
        /// <param name="stream">The stream to output.</param>
        /// <param name="request">The request context.</param>
        /// <param name="response">The response context.</param>
        protected virtual void OutputStream(Stream stream, IHttpRequest request, IHttpResponse response)
        {
            long? oldPosition = null;

            try
            {
                if (stream.CanSeek)
                {
                    oldPosition = stream.Position;
                    stream.Position = 0;
                }

                var buffer = new byte[response.ReadBufferSize ?? 10240];

                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
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
                    stream.Position = oldPosition.Value;
                }
            }
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