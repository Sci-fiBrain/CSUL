using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSUL.Models
{
    /// <summary>
    /// 配置文件执行器类
    /// </summary>
    public static class ConfigActivator
    {
        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="obj">要保存的对象</param>
        /// <param name="configPath">配置文件路径</param>
        /// <param name="incPrivate">是否包含私有成员</param>
        public static void SaveConfig(this object obj, string configPath, bool incPrivate = false) => Save(obj, configPath, incPrivate);

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="obj">要保存的对象</param>
        /// <param name="dir">配置文件所处文件夹</param>
        /// <param name="configName">配置文件名称</param>
        /// <param name="incPrivate">是否包含私有成员</param>
        public static void SaveConfig(this object obj, DirectoryInfo dir, string configName, bool incPrivate = false)
            => Save(obj, Path.Combine(dir.FullName, configName), incPrivate);

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="obj">要保存的对象</param>
        /// <param name="configPath">配置文件路径</param>
        /// <param name="incPrivate">是否包含私有成员</param>
        public static void LoadConfig(this object obj, string configPath, bool incPrivate = false) => Load(obj, configPath, incPrivate);

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="obj">要保存的对象</param>
        /// <param name="dir">配置文件所处文件夹</param>
        /// <param name="configName">配置文件名称</param>
        /// <param name="incPrivate">是否包含私有成员</param>
        public static void LoadConfig(this object obj, DirectoryInfo dir, string configName, bool incPrivate = false)
            => Load(obj, Path.Combine(dir.FullName, configName), incPrivate);

        #region ---私有方法---

        private static readonly JsonSerializerOptions options = new() { Converters = { new DirectoryInfoConverter() } };

        private static void Save(object obj, string configPath, bool incPrivate = false)
        {   //保存配置文件
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (incPrivate) flags |= BindingFlags.NonPublic;
            var properties = from pro in obj.GetType().GetProperties(flags) where Attribute.IsDefined(pro, typeof(ConfigAttribute)) select pro;
            using Utf8JsonWriter writer = new(new FileStream(configPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite), new() { Indented = true });
            writer.WriteStartObject();
            foreach (var pro in properties)
            {
                writer.WriteStartObject(pro.Name);
                writer.WriteString("Type", pro.PropertyType.AssemblyQualifiedName);
                writer.WriteString("Value", JsonSerializer.Serialize(pro.GetValue(obj), pro.PropertyType, options));
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        private static void Load(object obj, string configPath, bool incPrivate = false)
        {   //加载配置文件
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (incPrivate) flags |= BindingFlags.NonPublic;
            var properties = (from pro in obj.GetType().GetProperties(flags) where Attribute.IsDefined(pro, typeof(ConfigAttribute)) select pro).ToDictionary(x => x.Name);
            using JsonDocument jsonFile = JsonDocument.Parse(new FileStream(configPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            JsonElement.ObjectEnumerator jsons = jsonFile.RootElement.EnumerateObject();
            foreach (JsonProperty json in jsons)
            {
                JsonElement.ObjectEnumerator content = json.Value.EnumerateObject();
                if (properties.TryGetValue(json.Name, out var value))
                {
                    content.MoveNext();
                    Type type = Type.GetType(content.Current.Value.GetString()!)!;
                    content.MoveNext();
                    object? proValue = JsonSerializer.Deserialize(content.Current.Value.GetString()!, type, options);
                    value.SetValue(obj, proValue);
                }
            }
        }

        #endregion ---私有方法---
    }

    /// <summary>
    /// 标记指定属性需要被保存在配置文件中
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConfigAttribute : Attribute
    { }

    public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string read = reader.GetString()!;
            return new(read);
        }

        public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
        {
            string path = value.FullName;
            writer.WriteStringValue(path);
        }
    }
}