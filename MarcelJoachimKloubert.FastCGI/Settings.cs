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
using System.Net;

namespace MarcelJoachimKloubert.FastCGI
{
    /// <summary>
    /// Settings.
    /// </summary>
    public class Settings : FastCGIObject, ISettings
    {
        #region Fields (2)

        private Lazy<object> _syncRoot;

        /// <summary>
        /// Stores the default TCP port.
        /// </summary>
        public int DEFAULT_PORT = 9001;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of <see cref="Settings" /> class.
        /// </summary>
        public Settings()
        {
            this.Port = DEFAULT_PORT;
            this.SyncRoot = null;
        }

        #endregion Constructors (1)

        #region Properties (7)

        /// <summary>
        /// <see cref="ISettings.LocalAddress" />
        /// </summary>
        public IRequestHandler Handler { get; set; }

        /// <summary>
        /// <see cref="ISettings.InputStreamFactory" />
        /// </summary>
        public StreamFactory InputStreamFactory { get; set; }

        /// <summary>
        /// <see cref="ISettings.LocalAddress" />
        /// </summary>
        public IPAddress LocalAddress { get; set; }

        /// <summary>
        /// <see cref="ISettings.OutputStreamFactory" />
        /// </summary>
        public StreamFactory OutputStreamFactory { get; set; }

        /// <summary>
        /// <see cref="ISettings.Port" />
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// <see cref="ISettings.SyncRoot" />
        /// </summary>
        public object SyncRoot
        {
            get { return this._syncRoot.Value; }

            set { this._syncRoot = new Lazy<object>(() => value ?? new object()); }
        }

        /// <summary>
        /// <see cref="ISettings.WriteBufferSize" />
        /// </summary>
        public int? WriteBufferSize { get; set; }

        #endregion Properties (7)
    }
}