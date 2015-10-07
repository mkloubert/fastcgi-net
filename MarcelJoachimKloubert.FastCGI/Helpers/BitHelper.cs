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
using System.Linq;

namespace MarcelJoachimKloubert.FastCGI.Helpers
{
    /// <summary>
    /// Helper class for bit and byte operations.
    /// </summary>
    public static class BitHelper
    {
        #region Methods (4)

        /// <summary>
        /// Returns binary data as array.
        /// </summary>
        /// <param name="data">The input value.</param>
        /// <param name="nullAsEmpty">
        /// Return an empty array if <paramref name="data" /> is <see langword="null" /> (<see langword="true" />)
        /// or <see langword="null" /> (<see langword="false" />).
        /// </param>
        /// <returns>The output value.</returns>
        public static byte[] AsArray(IEnumerable<byte> data, bool nullAsEmpty = false)
        {
            return CollectionHelper.AsArray<byte>(data, nullAsEmpty);
        }

        /// <summary>
        /// Converts a <see cref="ushort" /> to a byte array.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The converted data.</returns>
        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value)
                               .Reverse().ToArray();
        }

        /// <summary>
        /// Returns the (whole) content of a stream as byte array.
        /// </summary>
        /// <param name="stream">The stream to return.</param>
        /// <returns>
        /// The data of <paramref name="stream" />.
        /// </returns>
        public static byte[] ToByteArray(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }

            long? oldPosition = null;
            try
            {
                if (stream.CanSeek)
                {
                    oldPosition = stream.Position;
                    stream.Position = 0;
                }

                using (var temp = new MemoryStream())
                {
                    stream.CopyTo(temp);

                    return temp.ToArray();
                }
            }
            finally
            {
                if (oldPosition.HasValue)
                {
                    stream.Position = oldPosition.Value;
                }
            }
        }

        /// <summary>
        /// Converts binary data to <see cref="ushort" />.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>The converted data.</returns>
        public static ushort ToUInt16(IEnumerable<byte> data)
        {
            return BitConverter.ToUInt16(AsArray(data).Reverse().ToArray(),
                                         0);
        }

        #endregion Methods (4)
    }
}