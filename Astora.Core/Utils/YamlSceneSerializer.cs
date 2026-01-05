using Astora.Core;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Astora.Core.Utils
{
    /// <summary>
    /// 序列化的节点数据 - 使用字典存储属性，更灵活
    /// </summary>
    public class SerializedNode
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object?>? Properties { get; set; }
        public List<SerializedNode>? Children { get; set; }
    }

    public class YamlSceneSerializer : ISceneSerializer
    {
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;
        
        // 节点类型注册表，支持扩展
        private static readonly Dictionary<string, Func<string, Node>> NodeFactories = new()
        {
            { nameof(Node), name => new Node(name) },
            { nameof(Node2D), name => new Node2D(name) },
            { nameof(Sprite), name => new Sprite(name, null) },
            { nameof(Camera2D), name => new Camera2D(name) }
        };

        // 需要特殊处理的属性（不直接序列化）
        private static readonly HashSet<string> IgnoredProperties = new()
        {
            nameof(Node.Parent),
            nameof(Node.Children),
            nameof(Node.IsQueuedForDeletion)
        };

        public YamlSceneSerializer()
        {
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        public string GetExtension() => ".scene";

        public void Save(Node rootNode, string path)
        {
            var serializedNode = SerializeNode(rootNode);
            var yaml = _serializer.Serialize(serializedNode);
            File.WriteAllText(path, yaml);
        }

        public Node Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"场景文件不存在: {path}");
            }

            var yaml = File.ReadAllText(path);
            var serializedNode = _deserializer.Deserialize<SerializedNode>(yaml);
            return DeserializeNode(serializedNode);
        }

        /// <summary>
        /// 序列化节点树 - 使用反射自动处理属性
        /// </summary>
        private SerializedNode SerializeNode(Node node)
        {
            var serialized = new SerializedNode
            {
                Type = node.GetType().Name,
                Name = node.Name,
                Properties = new Dictionary<string, object?>()
            };

            // 使用反射自动序列化所有公共属性
            var type = node.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !IgnoredProperties.Contains(p.Name));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(node);
                if (value == null) continue;

                // 处理特殊类型
                var serializedValue = SerializeProperty(prop.PropertyType, value);
                if (serializedValue != null)
                {
                    serialized.Properties[prop.Name] = serializedValue;
                }
            }

            // 序列化子节点
            if (node.Children.Count > 0)
            {
                serialized.Children = node.Children.Select(SerializeNode).ToList();
            }

            return serialized;
        }

        /// <summary>
        /// 序列化属性值，处理特殊类型
        /// </summary>
        private object? SerializeProperty(Type propertyType, object value)
        {
            // Vector2 -> 字典
            if (propertyType == typeof(Vector2))
            {
                var vec = (Vector2)value;
                return new Dictionary<string, float> { { "x", vec.X }, { "y", vec.Y } };
            }

            // Color -> 字典
            if (propertyType == typeof(Color))
            {
                var color = (Color)value;
                return new Dictionary<string, int>
                {
                    { "r", color.R },
                    { "g", color.G },
                    { "b", color.B },
                    { "a", color.A }
                };
            }

            // Texture2D -> 路径字符串（需要外部资源管理器支持）
            if (propertyType == typeof(Texture2D))
            {
                // 这里可以扩展为从资源管理器获取路径
                // 暂时返回 null，表示不序列化纹理对象本身
                return null;
            }

            // 基本类型直接返回
            if (propertyType.IsPrimitive || 
                propertyType == typeof(string) || 
                propertyType == typeof(float) || 
                propertyType == typeof(double) || 
                propertyType == typeof(int) || 
                propertyType == typeof(bool))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// 反序列化节点树
        /// </summary>
        private Node DeserializeNode(SerializedNode serialized)
        {
            // 创建节点实例
            Node node;
            
            if (NodeFactories.TryGetValue(serialized.Type, out var factory))
            {
                // 使用注册的工厂方法
                node = factory(serialized.Name);
            }
            else
            {
                // 尝试通过反射动态查找类型
                node = CreateNodeByReflection(serialized.Type, serialized.Name);
            }

            // 使用反射恢复属性
            if (serialized.Properties != null)
            {
                var type = node.GetType();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && !IgnoredProperties.Contains(p.Name));

                foreach (var prop in properties)
                {
                    if (!serialized.Properties.TryGetValue(prop.Name, out var value) || value == null)
                        continue;

                    try
                    {
                        var deserializedValue = DeserializeProperty(prop.PropertyType, value, prop.Name);
                        if (deserializedValue != null)
                        {
                            prop.SetValue(node, deserializedValue);
                        }
                    }
                    catch
                    {
                        // 忽略无法反序列化的属性
                    }
                }
            }

            // 恢复子节点
            if (serialized.Children != null)
            {
                foreach (var childSerialized in serialized.Children)
                {
                    var child = DeserializeNode(childSerialized);
                    node.AddChild(child);
                }
            }

            return node;
        }

        /// <summary>
        /// 从字典中获取值（支持不同的字典类型）
        /// </summary>
        private T? GetDictValue<T>(IDictionary dict, string key) where T : struct
        {
            // 尝试不同的键格式
            var keys = new List<string> { key, key.ToLower(), key.ToUpper() };
            if (key.Length > 0)
            {
                keys.Add(char.ToUpper(key[0]) + (key.Length > 1 ? key.Substring(1) : ""));
            }
            
            foreach (var k in keys)
            {
                if (dict.Contains(k))
                {
                    var val = dict[k];
                    if (val != null)
                    {
                        try
                        {
                            return (T)Convert.ChangeType(val, typeof(T));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 反序列化属性值，处理特殊类型
        /// </summary>
        private object? DeserializeProperty(Type propertyType, object value, string propertyName)
        {
            // Vector2
            if (propertyType == typeof(Vector2) && value is IDictionary dict)
            {
                var x = GetDictValue<float>(dict, "x") ?? 0f;
                var y = GetDictValue<float>(dict, "y") ?? 0f;
                return new Vector2(x, y);
            }

            // Color
            if (propertyType == typeof(Color) && value is IDictionary colorDict)
            {
                var r = GetDictValue<int>(colorDict, "r") ?? 255;
                var g = GetDictValue<int>(colorDict, "g") ?? 255;
                var b = GetDictValue<int>(colorDict, "b") ?? 255;
                var a = GetDictValue<int>(colorDict, "a") ?? 255;
                return new Color(r, g, b, a);
            }

            // Texture2D - 从路径加载
            if (propertyType == typeof(Texture2D) && value is string texturePath)
            {
                if (Engine.Content != null && !string.IsNullOrEmpty(texturePath))
                {
                    try
                    {
                        return Engine.Content.Load<Texture2D>(texturePath);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }

            // 基本类型转换
            if (propertyType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // 尝试类型转换
            try
            {
                return Convert.ChangeType(value, propertyType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过反射动态创建节点实例
        /// </summary>
        private Node CreateNodeByReflection(string typeName, string nodeName)
        {
            // 从所有已加载的程序集中查找类型
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var type = assembly.GetType(typeName, false, true);
                    if (type == null)
                    {
                        // 尝试在命名空间中查找（处理完全限定名）
                        type = assembly.GetTypes()
                            .FirstOrDefault(t => t.Name == typeName && typeof(Node).IsAssignableFrom(t));
                    }
                    
                    if (type != null && typeof(Node).IsAssignableFrom(type))
                    {
                        // 尝试使用带 name 参数的构造函数
                        var nameConstructor = type.GetConstructor(new[] { typeof(string) });
                        if (nameConstructor != null)
                        {
                            return (Node)nameConstructor.Invoke(new object[] { nodeName });
                        }
                        
                        // 尝试使用无参构造函数，然后设置 Name 属性
                        var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
                        if (parameterlessConstructor != null)
                        {
                            var instance = (Node)parameterlessConstructor.Invoke(null);
                            instance.Name = nodeName;
                            return instance;
                        }
                        
                        throw new NotSupportedException($"类型 {typeName} 没有找到合适的构造函数（需要无参构造函数或带 string 参数的构造函数）");
                    }
                }
                catch (Exception ex) when (ex is not NotSupportedException)
                {
                    // 忽略程序集加载错误，继续查找
                    continue;
                }
            }
            
            throw new NotSupportedException($"无法找到节点类型: {typeName}。请确保类型已加载并且继承自 Node。");
        }

        /// <summary>
        /// 注册自定义节点类型（用于扩展）
        /// </summary>
        public static void RegisterNodeType(string typeName, Func<string, Node> factory)
        {
            NodeFactories[typeName] = factory;
        }
    }
}
