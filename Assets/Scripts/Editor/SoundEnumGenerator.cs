using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AD.Editor
{
    public static class SoundEnumGenerator
    {
        private const string InputCsvPath = "Resources/Table/SoundTable.csv";
        private const string OutputScriptFolder = "Assets/Scripts/Data";

        [MenuItem("Tools/Sound/Generate SoundType Enums")]
        public static void GenerateEnums()
        {
            string fullPath = Path.Combine(Application.dataPath, InputCsvPath);
            if (!File.Exists(fullPath))
            {
                DebugLogger.LogError($"CSV 파일이 존재하지 않습니다: {fullPath}");
                return;
            }

            var bgmNames = new List<string>();
            var sfxNames = new List<string>();

            using (var reader = new StreamReader(fullPath))
            {
                bool skipHeader = true;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (skipHeader) { skipHeader = false; continue; }
                    string[] parts = line.Split(',');
                    if (parts.Length < 2) continue;

                    string type = parts[0].Trim();
                    string name = parts[1].Trim();

                    if (string.IsNullOrEmpty(name)) continue;

                    if (type.Equals("BGM", StringComparison.OrdinalIgnoreCase) && !bgmNames.Contains(name))
                        bgmNames.Add(name);
                    else if (type.Equals("SFX", StringComparison.OrdinalIgnoreCase) && !sfxNames.Contains(name))
                        sfxNames.Add(name);
                }
            }

            WriteEnumScript("BGMType", bgmNames);
            WriteEnumScript("SFXType", sfxNames);

            AssetDatabase.Refresh();
        }

        private static void WriteEnumScript(string enumName, IEnumerable<string> values)
        {
            string filePath = Path.Combine(OutputScriptFolder, enumName + ".cs");
            Directory.CreateDirectory(OutputScriptFolder);

            using (var writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("namespace AD");
                writer.WriteLine("{");
                writer.WriteLine($"    public enum {enumName}");
                writer.WriteLine("    {");

                foreach (var name in values)
                {
                    writer.WriteLine($"        {name},");
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }
    }
}
