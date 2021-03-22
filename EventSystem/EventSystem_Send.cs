using UNONE.Foundation.Utilities;

namespace UNONE.Foundation.EventSystems
{
    public partial class EventSystem
    {
        public void Send<T>(T data, bool sendNow = false)
        {
            var typeIndex = HashcodeCached<T>.TypeIndex;
            IEventListener listener;
            if (!_listenerMapping.TryGetValue(typeIndex, out listener))
                return;

            var typeListener = listener as EventListener<T>;
            if (sendNow)
            {
                typeListener.Send(data);
            }
            else
            {
                var dispatch = new EventDispatch<T>(typeListener, data);
                _dispatches.Enqueue(dispatch);
            }
        }

        public void Send<T>(T data, int tag, bool sendNow = false)
        {
            var typeIndex = HashcodeCached<T>.TypeIndex;
            IEventListener listener;
            if (!_listenerMapping.TryGetValue(typeIndex, out listener))
                return;

            var typeListener = listener as EventListener<T>;
            if (sendNow)
            {
                typeListener.Send(data, tag);
            }
            else
            {
                var dispatch = new EventDispatch<T>(typeListener, data, tag);
                _dispatches.Enqueue(dispatch);
            }
        }

        public void Update()
        {
            while(_dispatches.Count != 0)
            {
                var dispatch = _dispatches.Dequeue();
                dispatch.Dispatch();
            }
        }

        private interface IEventDispatch
        {
            void Dispatch();
        }

        private struct EventDispatch<T> : IEventDispatch
        {
            public EventDispatch(EventListener<T> listener, T data)
            {
                _listener = listener;
                _data = data;
                _hasTag = false;
                _tag = 0;
            }

            public EventDispatch(EventListener<T> listener, T data, int tag)
            {
                _listener = listener;
                _data = data;
                _hasTag = true;
                _tag = tag;
            }

            private EventListener<T> _listener;
            private T _data;
            private bool _hasTag;
            private int _tag;

            public void Dispatch()
            {
                if (_hasTag)
                {
                    _listener.Send(_data, _tag);
                }
                else
                {
                    _listener.Send(_data);
                }
            }
        }
    }
}
