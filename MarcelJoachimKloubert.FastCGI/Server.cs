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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.FastCGI
{
    /// <summary>
    /// A FastCGI server.
    /// </summary>
    public partial class Server : FastCGIObject, IDisposable
    {
        #region Fields (3)

        /// <summary>
        /// The current TCP listener.
        /// </summary>
        protected TcpListener _listener;

        /// <summary>
        /// Stores the settings.
        /// </summary>
        protected ISettings _SETTINGS;

        /// <summary>
        /// An object for thread safe operations.
        /// </summary>
        protected readonly object _SYNC;

        #endregion Fields (3)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of <see cref="Server" /> class.
        /// </summary>
        /// <param name="settings">The custom settings.</param>
        public Server(ISettings settings = null)
        {
            this._SETTINGS = settings ?? new Settings();

            this._SYNC = this._SETTINGS.SyncRoot ?? new object();
        }

        /// <summary>
        /// The destructor.
        /// </summary>
        ~Server()
        {
            this.Dispose(false);
        }

        #endregion Constructors (1)

        #region Events (7)

        /// <summary>
        /// Is raised when server begins disposing itself.
        /// </summary>
        public event EventHandler Disposing;

        /// <summary>
        /// Is raised when the server has been disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Is raised on an error.
        /// </summary>
        public event EventHandler<ServerErrorEventArgs> Error;

        /// <summary>
        /// Is raised after server has been started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Is raised when server begins start process.
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Is raised after server has been stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Is raised when server begins stop process.
        /// </summary>
        public event EventHandler Stopping;

        #endregion Events (7)

        #region Properties (3)

        /// <summary>
        /// Gets if the server has been disposed (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets if the server is running (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the object that is used for thread safe operations.
        /// </summary>
        public object SyncRoot
        {
            get { return this._SYNC; }
        }

        #endregion Properties (3)

        #region Methods (16)

        /// <summary>
        /// Starts listening for a TCP client connection.
        /// </summary>
        /// <param name="listener">The underlying listener.</param>
        /// <param name="throwException">
        /// Throw exception (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        /// <returns>
        /// Operation was successful (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="listener" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="ServerException">
        /// The raised exception.
        /// </exception>
        protected bool? BeginAcceptingTcpClient(TcpListener listener, bool throwException = false)
        {
            if (listener == null)
            {
                return null;
            }

            try
            {
                listener.BeginAcceptTcpClient(this.EndAcceptingTcpClient, listener);
                return true;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, throwException);
                return false;
            }
        }

        /// <summary>
        /// Creates a stream that can be used for the input body that is send from the remote client.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        public Stream CreateInputStream(IRequestContext context, ref int? readBufferSize, ref int? writeBufferSize)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var factory = this._SETTINGS.InputStreamFactory ?? this.DefaultInputStreamFactory;

            var result = factory(context, ref readBufferSize, ref writeBufferSize);
            if (result == null)
            {
                result = new MemoryStream();

                readBufferSize = null;
                writeBufferSize = null;
            }

            return result;
        }

        /// <summary>
        /// Creates a stream that can be used for the output body that is send to the remote client.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        public Stream CreateOutputStream(IRequestContext context, ref int? readBufferSize, ref int? writeBufferSize)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var factory = this._SETTINGS.OutputStreamFactory ?? this.DefaultOutputStreamFactory;

            var result = factory(context, ref readBufferSize, ref writeBufferSize);
            if (result == null)
            {
                result = new MemoryStream();

                readBufferSize = null;
                writeBufferSize = null;
            }

            return result;
        }

        /// <summary>
        /// The default factory that created a stream that can be used for the input body that is send from the remote client.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        protected virtual Stream DefaultInputStreamFactory(IRequestContext context, ref int? readBufferSize, ref int? writeBufferSize)
        {
            return new MemoryStream();
        }

        /// <summary>
        /// The default factory that created a stream that can be used for the output body that is send to the remote client.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="readBufferSize">The variable where to write down the buffer size the new stream can be read with.</param>
        /// <param name="writeBufferSize">The variable where to write down the buffer size the new stream can be written with.</param>
        /// <returns>The created stream.</returns>
        protected virtual Stream DefaultOutputStreamFactory(IRequestContext context, ref int? readBufferSize, ref int? writeBufferSize)
        {
            return new MemoryStream();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()" />
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (this._SYNC)
            {
                if (disposing && this.IsDisposed)
                {
                    return;
                }

                try
                {
                    if (disposing)
                    {
                        this.RaiseEventHandler(this.Disposing);
                    }

                    var isDisposed = disposing ? true : this.IsDisposed;
                    this.OnDispose(disposing, ref isDisposed);

                    this.IsDisposed = isDisposed;
                    if (this.IsDisposed)
                    {
                        this.RaiseEventHandler(this.Disposed);
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, disposing);
                }
            }
        }

        /// <summary>
        /// The async callback for <see cref="Server.BeginAcceptingTcpClient(TcpListener, bool)" /> method.
        /// </summary>
        /// <param name="ar">The async result.</param>
        protected void EndAcceptingTcpClient(IAsyncResult ar)
        {
            TcpListener listener = null;
            try
            {
                listener = ar.AsyncState as TcpListener;
                if (listener == null)
                {
                    return;
                }

                var client = listener.EndAcceptTcpClient(ar);

                this.StartCommunicationWithTcpClient(client);
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);
            }
            finally
            {
                this.BeginAcceptingTcpClient(listener);
            }
        }

        /// <summary>
        /// The logic for the <see cref="Server.Dispose()" /> method or the destructor.
        /// </summary>
        /// <param name="disposing">
        /// <see cref="Server.Dispose()" /> method was invoked (<see langword="true" />)
        /// or the destructor (<see langword="false" />).
        /// </param>
        /// <param name="isDisposed">
        /// The new value for <see cref="Server.IsDisposed" /> property.
        /// </param>
        protected virtual void OnDispose(bool disposing, ref bool isDisposed)
        {
            var isRunning = false;
            this.OnStop(disposing, ref isRunning);

            this.IsRunning = isRunning;
        }

        /// <summary>
        /// The logic for the <see cref="Server.Start()" /> method.
        /// </summary>
        /// <param name="isRunning">
        /// The new value for the <see cref="Server.IsRunning" /> property.
        /// </param>
        protected virtual void OnStart(ref bool isRunning)
        {
            var newListener = new TcpListener(this._SETTINGS.LocalAddress ?? IPAddress.Loopback,
                                              this._SETTINGS.Port);

            newListener.Start();

            this.BeginAcceptingTcpClient(newListener, true);

            this._listener = newListener;
        }

        /// <summary>
        /// The logic for the <see cref="Server.Stop()" /> method.
        /// </summary>
        /// <param name="disposing">
        /// <see cref="Server.Dispose()" /> method was invoked (<see langword="true" />)
        /// or the destructor (<see langword="false" />).
        /// <see langword="null" /> indicates that <see cref="Server.Stop()" /> method was invoked.
        /// </param>
        /// <param name="isRunning">
        /// The new value for the <see cref="Server.IsRunning" /> property.
        /// </param>
        protected virtual void OnStop(bool? disposing, ref bool isRunning)
        {
            using (this._listener.Server)
            {
                this._listener.Stop();
            }
        }

        /// <summary>
        /// Raises the error event.
        /// </summary>
        /// <param name="ex">The underlying exception.</param>
        /// <param name="rethrow">
        /// Rethrow after event was raised (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        /// <returns>
        /// Handler was raised (<see langword="true" />) or not <see langword="false" />.
        /// <see langword="null" /> indicates that <paramref name="ex" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="ServerException">
        /// Wrap version of <paramref name="ex" />.
        /// </exception>
        protected bool? RaiseError(Exception ex, bool rethrow = false)
        {
            if (ex == null)
            {
                return null;
            }

            var serverEx = (ex as ServerException) ?? new ServerException(ex);

            bool result;
            try
            {
                result = this.RaiseEventHandler(this.Error,
                                                new ServerErrorEventArgs(serverEx));
            }
            catch
            {
                result = false;
            }

            if (rethrow)
            {
                throw serverEx;
            }

            return result;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Server has been disposed.</exception>
        public void Start()
        {
            try
            {
                lock (this._SYNC)
                {
                    this.ThrowIfDisposed();

                    if (this.IsRunning)
                    {
                        return;
                    }

                    this.RaiseEventHandler(this.Starting);

                    var isRunning = true;
                    this.OnStart(ref isRunning);

                    this.IsRunning = isRunning;
                    if (this.IsRunning)
                    {
                        this.RaiseEventHandler(this.Started);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, true);
            }
        }

        /// <summary>
        /// Starts the communication with a remote client.
        /// </summary>
        /// <param name="client">The TCP client.</param>
        /// <param name="throwException">
        /// Throw exception (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        /// <returns>
        /// Operation was successful (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="client" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="ServerException">
        /// The raised exception.
        /// </exception>
        protected bool? StartCommunicationWithTcpClient(TcpClient client, bool throwException = true)
        {
            if (client == null)
            {
                return null;
            }

            try
            {
                var newHandler = new TcpClientConnectionHandler(this, client);

                Task.Factory
                    .StartNew(action: (state) =>
                                      {
                                          var handler = (TcpClientConnectionHandler)state;
                                          var server = handler.Server;

                                          try
                                          {
                                              using (handler)
                                              {
                                                  do
                                                  {
                                                      handler.HandleNext();
                                                  }
                                                  while (!handler.CloseConnection);
                                              }
                                          }
                                          catch (Exception ex)
                                          {
                                              server.RaiseError(ex, false);
                                          }
                                      },
                                      state: newHandler);

                return true;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, throwException);
                return false;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Server has been disposed.</exception>
        public void Stop()
        {
            try
            {
                lock (this._SYNC)
                {
                    this.ThrowIfDisposed();

                    if (!this.IsRunning)
                    {
                        return;
                    }

                    this.RaiseEventHandler(this.Stopping);

                    var isRunning = false;
                    this.OnStop(null, ref isRunning);

                    this.IsRunning = isRunning;
                    if (!this.IsRunning)
                    {
                        this.RaiseEventHandler(this.Stopped);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, true);
            }
        }

        /// <summary>
        /// Throws an exception if that object has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        #endregion Methods (16)
    }
}