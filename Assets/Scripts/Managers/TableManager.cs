using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AD
{
    public class TableManager : ISubManager
    {
        private Dictionary<Type, object> _tableDataMap = new();
        private readonly Dictionary<Type, Dictionary<string, ITableData>> _keyIndex =
            new Dictionary<Type, Dictionary<string, ITableData>>();

        public async UniTask InitAsync()
        {
            TextAsset[] csvFiles =
                Resources.LoadAll<TextAsset>(GameConstants.GetPath(GameConstants.ResourceCategory.Table));

            foreach (var file in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.name);

                Type dataType = Type.GetType($"AD.GameData.{fileName}Data");
                if (dataType == null)
                {
                    DebugLogger.LogError($"TableManager - 타입을 찾을 수 없습니다: {fileName}");
                    continue;
                }

                object parsedData = InvokeParseMethod(dataType, file.text);
                if (parsedData != null)
                {
                    _tableDataMap[dataType] = parsedData;

                    // ITableData면 Key 인덱스 캐싱
                    if (typeof(ITableData).IsAssignableFrom(dataType))
                    {
                        var list = parsedData as System.Collections.IEnumerable;
                        var dict = new Dictionary<string, ITableData>();
                        foreach (var row in list)
                        {
                            if (row is ITableData td)
                            {
                                var key = td.GetKey();
                                if (!string.IsNullOrEmpty(key))
                                {
                                    dict[key] = td;
                                }
                            }
                        }
                        _keyIndex[dataType] = dict;
                    }
                }
            }
            await UniTask.Yield();
        }

        #region Init

        private object InvokeParseMethod(Type dataType, string csvText)
        {
            MethodInfo method = typeof(TableManager)
                .GetMethod(nameof(ParseCsv), BindingFlags.NonPublic | BindingFlags.Instance)
                ?.MakeGenericMethod(dataType);

            return method?.Invoke(this, new object[] { csvText });
        }

        private List<T> ParseCsv<T>(string csvText) where T : class, new()
        {
            var lines = csvText
                .Replace("\r\n", "\n")
                .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var usable = new List<string>(capacity: lines.Length);
            foreach (var l in lines)
            {
                var line = l.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue; // 주석 라인 무시
                usable.Add(line);
            }

            if (usable.Count < 2)
            {
                DebugLogger.LogError($"TableManager - CSV 행이 너무 적습니다: {typeof(T).Name}");
                return null;
            }

            string[] headers = usable[0].Split(',').Select(h => h.Trim()).ToArray();
            var list = new List<T>();

            for (int i = 1; i < usable.Count; i++)
            {
                string[] values = usable[i].Split(',');
                T obj = new T();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    string header = headers[j];
                    string raw = values[j].Trim();

                    FieldInfo field = typeof(T).GetField(header, BindingFlags.Public | BindingFlags.Instance);
                    if (field == null) continue;

                    object convertedValue;
                    if (field.FieldType.IsEnum)
                    {
                        convertedValue = Enum.Parse(field.FieldType, raw);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(raw, field.FieldType);
                    }
                    field.SetValue(obj, convertedValue);
                }
                list.Add(obj);
            }

            return list;
        }

        #endregion

        public void Release()
        {
            _tableDataMap.Clear();
            _keyIndex.Clear();
        }

        #region Functions

        public List<T> GetTable<T>() where T : class
        {
            if (_tableDataMap.TryGetValue(typeof(T), out var tableData))
            {
                return tableData as List<T>;
            }

            DebugLogger.LogError($"TableManager - {typeof(T)} 테이블이 없습니다.");
            return null;
        }

        public bool TryGetTable<T>(out List<T> table) where T : class
        {
            if (_tableDataMap.TryGetValue(typeof(T), out var tableData))
            {
                table = tableData as List<T>;
                return table != null;
            }
            table = null;
            return false;
        }

        public T GetByKey<T>(string key) where T : class, ITableData
        {
            if (_keyIndex.TryGetValue(typeof(T), out var dict))
            {
                if (dict.TryGetValue(key, out var row))
                {
                    return row as T;
                }
            }
            return null;
        }

        public Dictionary<string, T> GetTableAsDictionary<T>() where T : class, ITableData
        {
            if (_keyIndex.TryGetValue(typeof(T), out var dict))
            {
                // 복사본 반환(외부 변조 방지)
                var ret = new Dictionary<string, T>(dict.Count);
                foreach (var kv in dict)
                {
                    ret[kv.Key] = kv.Value as T;
                }
                return ret;
            }

            // 인덱스가 없다면 테이블에서 즉시 생성(최초 1회)
            var list = GetTable<T>();
            if (list == null)
            {
                return new Dictionary<string, T>();
            }
            var newDict = new Dictionary<string, T>(list.Count);
            foreach (var row in list)
            {
                newDict[row.GetKey()] = row;
            }
            _keyIndex[typeof(T)] = newDict.ToDictionary(k => k.Key, v => (ITableData)v.Value);
            return newDict;
        }

        /// <summary>
        /// 호출 시 넘긴 predicate(조건식)에 맞는 모든 요소를 찾아 새로운 List<T>로 반환
        /// </summary>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> FindAll<T>(Predicate<T> predicate) where T : class
        {
            var list = GetTable<T>();
            if (list == null) return new List<T>();
            return list.FindAll(predicate);
        }

        #endregion
        
    }
}
