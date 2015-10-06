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
using System.Linq;

namespace MarcelJoachimKloubert.FastCGI.Records
{
    /// <summary>
    /// Begins a request.
    /// </summary>
    public class BeginRequestRecord : RecordBase
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginRequestRecord" /> class.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <param name="type">The type.</param>
        /// <param name="data">The value for the <see cref="UnknownRecord.Data" /> property.</param>
        /// <param name="invokeInit">
        /// Invoke <see cref="BeginRequestRecord.Init()" /> method (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        public BeginRequestRecord(ushort requestId, byte type, byte[] data, bool invokeInit = true)
            : base(requestId: requestId, type: type, data: data)
        {
            if (invokeInit)
            {
                this.Init();
            }
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// Gets if connection should be closed after the request.
        /// </summary>
        public bool? CloseConnection
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the known role (if available).
        /// </summary>
        public RoleType? KnownRole
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ID of the role.
        /// </summary>
        public ushort? Role
        {
            get;
            private set;
        }

        #endregion Properties (3)

        #region Methods (1)

        /// <summary>
        /// <see cref="RecordBase.Init()" />
        /// </summary>
        protected override void Init()
        {
            if (this.Data.Length > 1)
            {
                this.Role = BitHelper.ToUInt16(this.Data.Take(2));
            }

            if (this.Role.HasValue)
            {
                RoleType role;
                if (Enum.TryParse<RoleType>(this.Role.ToString(), out role))
                {
                    this.KnownRole = role;
                }
            }

            if (this.Data.Length > 2)
            {
                this.CloseConnection = 0 == this.Data[2];
            }
        }

        #endregion Methods (1)
    }
}