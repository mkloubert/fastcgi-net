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
using System.Text;
using ServerRequestParameterComparer = MarcelJoachimKloubert.FastCGI.CaseInsensitiveComparer;
using ServerRequestParameters = MarcelJoachimKloubert.FastCGI.Server.RequestParameters;

namespace MarcelJoachimKloubert.FastCGI.Records
{
    /// <summary>
    /// Request parameters.
    /// </summary>
    public class ParameterRecord : RecordBase
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRecord" /> class.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <param name="type">The type.</param>
        /// <param name="data">The value for the <see cref="UnknownRecord.Data" /> property.</param>
        /// <param name="invokeInit">
        /// Invoke <see cref="ParameterRecord.Init()" /> method (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        public ParameterRecord(ushort requestId, byte type, byte[] data, bool invokeInit = true)
            : base(requestId: requestId, type: type, data: data)
        {
            if (invokeInit)
            {
                this.Init();
            }
        }

        #endregion Constructors (1)

        #region Properties (1)

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public IRequestParameters Parameters
        {
            get;
            private set;
        }

        #endregion Properties (1)

        #region Methods (1)

        /// <summary>
        /// <see cref="RecordBase.Init()" />
        /// </summary>
        protected override void Init()
        {
            var values = new Dictionary<string, byte[]>(new ServerRequestParameterComparer());

            using (var temp = new MemoryStream(this.Data, false))
            {
                do
                {
                    var nameLength = temp.ReadByte();
                    if (nameLength < 0)
                    {
                        break;
                    }

                    var valueLength = temp.ReadByte();
                    if (valueLength < 0)
                    {
                        break;
                    }

                    byte[] buffer;
                    int bytesRead;

                    buffer = new byte[nameLength];
                    bytesRead = temp.Read(buffer, 0, buffer.Length);
                    if (bytesRead != buffer.Length)
                    {
                        break;
                    }

                    var name = Encoding.UTF8.GetString(buffer).Trim();

                    buffer = new byte[valueLength];
                    bytesRead = temp.Read(buffer, 0, buffer.Length);
                    if (bytesRead != buffer.Length)
                    {
                        break;
                    }

                    var value = buffer;

                    if (values.ContainsKey(name))
                    {
                        values[name] = value;
                    }
                    else
                    {
                        values.Add(name, value);
                    }
                }
                while (true);
            }

            var @params = new ServerRequestParameters()
            {
                Parameters = values,
            };

            this.Parameters = @params;
        }

        #endregion Methods (1)
    }
}