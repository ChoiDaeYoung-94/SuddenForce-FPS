using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AD
{
    public class TableManager : ISubManager
    {
        private Dictionary<Type, object> _tableDataMap = new();

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
            var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                DebugLogger.LogError($"TableManager - CSV 행이 너무 적습니다: {typeof(T).Name}");
                return null;
            }

            string[] headers = lines[0].Split(',');
            var list = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                T obj = new T();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    FieldInfo field = typeof(T).GetField(headers[j], BindingFlags.Public | BindingFlags.Instance);
                    if (field == null) continue;
                    
                    object convertedValue;
                    if (field.FieldType.IsEnum)
                    {
                        convertedValue = Enum.Parse(field.FieldType, values[j]);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(values[j], field.FieldType);
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
        
        #endregion
    }
}