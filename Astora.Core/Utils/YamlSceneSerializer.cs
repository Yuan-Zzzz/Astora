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
    /// Serialized representation of a Node for YAML serialization
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

        private static readonly Dictionary<string, Func<string, Node>> NodeFactories = new()
        {
            { nameof(Node), name => new Node(name) },
            { nameof(Node2D), name => new Node2D(name) },
            { nameof(Sprite), name => new Sprite(name, null) },
            { nameof(Camera2D), name => new Camera2D(name) }
        };

        private static readonly HashSet<string> IgnoredFields = new()
        {
            "_parent",
            "_children",
            "_isQueuedForDeletion"
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
                throw new FileNotFoundException($"There is no scene file at path: {path}");
            }

            var yaml = File.ReadAllText(path);
            var serializedNode = _deserializer.Deserialize<SerializedNode>(yaml);
            return DeserializeNode(serializedNode);
        }

        private SerializedNode SerializeNode(Node node)
        {
            var serialized = new SerializedNode
            {
                Type = node.GetType().Name,
                Name = node.Name,
                Properties = new Dictionary<string, object?>()
            };

            var type = node.GetType();
            // 获取所有实例字段（包括私有和公共）
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !IgnoredFields.Contains(f.Name));

            foreach (var field in fields)
            {
                // 只序列化标记了 [SerializeField] 的字段
                if (!field.IsDefined(typeof(SerializeFieldAttribute), false))
                    continue;

                var value = field.GetValue(node);
                if (value == null) continue;

                var serializedValue = SerializeField(field.FieldType, value);
                if (serializedValue != null)
                {
                    serialized.Properties[field.Name] = serializedValue;
                }
            }

            if (node.Children.Count > 0)
            {
                serialized.Children = node.Children.Select(SerializeNode).ToList();
            }

            return serialized;
        }

        private object? SerializeField(Type fieldType, object value)
        {
            if (fieldType == typeof(Vector2))
            {
                var vec = (Vector2)value;
                return new Dictionary<string, float> { { "x", vec.X }, { "y", vec.Y } };
            }

            if (fieldType == typeof(Color))
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

            if (fieldType == typeof(Texture2D))
            {
                return null;
            }

            if (fieldType.IsPrimitive ||
                fieldType == typeof(string) ||
                fieldType == typeof(float) ||
                fieldType == typeof(double) ||
                fieldType == typeof(int) ||
                fieldType == typeof(bool))
            {
                return value;
            }

            return null;
        }

        private Node DeserializeNode(SerializedNode serialized)
        {
            Node node;

            // 首先尝试使用工厂字典
            if (NodeFactories.TryGetValue(serialized.Type, out var factory))
            {
                node = factory(serialized.Name);
            }
            // 然后尝试使用 NodeTypeRegistry（如果已设置）
            else if ((node = TryCreateNodeWithRegistry(serialized.Type, serialized.Name)) != null)
            {
                // 节点已通过注册表创建
            }
            // 最后回退到反射创建
            else
            {
                node = CreateNodeByReflection(serialized.Type, serialized.Name);
            }

            if (serialized.Properties != null)
            {
                var type = node.GetType();
                // 获取所有实例字段（包括私有和公共）
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !IgnoredFields.Contains(f.Name));

                foreach (var field in fields)
                {
                    // 只反序列化标记了 [SerializeField] 的字段
                    if (!field.IsDefined(typeof(SerializeFieldAttribute), false))
                        continue;

                    if (!serialized.Properties.TryGetValue(field.Name, out var value) || value == null)
                        continue;

                    try
                    {
                        var deserializedValue = DeserializeField(field.FieldType, value, field.Name);
                        if (deserializedValue != null)
                        {
                            field.SetValue(node, deserializedValue);
                        }
                    }
                    catch
                    {
                    }
                }
            }

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

        private object? DeserializeField(Type fieldType, object value, string fieldName)
        {
            // Vector2
            if (fieldType == typeof(Vector2) && value is IDictionary dict)
            {
                var x = GetDictValue<float>(dict, "x") ?? 0f;
                var y = GetDictValue<float>(dict, "y") ?? 0f;
                return new Vector2(x, y);
            }

            if (fieldType == typeof(Color) && value is IDictionary colorDict)
            {
                var r = GetDictValue<int>(colorDict, "r") ?? 255;
                var g = GetDictValue<int>(colorDict, "g") ?? 255;
                var b = GetDictValue<int>(colorDict, "b") ?? 255;
                var a = GetDictValue<int>(colorDict, "a") ?? 255;
                return new Color(r, g, b, a);
            }

            if (fieldType == typeof(Texture2D) && value is string texturePath)
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

            if (fieldType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            try
            {
                return Convert.ChangeType(value, fieldType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过反射动态创建节点实例
        /// </summary>
        private static Assembly? _priorityAssembly; // 优先查找的程序集
        
        /// <summary>
        /// 设置优先查找的程序集（通常是项目程序集）
        /// </summary>
        public static void SetPriorityAssembly(Assembly? assembly)
        {
            _priorityAssembly = assembly;
        }
        
        private Node CreateNodeByReflection(string typeName, string nodeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var searchedAssemblies = new List<string>();

            // 优先从项目程序集中查找
            if (_priorityAssembly != null)
            {
                try
                {
                    var type = _priorityAssembly.GetType(typeName, false, true);
                    if (type == null)
                    {
                        type = _priorityAssembly.GetTypes()
                            .FirstOrDefault(t => t.Name == typeName && typeof(Node).IsAssignableFrom(t));
                    }

                    if (type != null && typeof(Node).IsAssignableFrom(type))
                    {
                        var nameConstructor = type.GetConstructor(new[] { typeof(string) });
                        if (nameConstructor != null)
                        {
                            return (Node)nameConstructor.Invoke(new object[] { nodeName });
                        }

                        var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
                        if (parameterlessConstructor != null)
                        {
                            var instance = (Node)parameterlessConstructor.Invoke(null);
                            instance.Name = nodeName;
                            return instance;
                        }
                    }
                }
                catch (Exception)
                {
                    // 如果优先程序集中找不到，继续在其他程序集中查找
                }
            }

            // 然后从其他程序集中查找
            foreach (var assembly in assemblies)
            {
                // 跳过优先程序集（已经查找过了）
                if (assembly == _priorityAssembly)
                {
                    continue;
                }
                
                try
                {
                    if (assembly.IsDynamic || assembly.FullName?.StartsWith("System.") == true ||
                        assembly.FullName?.StartsWith("Microsoft.") == true)
                    {
                        continue;
                    }

                    searchedAssemblies.Add(assembly.GetName().Name ?? "Unknown");

                    var type = assembly.GetType(typeName, false, true);
                    if (type == null)
                    {
                        type = assembly.GetTypes()
                            .FirstOrDefault(t => t.Name == typeName && typeof(Node).IsAssignableFrom(t));
                    }

                    if (type != null && typeof(Node).IsAssignableFrom(type))
                    {
                        var nameConstructor = type.GetConstructor(new[] { typeof(string) });
                        if (nameConstructor != null)
                        {
                            return (Node)nameConstructor.Invoke(new object[] { nodeName });
                        }

                        var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
                        if (parameterlessConstructor != null)
                        {
                            var instance = (Node)parameterlessConstructor.Invoke(null);
                            instance.Name = nodeName;
                            return instance;
                        }

                        throw new NotSupportedException(
                            $"Cannot create instance of type: {typeName}. No suitable constructor found.");
                    }
                }
                catch (Exception ex) when (ex is not NotSupportedException)
                {
                    continue;
                }
            }

            var assemblyList = string.Join(", ", searchedAssemblies.Take(10));
            throw new NotSupportedException(
                $"Node type '{typeName}' is not registered and could not be found via reflection. " +
                $"Searched assemblies: {assemblyList}...");
        }

        public static void RegisterNodeType(string typeName, Func<string, Node> factory)
        {
            NodeFactories[typeName] = factory;
        }

        /// <summary>
        /// 使用 NodeTypeRegistry 创建节点（如果可用）
        /// 这提供了更好的性能和类型发现支持
        /// </summary>
        private static NodeTypeRegistry? _nodeTypeRegistry;

        /// <summary>
        /// 设置节点类型注册表（可选，用于优化自定义节点创建）
        /// </summary>
        public static void SetNodeTypeRegistry(NodeTypeRegistry? registry)
        {
            _nodeTypeRegistry = registry;
        }

        /// <summary>
        /// 尝试使用 NodeTypeRegistry 创建节点，如果失败则回退到反射
        /// </summary>
        private Node? TryCreateNodeWithRegistry(string typeName, string nodeName)
        {
            if (_nodeTypeRegistry != null)
            {
                var node = _nodeTypeRegistry.CreateNode(typeName, nodeName);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
