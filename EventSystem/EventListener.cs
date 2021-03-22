using System;
using System.Collections.Generic;
using UnityEngine;
using UNONE.Foundation.MemoryManagement;

namespace UNONE.Foundation.EventSystems
{
    public partial class EventSystem
    {
        private interface IEventListener
        {
            void Remove(int keyId);
        }

        private class EventListener<T> : IEventListener
        {
            private static Pool<ListedDictionary<int, Action<T>>> s_ActionsPool;

            public EventListener(int typeIndex)
            {
                TypeIndex = typeIndex;
                s_ActionsPool = new Pool<ListedDictionary<int, Action<T>>>();
                _keyIdToAction = new ListedDictionary<int, Action<T>>();
                _tagToMapping = new Dictionary<int, ListedDictionary<int, Action<T>>>();
                _keyIdToTags = new Dictionary<int, List<int>>();
            }

            private ListedDictionary<int, Action<T>> _keyIdToAction;
            private Dictionary<int, ListedDictionary<int, Action<T>>> _tagToMapping;
            private Dictionary<int, List<int>> _keyIdToTags;

            public int TypeIndex { get; private set; }

            public void Add(int keyId, Action<T> action)
            {
                _keyIdToAction.Add(keyId, action);
            }

            public void Add(int keyId, Action<T> action, int tag)
            {
                ListedDictionary<int, Action<T>> actions;
                if (!_tagToMapping.TryGetValue(tag, out actions))
                {
                    actions = s_ActionsPool.Pop();
                    _tagToMapping[tag] = actions;
                }

                actions.Add(keyId, action);

                List<int> tags;
                if (!_keyIdToTags.TryGetValue(keyId, out tags))
                {
                    tags = s_IndexesPool.Pop();
                    _keyIdToTags[keyId] = tags;
                }

                tags.Add(tag);
            }

            public void Remove(int keyId)
            {
                if (_keyIdToAction.ContainsKey(keyId))
                    _keyIdToAction.Remove(keyId);

                List<int> tags;
                if (_keyIdToTags.TryGetValue(keyId, out tags))
                {
                    for (int i = tags.Count - 1; i >= 0; i--)
                    {
                        var mapping = _tagToMapping[tags[i]];
                        mapping.Remove(keyId);

                        if (mapping.Count != 0)
                            continue;
                        _keyIdToTags.Remove(keyId);

                        s_ActionsPool.Push(mapping);
                    }

                    tags.Clear();
                    s_IndexesPool.Push(tags);
                }
            }

            public void Send(T data)
            {
                var list = _keyIdToAction.List;
                for (int i = list.Count - 1; i >= 0; i--)
                    list[i](data);
            }

            public void Send(T data, int tag)
            {
                ListedDictionary<int, Action<T>> actions;
                if (!_tagToMapping.TryGetValue(tag, out actions))
                    return;
                var list = actions.List;

                for (int i = list.Count - 1; i >= 0; i--)
                    list[i](data);
            }
        }

        private class ListedDictionary<K, V>
        {
            public ListedDictionary()
            {
                _dict = new Dictionary<K, V>();
                _list = new List<V>();
            }

            private Dictionary<K, V> _dict;
            private List<V> _list;

            public int Count => _dict.Count;

            public IReadOnlyList<V> List => _list;

            public void Add(K key, V value)
            {
                V prev;
                if(_dict.TryGetValue(key, out prev))
                    _list.Remove(prev);
                _dict[key] = value;
                _list.Add(value);
            }

            public void Remove(K key)
            {
                V value;
                if (!_dict.TryGetValue(key, out value))
                    return;
                _list.Remove(value);
                _dict.Remove(key);
            }

            public bool ContainsKey(K key)
            {
                return _dict.ContainsKey(key);
            }

            public bool TryGetValue(K key, out V value)
            {
                return _dict.TryGetValue(key, out value);
            }
        }
    }
}
