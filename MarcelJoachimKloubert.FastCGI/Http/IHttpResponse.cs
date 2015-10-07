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
using System.IO;
using System.Text;

namespace MarcelJoachimKloubert.FastCGI.Http
{
    /// <summary>
    /// Describes a HTTP response context.
    /// </summary>
    public interface IHttpResponse
    {
        #region Properties (15)

        /// <summary>
        /// Gets or sets the response code. <see langword="null" /> indicates to use the default.
        /// </summary>
        int? Code { get; set; }

        /// <summary>
        /// Gets or sets the custom content length.
        /// </summary>
        long? ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets the underlying FastCGI context.
        /// </summary>
        IRequestContext Context { get; }

        /// <summary>
        /// Gets or sets the output encoding.
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the list of response headers.
        /// </summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets if operation is allowed (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        bool IsAllowed { get; set; }

        /// <summary>
        /// Gets or sets if the requesting client is authorized (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets if operation is forbidden (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        bool IsForbidden { get; set; }

        /// <summary>
        /// Gets or sets to return a HTTP 404 error (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        bool NotFound { get; set; }

        /// <summary>
        /// Gets or sets if a method is not implemented (<see langword="true" />); otherwise <see langword="false" />.
        /// </summary>
        bool NotImplemented { get; set; }

        /// <summary>
        /// Gets the buffer size in bytes <see cref="IHttpResponse.Stream" /> should be read with.
        /// <see langword="null" /> indicates to use the system default.
        /// </summary>
        int? ReadBufferSize { get; }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Gets or sets the version of the HTTP protocol.
        /// </summary>
        Version Version { get; set; }

        /// <summary>
        /// Gets the buffer size in bytes <see cref="IHttpResponse.Stream" /> should be written with.
        /// <see langword="null" /> indicates to use the system default.
        /// </summary>
        int? WriteBufferSize { get; }

        #endregion Properties (15)

        #region Methods (10)

        /// <summary>
        /// Sets up the response for HTML output.
        /// </summary>
        /// <returns>That instance.</returns>
        IHttpResponse SetupForHtml();

        /// <summary>
        /// Sets up the response for JSON output.
        /// </summary>
        /// <returns>That instance.</returns>
        IHttpResponse SetupForJson();

        /// <summary>
        /// Sets up the response for XML output.
        /// </summary>
        /// <returns>That instance.</returns>
        IHttpResponse SetupForXml();

        /// <summary>
        /// Writes a string to output.
        /// </summary>
        /// <param name="chars">The string / chars to write.</param>
        /// <returns>That instance.</returns>
        IHttpResponse Write(IEnumerable<char> chars);

        /// <summary>
        /// Writes a binary data to output.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns>That instance.</returns>
        IHttpResponse Write(IEnumerable<byte> data);

        /// <summary>
        /// Writes the data from a stream to output.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="bufferSize">The custom buffer size to use.</param>
        /// <returns>That instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream" /> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="bufferSize" /> is less than 1.
        /// </exception>
        IHttpResponse Write(Stream stream, int? bufferSize = null);

        /// <summary>
        /// Writes an object to output.
        /// </summary>
        /// <param name="obj">The object to write.</param>
        /// <returns>That instance.</returns>
        IHttpResponse Write(object obj);

        /// <summary>
        /// Writes a formatted string to the output.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="argList">The arguments for <paramref name="format" />.</param>
        /// <returns>That instance.</returns>
        IHttpResponse WriteFormat(IEnumerable<char> format, IEnumerable<object> argList);

        /// <summary>
        /// Writes a formatted string to the output.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments for <paramref name="format" />.</param>
        /// <returns>That instance.</returns>
        IHttpResponse WriteFormat(IEnumerable<char> format, params object[] args);

        /// <summary>
        /// Serizalizes an object as JSON string and writes it to output.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>That instance.</returns>
        IHttpResponse WriteJson(object obj);

        #endregion Methods (10)
    }
}