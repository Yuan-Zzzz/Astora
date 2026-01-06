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
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !IgnoredProperties.Contains(p.Name));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(node);
                if (value == null) continue;

                var serializedValue = SerializeProperty(prop.PropertyType, value);
                if (serializedValue != null)
                {
                    serialized.Properties[prop.Name] = serializedValue;
                }
            }

            if (node.Children.Count > 0)
            {
                serialized.Children = node.Children.Select(SerializeNode).ToList();
            }

            return serialized;
        }

        private object? SerializeProperty(Type propertyType, object value)
        {
            if (propertyType == typeof(Vector2))
            {
                var vec = (Vector2)value;
                return new Dictionary<string, float> { { "x", vec.X }, { "y", vec.Y } };
            }

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

            if (propertyType == typeof(Texture2D))
            {
                return null;
            }

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

        private object? DeserializeProperty(Type propertyType, object value, string propertyName)
        {
            // Vector2
            if (propertyType == typeof(Vector2) && value is IDictionary dict)
            {
                var x = GetDictValue<float>(dict, "x") ?? 0f;
                var y = GetDictValue<float>(dict, "y") ?? 0f;
                return new Vector2(x, y);
            }

            if (propertyType == typeof(Color) && value is IDictionary colorDict)
            {
                var r = GetDictValue<int>(colorDict, "r") ?? 255;
                var g = GetDictValue<int>(colorDict, "g") ?? 255;
                var b = GetDictValue<int>(colorDict, "b") ?? 255;
                var a = GetDictValue<int>(colorDict, "a") ?? 255;
                return new Color(r, g, b, a);
            }

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

            if (propertyType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var searchedAssemblies = new List<string>();

            foreach (var assembly in assemblies)
            {
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
