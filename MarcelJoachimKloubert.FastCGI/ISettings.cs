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

using System.Net;

namespace MarcelJoachimKloubert.FastCGI
{
    /// <summary>
    /// Stores settings.
    /// </summary>
    public interface ISettings
    {
        #region Properties (8)

        /// <summary>
        /// Gets the handler to use.
        /// </summary>
        IRequestHandler Handler { get; }

        /// <summary>
        /// Creates the stream that is used to store the request body.
        /// </summary>
        StreamFactory InputStreamFactory { get; }

        /// <summary>
        /// Gets the local address for the server.
        /// </summary>
        IPAddress LocalAddress { get; }

        /// <summary>
        /// Gets the maximum body length in bytes.
        /// </summary>
        long? MaxBodyLength { get; }

        /// <summary>
        /// Creates the stream that is used to store the response body.
        /// </summary>
        StreamFactory OutputStreamFactory { get; }

        /// <summary>
        /// Gets the TCP port to use.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the object that is used for thread safe operations.
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// Gets the buffer size in bytes that is used to write to the connected FastCGI client.
        /// <see langword="null" /> indicates to use the system's default.
        /// </summary>
        int? WriteBufferSize { get; }

        #endregion Properties (8)
    }
}