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
        #region Fields (5)

        private int _port;
        private long? _maxBodyLength;
        private int? _maxRequestsByConnection;
        private int? _writeBufferSize;

        /// <summary>
        /// Stores the default TCP port.
        /// </summary>
        public int DEFAULT_PORT = 9001;

        #endregion Fields (5)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of <see cref="Settings" /> class.
        /// </summary>
        public Settings()
        {
            // 8 MB
            this.MaxBodyLength = 8 * 1024 * 1024;

            this.MaxRequestsByConnection = 64;

            this.Port = DEFAULT_PORT;
        }

        #endregion Constructors (1)

        #region Properties (8)

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
        /// <see cref="ISettings.MaxBodyLength" />
        /// </summary>
        public long? MaxBodyLength
        {
            get { return this._maxBodyLength; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          "Must be 0 at least!");
                }

                this._maxBodyLength = value;
            }
        }

        /// <summary>
        /// <see cref="ISettings.MaxRequestsByConnection" />
        /// </summary>
        public int? MaxRequestsByConnection
        {
            get { return this._maxRequestsByConnection; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          "Must be 0 at least!");
                }

                this._maxRequestsByConnection = value;
            }
        }

        /// <summary>
        /// <see cref="ISettings.OutputStreamFactory" />
        /// </summary>
        public StreamFactory OutputStreamFactory { get; set; }

        /// <summary>
        /// <see cref="ISettings.Port" />
        /// </summary>
        public int Port
        {
            get { return this._port; }

            set
            {
                if ((value < IPEndPoint.MinPort) || (value > IPEndPoint.MaxPort))
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          string.Format("Allowed values are between {0} and {1}!",
                                                                        IPEndPoint.MinPort, IPEndPoint.MaxPort));
                }

                this._port = value;
            }
        }

        /// <summary>
        /// <see cref="ISettings.WriteBufferSize" />
        /// </summary>
        public int? WriteBufferSize
        {
            get { return this._writeBufferSize; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          "Must be 1 at least!");
                }

                this._writeBufferSize = value;
            }
        }

        #endregion Properties (8)
    }
}