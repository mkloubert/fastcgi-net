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

using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MarcelJoachimKloubert.FastCGI
{
    /// <summary>
    /// Describes a FastCGI request context.
    /// </summary>
    public interface IRequestContext
    {
        #region Properties (4)

        /// <summary>
        /// Gets the server address.
        /// </summary>
        IPAddress Address { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        IRequestParameters Parameters { get; }

        /// <summary>
        /// Gets the server port.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        ISettings Settings { get; }

        #endregion Properties (4)

        #region Methods (6)

        /// <summary>
        /// Creates a stream that can be used for the input body that is send from the remote client.
        /// </summary>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        Stream CreateInputStream(ref int? readBufferSize, ref int? writeBufferSize);

        /// <summary>
        ///  Creates a stream that can be used for the output body that is send to the remote client.
        /// </summary>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        Stream CreateOutputStream(ref int? readBufferSize, ref int? writeBufferSize);

        /// <summary>
        /// Returns the whole body content.
        /// </summary>
        /// <returns>The body content.</returns>
        byte[] GetBody();

        /// <summary>
        /// Gets a new stream that can be used to access the body data.
        /// </summary>
        /// <returns>The created stream.</returns>
        Stream GetBodyStream(ref int? readBufferSize);

        /// <summary>
        /// Ends the requests.
        /// </summary>
        /// <returns>Operation was successfull (<see langword="true" />) or not (<see langword="false" />).</returns>
        bool End();

        /// <summary>
        /// Writes data to the client.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns>That instance.</returns>
        IRequestContext Write(IEnumerable<byte> data);

        #endregion Methods (6)
    }
}