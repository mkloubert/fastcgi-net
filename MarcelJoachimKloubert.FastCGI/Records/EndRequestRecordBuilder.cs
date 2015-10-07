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
    /// Builds FastCGI records for ending a request.
    /// </summary>
    public class EndRequestRecordBuilder : RecordBuilder
    {
        #region Fields (2)

        private uint _appStatus;
        private ProtocolStatus _status;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="EndRequestRecordBuilder" /> class.
        /// </summary>
        public EndRequestRecordBuilder()
            : base(type: RecordType.FCGI_END_REQUEST)
        {
            this.AppStatus = 0;
            this.Status = ProtocolStatus.FCGI_REQUEST_COMPLETE;
        }

        #endregion Constructors (1)

        #region Methods (1)

        /// <summary>
        /// Updates the value of <see cref="EndRequestRecordBuilder.Content" />.
        /// </summary>
        protected void UpdateContent()
        {
            using (var temp = new MemoryStream())
            {
                // appStatus
                temp.Write(BitHelper.GetBytes(this.AppStatus), 0, 4);

                // protocolStatus
                temp.WriteByte((byte)this.Status);

                // reserved
                temp.Write(new byte[3], 0, 3);

                base.Content = temp.ToArray();
            }
        }

        #endregion Methods (1)

        #region Properties (4)

        /// <summary>
        /// Gets or sets the value for the application status.
        /// </summary>
        public uint AppStatus
        {
            get { return this._appStatus; }

            set
            {
                this._appStatus = value;

                this.UpdateContent();
            }
        }

        /// <summary>
        /// <see cref="RecordBuilder.Content" />
        /// </summary>
        public override byte[] Content
        {
            get { return base.Content; }

            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ProtocolStatus Status
        {
            get { return this._status; }

            set
            {
                this._status = value;

                this.UpdateContent();
            }
        }

        /// <summary>
        /// <see cref="RecordBuilder.Type" />
        /// </summary>
        public override RecordType Type
        {
            get { return base.Type; }

            set { throw new NotSupportedException(); }
        }

        #endregion Properties (4)
    }
}