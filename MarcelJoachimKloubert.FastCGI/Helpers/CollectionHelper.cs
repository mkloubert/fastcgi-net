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
using System.Linq;

namespace MarcelJoachimKloubert.FastCGI.Helpers
{
    /// <summary>
    /// Helper class for collection operations.
    /// </summary>
    public static class CollectionHelper
    {
        #region Methods (2)

        /// <summary>
        /// Adds or sets a value for a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys.</typeparam>
        /// <typeparam name="TValue">Type of the values.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="value" /> was added or <see langword="false" /> if it was set.
        /// <see langword="null" /> indicates that <paramref name="dict" /> is <see langword="null" />.
        /// </returns>
        public static bool? AddOrSet<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null)
            {
                return null;
            }

            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return true;
            }

            dict[key] = value;
            return false;
        }

        /// <summary>
        /// Returns a sequence as array.
        /// </summary>
        /// <param name="seq">The input sequence.</param>
        /// <param name="nullAsEmpty">
        /// Return an empty array if <paramref name="seq" /> is <see langword="null" /> (<see langword="true" />)
        /// or <see langword="null" /> instead (<see langword="false" />).
        /// </param>
        /// <returns>The output value.</returns>
        public static T[] AsArray<T>(IEnumerable<T> seq, bool nullAsEmpty = false)
        {
            if (seq == null)
            {
                return !nullAsEmpty ? null : new T[0];
            }

            var result = seq as T[];
            if (result == null)
            {
                result = seq.ToArray();
            }

            return result;
        }

        #endregion Methods (2)
    }
}