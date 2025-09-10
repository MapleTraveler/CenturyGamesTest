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
            if ((keys.Count == 0 && values.Count == 0) && this.Count > 0)
            {
                keys.Clear();
                values.Clear();
                foreach (var kv in this)
                {
                    keys.Add(kv.Key);
                    values.Add(kv.Value);
                }
            }
        }

        // 从序列化列表恢复数据
        public void OnAfterDeserialize()
        {
            this.Clear();
            int count = Math.Min(keys.Count, values.Count);

            for (int i = 0; i < count; i++)
            {
                // 允许重复键，采用“最后一次出现覆盖前面”的策略
                this[keys[i]] = values[i];
            }

            if (keys.Count != values.Count)
            {
                Debug.LogError("[SerializableDictionary] 序列化数据不匹配，键和值的数量不同。");
            }
        }


        public List<TKey> FindDuplicateKeys()
        {
            var seen = new HashSet<TKey>();
            var dup = new HashSet<TKey>();
            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                if (!seen.Add(keys[i])) dup.Add(keys[i]);
            }

            return new List<TKey>(dup);
        }

        //（可选）一键修复：仅对 string/int/enum 等可安全自增的键类型启用
        public int MakeKeysUniqueIfPossible()
        {
            int fixedCount = 0;
            var used = new HashSet<TKey>();
            int count = Math.Min(keys.Count, values.Count);

            for (int i = 0; i < count; i++)
            {
                var k = keys[i];
                if (used.Add(k)) continue;

                // 仅示例：string 键添加后缀
                if (k is string s)
                {
                    string baseName = string.IsNullOrEmpty(s) ? "Key" : s;
                    string name = baseName;
                    int idx = 1;
                    while (!used.Add((TKey)(object)name)) name = $"{baseName} ({idx++})";
                    keys[i] = (TKey)(object)name;
                    fixedCount++;
                }
                // 其他类型可按需扩展（int 递增、enum 找空位等）
            }

            return fixedCount;
        }
    }

}