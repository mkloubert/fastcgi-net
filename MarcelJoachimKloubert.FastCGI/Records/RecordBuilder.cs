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
using System.IO;

namespace MarcelJoachimKloubert.FastCGI.Records
{
    /// <summary>
    /// Builds FastCGI records.
    /// </summary>
    public class RecordBuilder : FastCGIObject
    {
        #region Fields (6)

        private byte[] _content;
        private byte[] _padding;
        private RecordType _type;

        /// <summary>
        /// Stores the request ID.
        /// </summary>
        public ushort RequestId = 0;

        /// <summary>
        /// Stores the &quot;reserved&quot; byte.
        /// </summary>
        public byte Reserved = 0;

        /// <summary>
        /// Stores the version.
        /// </summary>
        public byte Version = 1;

        #endregion Fields (6)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordBuilder" /> class.
        /// </summary>
        /// <param name="type">The initial value for the <see cref="RecordBuilder.Type" /> property.</param>
        public RecordBuilder(RecordType type = RecordType.FCGI_STDOUT)
        {
            this._type = type;
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">New value is too big (max. 65535 bytes).</exception>
        public virtual byte[] Content
        {
            get { return this._content; }

            set
            {
                if (value != null)
                {
                    if (value.LongLength > ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value", value.LongLength, "Maximum size is " + ushort.MaxValue);
                    }
                }

                this._content = value;
            }
        }

        /// <summary>
        /// Gets or sets the padding data.
        /// </summary>
        public byte[] Padding
        {
            get { return this._padding; }

            set
            {
                if (value != null)
                {
                    if (value.LongLength > byte.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value", value.LongLength, "Maximum size is " + byte.MaxValue);
                    }
                }

                this._padding = value;
            }
        }

        /// <summary>
        /// Stores the record type.
        /// </summary>
        public virtual RecordType Type
        {
            get { return this._type; }

            set { this._type = value; }
        }

        #endregion Properties (3)

        #region Method (1)

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <returns>The created data.</returns>
        public byte[] Build()
        {
            using (var temp = new MemoryStream())
            {
                var content = BitHelper.AsArray(this.Content, true);
                var padding = BitHelper.AsArray(this.Padding, true);

                // version
                temp.WriteByte(this.Version);

                // type
                temp.WriteByte((byte)this.Type);

                // requestId
                temp.Write(BitHelper.GetBytes(this.RequestId), 0, 2);

                // contentLength
                temp.Write(BitHelper.GetBytes((ushort)content.Length), 0, 2);

                // paddingLength
                temp.WriteByte((byte)padding.Length);

                // reserved
                temp.WriteByte(this.Reserved);

                // contentData
                temp.Write(content, 0, content.Length);

                // paddingData
                temp.Write(padding, 0, padding.Length);

                return temp.ToArray();
            }
        }

        #endregion Method (1)
    }
}