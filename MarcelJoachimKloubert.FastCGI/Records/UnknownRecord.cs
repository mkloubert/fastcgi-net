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

namespace MarcelJoachimKloubert.FastCGI.Records
{
    /// <summary>
    /// An unknown / no defined request.
    /// </summary>
    public class UnknownRecord
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownRecord" /> class.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <param name="type">The type.</param>
        /// <param name="data">The value for the <see cref="UnknownRecord.Data" /> property.</param>
        public UnknownRecord(ushort requestId, byte type, byte[] data)
        {
            this.RequestId = requestId;
            this.Type = type;
            this.Data = data ?? new byte[0];
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// Gets the (raw) data.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the request ID.
        /// </summary>
        public ushort RequestId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public byte Type
        {
            get;
            private set;
        }

        #endregion Properties (3)

        #region Methods (1)

        /// <summary>
        /// Creates requests for a stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <returns>The requests.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<UnknownRecord> FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var version = stream.ReadByte();
            if (version < 0)
            {
                yield break;
            }

            var type = stream.ReadByte();
            if (type < 0)
            {
                yield break;
            }

            byte[] buffer;
            int bytesRead;

            buffer = new byte[2];
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                yield break;
            }

            var requestId = BitHelper.ToUInt16(buffer);

            buffer = new byte[2];
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                yield break;
            }

            var contentLength = BitHelper.ToUInt16(buffer);

            buffer = new byte[1];
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                yield break;
            }

            var paddingLength = buffer[0];

            buffer = new byte[1];
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                yield break;
            }

            var reserved = buffer[0];

            var content = new byte[0];
            if (contentLength > 0)
            {
                buffer = new byte[contentLength];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead != buffer.Length)
                {
                    yield break;
                }

                content = buffer;
            }

            var padding = new byte[0];
            if (paddingLength > 0)
            {
                buffer = new byte[paddingLength];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead != buffer.Length)
                {
                    yield break;
                }

                padding = buffer;
            }

            RecordType reqType;
            if (Enum.TryParse<RecordType>(type.ToString(), out reqType))
            {
                switch (reqType)
                {
                    case RecordType.FCGI_BEGIN_REQUEST:
                        yield return new BeginRequestRecord(requestId, (byte)type, content);
                        break;

                    case RecordType.FCGI_PARAMS:
                        yield return new ParameterRecord(requestId, (byte)type, content);
                        break;

                    case RecordType.FCGI_STDIN:
                        yield return new InputRecord(requestId, (byte)type, content);
                        break;
                }
            }
            else
            {
                yield return new UnknownRecord(requestId, (byte)type, new byte[7]);
            }
        }

        #endregion Methods (1)
    }
}