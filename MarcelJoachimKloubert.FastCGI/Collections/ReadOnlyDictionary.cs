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
using System.Collections;
using System.Collections.Generic;

namespace MarcelJoachimKloubert.FastCGI.Collections
{
    /// <summary>
    /// A dictionary that only supports read operations and throws <see cref="NotSupportedException" />
    /// on write operations.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : FastCGIObject, IDictionary<TKey, TValue>
    {
        #region Fields (1)

        /// <summary>
        /// The inner dictionary.
        /// </summary>
        protected readonly IDictionary<TKey, TValue> _DICT;

        #endregion Fields (1)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dict">The dictionary to wrap.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dict" /> is <see langword="null" />.
        /// </exception>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException("dict");
            }

            this._DICT = dict;
        }

        #endregion Constructors (1)

        #region Methods (11)

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="ICollection{T}.Add(T)" />
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="ICollection{T}.Clear()" />
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="ICollection{T}.Contains(T)" />
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this._DICT.Contains(item);
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.ContainsKey(TKey)" />
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return this._DICT.ContainsKey(key);
        }

        /// <summary>
        /// <see cref="ICollection{T}.CopyTo(T[], int)" />
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this._DICT.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// <see cref="IEnumerable{T}.GetEnumerator()" />
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._DICT.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Remove(TKey)" />
        /// </summary>
        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="ICollection{T}.Remove(T)" />
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._DICT.TryGetValue(key, out value);
        }

        #endregion Methods (11)

        #region Properties

        /// <summary>
        /// <see cref="ICollection{T}.Count" />
        /// </summary>
        public int Count
        {
            get { return this._DICT.Count; }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Keys" />
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return this._DICT.Keys; }
        }

        /// <summary>
        /// <see cref="ICollection{T}.IsReadOnly" />
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.this[TKey]" />
        /// </summary>
        public TValue this[TKey key]
        {
            get { return this._DICT[key]; }

            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Values" />
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return this._DICT.Values; }
        }

        #endregion Properties
    }
}