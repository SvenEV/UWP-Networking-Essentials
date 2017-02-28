using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace UwpNetworkingEssentials.MultiChannel
{
    public class MultiChannelConnectionListener : ConnectionListenerBase<IConnection>
    {
        private readonly ConnectionListenerDictionary _listeners = new ConnectionListenerDictionary();

        public ICollection<IConnectionListener> Listeners => _listeners;

        public MultiChannelConnectionListener()
        {
        }

        public MultiChannelConnectionListener(IEnumerable<IConnectionListener> listeners)
        {
            foreach (var listener in listeners)
                _listeners.Add(listener);
        }

        protected override Task DisposeCoreAsync()
        {
            _listeners.ConnectionReceived -= OnConnectionReceived;
            return Task.CompletedTask;
        }

        protected override Task StartCoreAsync()
        {
            _listeners.ConnectionReceived += OnConnectionReceived;
            return Task.CompletedTask;
        }

        private void OnConnectionReceived(IConnectionListener sender, IConnection connection)
        {
            _connectionReceived.OnNext(connection);
        }

        class ConnectionListenerDictionary : ICollection<IConnectionListener>
        {
            private readonly Dictionary<IConnectionListener, IDisposable> _listeners =
                new Dictionary<IConnectionListener, IDisposable>();

            internal event TypedEventHandler<IConnectionListener, IConnection> ConnectionReceived;

            public int Count => _listeners.Count;

            bool ICollection<IConnectionListener>.IsReadOnly => false;

            public void Add(IConnectionListener listener)
            {
                if (!_listeners.ContainsKey(listener))
                {
                    _listeners.Add(listener,
                        listener.ConnectionReceived.Subscribe(conn => ConnectionReceived?.Invoke(listener, conn)));
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

            void ICollection<IConnectionListener>.CopyTo(IConnectionListener[] array, int arrayIndex) =>
                _listeners.Keys.CopyTo(array, arrayIndex);

            public IEnumerator<IConnectionListener> GetEnumerator() => _listeners.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
