using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Foundation;

namespace UwpNetworkingEssentials.MultiChannel
{
    public class MultiChannelConnectionListener : IConnectionListener
    {
        private readonly Subject<IConnection> _connectionReceived = new Subject<IConnection>();
        private readonly ConnectionListenerDictionary _listeners = new ConnectionListenerDictionary();
        private bool _isRunning = false;

        public IObservable<IConnection> ConnectionReceived => _connectionReceived;

        public ICollection<IConnectionListener> Listeners => _listeners;

        public MultiChannelConnectionListener()
        {
            _listeners.ConnectionReceived += (listener, connection) =>
            {
                // only emit event if multi-channel listener is started too
                if (_isRunning)
                    _connectionReceived.OnNext(connection);
            };
        }

        public MultiChannelConnectionListener(IEnumerable<IConnectionListener> listeners) : this()
        {
            foreach (var listener in listeners)
                _listeners.Add(listener);
        }

        public Task DisposeAsync()
        {
            _isRunning = false;
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            _isRunning = true;
            return Task.CompletedTask;
        }
    }

    public class ConnectionListenerDictionary : ICollection<IConnectionListener>
    {
        private readonly Dictionary<IConnectionListener, IDisposable> _listeners = new Dictionary<IConnectionListener, IDisposable>();

        internal TypedEventHandler<IConnectionListener, IConnection> ConnectionReceived;

        public int Count => _listeners.Count;

        bool ICollection<IConnectionListener>.IsReadOnly => false;

        public void Add(IConnectionListener listener)
        {
            if (!_listeners.ContainsKey(listener))
            {
                _listeners.Add(listener, listener.ConnectionReceived.Subscribe(conn => ConnectionReceived?.Invoke(listener, conn)));
            }
        }

        public bool Remove(IConnectionListener listener)
        {
            if (_listeners.TryGetValue(listener, out var subscription))
            {
                subscription.Dispose();
                _listeners.Remove(listener);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (var subscription in _listeners.Values)
                subscription.Dispose();
            _listeners.Clear();
        }

        public bool Contains(IConnectionListener item) => _listeners.ContainsKey(item);

        void ICollection<IConnectionListener>.CopyTo(IConnectionListener[] array, int arrayIndex) => _listeners.Keys.CopyTo(array, arrayIndex);

        public IEnumerator<IConnectionListener> GetEnumerator() => _listeners.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
