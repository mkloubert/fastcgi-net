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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.FastCGI
{
    /// <summary>
    /// A FastCGI server.
    /// </summary>
    public partial class Server : IDisposable
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
        /// An unique object for thread safe operations.
        /// </summary>
        protected readonly object _SYNC = new object();

        #endregion Fields (3)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of <see cref="Server" /> class.
        /// </summary>
        /// <param name="settings">The custom settings.</param>
        public Server(ISettings settings = null)
        {
            this._SETTINGS = settings ?? new Settings();
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

        #region Properties (1)

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

        #endregion Properties (1)

        #region Methods (14)

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
            try
            {
                var newListener = new TcpListener(this._SETTINGS.LocalAddress ?? IPAddress.Any,
                                                  this._SETTINGS.Port);
                newListener.Start();

                this.BeginAcceptingTcpClient(newListener, true);

                this._listener = newListener;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, true);
            }
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

            var serverEx = ex as ServerException;
            if (serverEx == null)
            {
                serverEx = new ServerException(ex);
            }

            var result = this.RaiseEventHandler(this.Error,
                                                new ServerErrorEventArgs(serverEx));

            if (rethrow)
            {
                throw serverEx;
            }

            return result;
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <param name="handler">The handler to raise.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        protected bool RaiseEventHandler(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <typeparam name="TArgs">Type of the event arguments.</typeparam>
        /// <param name="handler">The handler to raise.</param>
        /// <param name="e">The arguments for the event.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseEventHandler<TArgs>(EventHandler<TArgs> handler, TArgs e)
            where TArgs : global::System.EventArgs
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (handler != null)
            {
                handler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Server has been disposed.</exception>
        public void Start()
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
                                          using (var handler = (TcpClientConnectionHandler)state)
                                          {
                                              do
                                              {
                                                  try
                                                  {
                                                      handler.HandleNext();
                                                  }
                                                  catch (Exception ex)
                                                  {
                                                      handler.Server.RaiseError(ex);
                                                  }
                                              }
                                              while (!handler.CloseConnection);
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

        #endregion Methods (14)
    }
}