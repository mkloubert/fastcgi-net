﻿/**********************************************************************************************************************
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

namespace MarcelJoachimKloubert.FastCGI.Collections
{
    /// <summary>
    /// A dictionary that only supports read operations and throws <see cref="NotSupportedException" />
    /// on write operations.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : DictionaryWrapperBase<TKey, TValue>
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dict">The dictionary to wrap.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dict" /> is <see langword="null" />.
        /// </exception>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
            : base(dict)
        {
        }

        #endregion Constructors (1)

        #region Methods (5)

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.Add(TKey, TValue)" />
        /// </summary>
        public sealed override void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.Add(KeyValuePair{TKey, TValue})" />
        /// </summary>
        public sealed override void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.Clear()" />
        /// </summary>
        public sealed override void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.Remove(TKey)" />
        /// </summary>
        public sealed override bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.Remove(KeyValuePair{TKey, TValue})" />
        /// </summary>
        public sealed override bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        #endregion Methods (5)

        #region Properties (2)

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.IsReadOnly" />
        /// </summary>
        public sealed override bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// <see cref="DictionaryWrapperBase{TKey, TValue}.this[TKey]" />
        /// </summary>
        public sealed override TValue this[TKey key]
        {
            get { return this._DICT[key]; }

            set { throw new NotSupportedException(); }
        }

        #endregion Properties (2)
    }
}