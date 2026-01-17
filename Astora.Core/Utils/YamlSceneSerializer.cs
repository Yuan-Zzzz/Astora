using Astora.Core;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;
using Astora.Core.Attributes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Astora.Core.Resources;

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
            { nameof(Camera2D), name => new Camera2D(name) },
            { nameof(CPUParticles2D), name => new CPUParticles2D(name)},
            { nameof(AnimatedSprite), name => new AnimatedSprite(name)}
        };

        private static readonly HashSet<string> IgnoredFieldNames = new()
        {
            "_parent",
            "_children",
            "_isQueuedForDeletion",
            "_defaultWhiteTexture" // Static field in Sprite
        };
        
        private static readonly HashSet<string> IgnoredPropertyNames = new()
        {
            "Parent",
            "Children",
            "IsQueuedForDeletion",
            "GlobalTransform",
            "GlobalPosition"
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
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !IgnoredFieldNames.Contains(f.Name) && !f.IsStatic);

            foreach (var field in fields)
            {
                bool shouldSerialize = field.IsPublic || field.IsDefined(typeof(SerializeFieldAttribute), false);
                
                if (!shouldSerialize)
                    continue;

                var value = field.GetValue(node);
                
                var serializedValue = SerializeField(field, value);
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

        private object? SerializeField(FieldInfo fieldInfo, object value)
        {
            // Handle null values
            if (value == null)
                return null;

            var fieldType = fieldInfo.FieldType;
            
            // Check the ContentRelativePathAttribute to translate the full path to relative path by Content.RootDirectory
            if (fieldType == typeof(string) && fieldInfo != null && fieldInfo.IsDefined(typeof(ContentRelativePath), false) && value is string pathValue)
            {
                if (string.IsNullOrEmpty(pathValue))
                  return pathValue;

                // translate the absolute path to relative path
                if (Path.IsPathRooted(pathValue))
                {
                    var contentRoot = Engine.Content.RootDirectory;
                    if (pathValue.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        var relativePath = Path.GetRelativePath(contentRoot, pathValue);
                        return relativePath.Replace('\\','/');
                    }
                }

                return pathValue;
            }

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
            
            if (fieldType == typeof(Rectangle) || fieldType == typeof(Rectangle?))
            {
                if (value is Rectangle rect)
                {
                    return new Dictionary<string, int>
                    {
                        { "x", rect.X },
                        { "y", rect.Y },
                        { "width", rect.Width },
                        { "height", rect.Height }
                    };
                }
                return null;
            }

            // Don't serialize Texture2D, Effect, or BlendState (runtime objects)
            // BlendState is a enum what could be serialized
            if (fieldType == typeof(Texture2D) || 
                fieldType == typeof(Effect) || 
                fieldType == typeof(BlendState))
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
            
            if (NodeFactories.TryGetValue(serialized.Type, out var factory))
            {
                node = factory(serialized.Name);
            }
            else if ((node = TryCreateNodeWithRegistry(serialized.Type, serialized.Name)) != null)
            {
            }
            else
            {
                node = CreateNodeByReflection(serialized.Type, serialized.Name);
            }

            if (serialized.Properties != null)
            {
                var type = node.GetType();
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !IgnoredFieldNames.Contains(f.Name) && !f.IsStatic);

                foreach (var field in fields)
                {
                    bool shouldDeserialize = field.IsPublic || field.IsDefined(typeof(SerializeFieldAttribute), false);
                    
                    if (!shouldDeserialize)
                        continue;

                    if (!serialized.Properties.TryGetValue(field.Name, out var value))
                        continue;

                    try
                    {
                        var deserializedValue = DeserializeField(field, value);
                        if (deserializedValue != null || (value == null && !field.FieldType.IsValueType))
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

        private object? DeserializeField(FieldInfo fieldInfo, object value)
        {
            var fieldType = fieldInfo.FieldType;
            // Translate ContentRelativePathAttribute filed to absolute path
            if (fieldType == typeof(string) && fieldInfo.IsDefined(typeof(ContentRelativePath), false) && value is string pathValue)
            {
                if (string.IsNullOrEmpty(pathValue))
                    return pathValue;

                if (!Path.IsPathRooted(pathValue))
                {
                    var contentRoot = Engine.Content.RootDirectory;
                    var absolutePath = Path.GetFullPath(Path.Combine(contentRoot, pathValue));
                    return absolutePath;
                }

                return pathValue;
            }


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
            
            if ((fieldType == typeof(Rectangle) || fieldType == typeof(Rectangle?)) && value is IDictionary rectDict)
            {
                var x = GetDictValue<int>(rectDict, "x") ?? 0;
                var y = GetDictValue<int>(rectDict, "y") ?? 0;
                var width = GetDictValue<int>(rectDict, "width") ?? 0;
                var height = GetDictValue<int>(rectDict, "height") ?? 0;
                return new Rectangle(x, y, width, height);
            }

            if (fieldType == typeof(Texture2D) && value is string texturePath)
            {
                if (Engine.Content != null && !string.IsNullOrEmpty(texturePath))
                {
                    // Deserialization shoude be fullpath,and relative path in serialization
                    try
                    {
                        return Resources.ResourceLoader.Load<Texture2DResource>(texturePath).Texture;
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
        /// For prioritizing assembly lookup (usually the project assembly)
        /// </summary>
        private static Assembly? _priorityAssembly;
        
        /// <summary>
        /// Sets the priority assembly for node type lookup
        /// </summary>
        public static void SetPriorityAssembly(Assembly? assembly)
        {
            _priorityAssembly = assembly;
        }
        
        private Node CreateNodeByReflection(string typeName, string nodeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var searchedAssemblies = new List<string>();
            
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
                }
            }
            
            foreach (var assembly in assemblies)
            {
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
        /// Use a NodeTypeRegistry to optimize custom node creation
        /// </summary>
        private static NodeTypeRegistry? _nodeTypeRegistry;

        /// <summary>
        /// Sets the NodeTypeRegistry for custom node creation
        /// </summary>
        public static void SetNodeTypeRegistry(NodeTypeRegistry? registry)
        {
            _nodeTypeRegistry = registry;
        }

        /// <summary>
        /// Try to create a node using the NodeTypeRegistry
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
