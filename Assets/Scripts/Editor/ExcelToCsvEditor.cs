using System.IO;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using System.Text;
using System.Data;

public class ExcelToCsvEditor : EditorWindow
{
    [MenuItem("Tools/Convert Excel to CSV")]
    public static void ConvertExcelToCsv()
    {
        string excelDir = "Assets/TableExcel";
        string outputDir =  "Assets/Resources/Table";

        if (!Directory.Exists(excelDir))
        {
            Debug.LogWarning("Excel directory not found: " + excelDir);
            return;
        }

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var files = Directory.GetFiles(excelDir, "*.xlsx");
        foreach (var filePath in files)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0];

                string csvPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath) + ".csv");
                using (var writer = new StreamWriter(csvPath))
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        string[] row = new string[table.Columns.Count];
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            row[j] = table.Rows[i][j].ToString();
                        }
                        writer.WriteLine(string.Join(",", row));
                    }
                }
                Debug.Log("Converted: " + filePath + " → " + csvPath);
            }
        }
        AssetDatabase.Refresh();
    }
}