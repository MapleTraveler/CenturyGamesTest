using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        // 存储数据到序列化列表
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        // 从序列化列表恢复数据
        public void OnAfterDeserialize()
        {
            this.Clear();
            int count = Math.Min(keys.Count, values.Count);

            for (int i = 0; i < count; i++)
            {
                if (!this.ContainsKey(keys[i])) // 避免重复键异常
                {
                    this[keys[i]] = values[i];
                }
                else
                {
                    Debug.LogError($"[SerializableDictionary] 检测到重复键: {keys[i]}，该条目已被跳过。");
                }
            }

            if (keys.Count != values.Count)
            {
                Debug.LogError("[SerializableDictionary] 序列化数据不匹配，键和值的数量不同。");
            }
        }
    }
}