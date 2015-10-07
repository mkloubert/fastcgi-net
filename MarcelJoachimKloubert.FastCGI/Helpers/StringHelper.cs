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

namespace MarcelJoachimKloubert.FastCGI.Helpers
{
    /// <summary>
    /// Helper class for string operations.
    /// </summary>
    public static class StringHelper
    {
        #region Methods (1)

        /// <summary>
        /// Returns an object as string.
        /// </summary>
        /// <param name="value">The value to convert / cast.</param>
        /// <param name="nullAsEmpty">
        /// Returns a <see langword="null" /> reference as empty string (<see langword="true" />)
        /// or <see langword="null" /> instead (<see langword="false" />).
        /// </param>
        /// <returns></returns>
        public static string AsString(object value, bool nullAsEmpty = false)
        {
            string result = null;

            if (value != null)
            {
                if (value is string)
                {
                    result = (string)value;
                }
                else
                {
                    var chars = CollectionHelper.AsArray(value as IEnumerable<char>);
                    if (chars != null)
                    {
                        result = new string(chars);
                    }
                    else
                    {
                        if (value is TextReader)
                        {
                            result = ((TextReader)value).ReadToEnd();
                        }
                    }

                    result = value.ToString();
                }
            }

            return result != null ? result
                                  : (nullAsEmpty ? string.Empty : null);
        }

        #endregion Methods (1)
    }
}