using System;
using System.Collections.Generic;
using UNONE.Foundation.MemoryManagement;
using UNONE.Foundation.Utilities;

namespace UNONE.Foundation.EventSystems
{
    public partial class EventSystem
    {
        private static Pool<List<int>> s_IndexesPool;
        private static Pool<HashSet<int>> s_RegisterPool;

        public EventSystem()
        {
            if (s_IndexesPool == null)
            {
                s_IndexesPool = new Pool<List<int>>();
                s_RegisterPool = new Pool<HashSet<int>>();
            }

            _listenerMapping = new Dictionary<int, IEventListener>();
            _registerMapping = new Dictionary<int, HashSet<int>>();
            _dispatches = new Queue<IEventDispatch>(128);
        }

        private Dictionary<int, IEventListener> _listenerMapping;
        private Dictionary<int, HashSet<int>> _registerMapping;
        private Queue<IEventDispatch> _dispatches;

        public void Register<T>(object key, Action<T> callback)
        {
            var keyId = key.GetHashCode();

            var typeListener = GetEventListener<T>();
            typeListener.Add(keyId, callback);

            HashSet<int> register;
            if (!_registerMapping.TryGetValue(keyId, out register))
            {
                register = s_RegisterPool.Pop();
                _registerMapping[keyId] = register;
            }

            if (!register.Contains(typeListener.TypeIndex))
            {
                register.Add(typeListener.TypeIndex);
            }
        }

        public void Register<T>(object key, Action<T> callback, int tag)
        {
            var keyId = key.GetHashCode();

            var typeListener = GetEventListener<T>();
            typeListener.Add(keyId, callback, tag);

            HashSet<int> register;
            if (!_registerMapping.TryGetValue(keyId, out register))
            {
                register = s_RegisterPool.Pop();
                _registerMapping[keyId] = register;
            }

            if (!register.Contains(typeListener.TypeIndex))
            {
                register.Add(typeListener.TypeIndex);
            }
        }

        public void Unregister(object key)
        {
            var keyId = key.GetHashCode();
            HashSet<int> register;
            if (!_registerMapping.TryGetValue(keyId, out register))
                return;
            foreach (var typeIndex in register)
            {
                IEventListener listener;
                if (!_listenerMapping.TryGetValue(typeIndex, out listener))
                    continue;
                listener.Remove(keyId);
            }

            _registerMapping.Remove(keyId);

            register.Clear();
            s_RegisterPool.Push(register);
        }

        private EventListener<T> GetEventListener<T>()
        {
            EventListener<T> typeListener;
            var typeIndex = HashcodeCached<T>.TypeIndex;

            IEventListener listener;
            if (!_listenerMapping.TryGetValue(typeIndex, out listener))
            {
                typeListener = new EventListener<T>(typeIndex);
                _listenerMapping[typeIndex] = typeListener;
            }
            else
            {
                typeListener = listener as EventListener<T>;
            }

            return typeListener;
        }
    }
}
