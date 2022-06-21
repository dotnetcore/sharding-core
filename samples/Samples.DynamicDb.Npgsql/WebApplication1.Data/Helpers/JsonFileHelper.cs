using System.IO;
using System.Text;
using System.Text.Json;

namespace WebApplication1.Data.Helpers
{

    public class JsonFileHelper
    {

        /// <summary>
        /// 序列化选项
        /// </summary>
        private static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true }; // 序列化选项

        /// <summary>
        /// 将数据存为Json字符串文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="data">数据</param>
        public static void Save<T>(string path, string fileName, T data)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var fullPath = Path.Combine(path, fileName.EndsWith(".json") ? fileName : $"{fileName}.json");

            string json = JsonSerializer.Serialize(data ?? default, serializerOptions);
            File.WriteAllText(fullPath, json, Encoding.UTF8);
        }

        /// <summary>
        /// 读取Json文件，返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="fileName">文件名</param>
        /// <returns>返回的数据</returns>
        public static T Read<T>(string path, string fileName)
        {
            var fullPath = Path.Combine(path, fileName.EndsWith(".json") ? fileName : $"{fileName}.json");
            T data = default;
            if (File.Exists(fullPath))
            {
                var jsonString = File.ReadAllText(fullPath, Encoding.UTF8);
                if (!string.IsNullOrEmpty(jsonString)) data = JsonSerializer.Deserialize<T>(jsonString);
            }
            return data;
        }

    }

}
