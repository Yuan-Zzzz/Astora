using System.Reflection;
using Astora.Core;
using Astora.Core.Nodes;

namespace Astora.Core.Utils
{
    /// <summary>
    /// Node Information
    /// </summary>
    public class NodeTypeInfo
    {
        public Type Type { get; set; }
        public string TypeName { get; set; }
        public string DisplayName { get; set; }
        public string? Namespace { get; set; }
        public string? Category { get; set; }
        
        public NodeTypeInfo(Type type)
        {
            Type = type;
            TypeName = type.Name;
            DisplayName = GetNodeDisplayName(type);
            Namespace = type.Namespace;
            Category = DetermineCategory(type);
        }
        
        private static string GetNodeDisplayName(Type type)
        {
            return type.Name;
        }
        
        private static string? DetermineCategory(Type type)
        {
            if (type.Namespace?.Contains("Astora.Core.Nodes") == true)
            {
                return "Core";
            }
            return type.Namespace;
        }
    }
    
    /// <summary>
    /// Node Type Registry
    /// </summary>
    public class NodeTypeRegistry
    {
        private readonly List<NodeTypeInfo> _nodeTypes = new List<NodeTypeInfo>();
        private readonly Dictionary<string, NodeTypeInfo> _nodeTypesByName = new Dictionary<string, NodeTypeInfo>();
        private bool _isDirty = true;
        private Assembly? _priorityAssembly; 
        
        /// <summary>
        /// Get all available node types
        /// </summary>
        public IEnumerable<NodeTypeInfo> GetAvailableNodeTypes()
        {
            if (_isDirty)
            {
                DiscoverNodeTypes();
            }
            return _nodeTypes;
        }
        
        /// <summary>
        /// Get node type info by type name
        /// </summary>
        public NodeTypeInfo? GetNodeTypeInfo(string typeName)
        {
            if (_isDirty)
            {
                DiscoverNodeTypes();
            }
            return _nodeTypesByName.TryGetValue(typeName, out var info) ? info : null;
        }
        
        /// <summary>
        /// Set the priority assembly to scan for node types
        /// </summary>
        public void SetPriorityAssembly(Assembly? assembly)
        {
            _priorityAssembly = assembly;
        }

        /// <summary>
        /// Discover node types from loaded assemblies
        /// </summary>
        public void DiscoverNodeTypes()
        {
            _nodeTypes.Clear();
            _nodeTypesByName.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nodeBaseType = typeof(Node);
            var seenTypeNames = new HashSet<string>();
            
            if (_priorityAssembly != null)
            {
                try
                {
                    DiscoverTypesFromAssembly(_priorityAssembly, nodeBaseType, seenTypeNames, isPriority: true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Scanning priority assembly failed: {ex.Message}");
                }
            }
            
            var coreAssembly = typeof(Node).Assembly;
            if (coreAssembly != _priorityAssembly)
            {
                try
                {
                    DiscoverTypesFromAssembly(coreAssembly, nodeBaseType, seenTypeNames, isPriority: false);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Scanning core assembly failed: {ex.Message}");
                }
            }
            
            if (_priorityAssembly != null)
            {
                return;
            }
            
            foreach (var assembly in assemblies)
            {
                if (assembly == _priorityAssembly || assembly == coreAssembly)
                {
                    continue;
                }
                
                try
                {
                    if (assembly.IsDynamic || 
                        assembly.FullName?.StartsWith("System.") == true || 
                        assembly.FullName?.StartsWith("Microsoft.") == true ||
                        assembly.FullName?.StartsWith("YamlDotNet") == true ||
                        assembly.FullName?.StartsWith("ImGui.NET") == true ||
                        assembly.FullName?.StartsWith("MonoGame") == true)
                    {
                        continue;
                    }
                    
                    DiscoverTypesFromAssembly(assembly, nodeBaseType, seenTypeNames, isPriority: false);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    System.Console.WriteLine($"Failed to load types from assembly {assembly.FullName}: {ex.Message}");
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Scanning assembly {assembly.FullName} failed: {ex.Message}");
                }
            }
            
            _nodeTypes.Sort((a, b) =>
            {
                var categoryCompare = string.Compare(a.Category ?? "", b.Category ?? "", StringComparison.Ordinal);
                if (categoryCompare != 0) return categoryCompare;
                
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });
            
            _isDirty = false;
        }
        
        /// <summary>
        /// Find and register node types from a specific assembly
        /// </summary>
        private void DiscoverTypesFromAssembly(Assembly assembly, Type nodeBaseType, HashSet<string> seenTypeNames, bool isPriority)
        {
            var types = assembly.GetTypes()
                .Where(t => 
                    !t.IsAbstract && 
                    !t.IsInterface && 
                    !t.IsGenericTypeDefinition &&
                    nodeBaseType.IsAssignableFrom(t) &&
                    t != nodeBaseType &&
                    HasSuitableConstructor(t));
            
            foreach (var type in types)
            {
                var fullTypeName = type.FullName ?? type.Name;
                
                if (seenTypeNames.Contains(fullTypeName))
                {
                    if (isPriority)
                    {
                        var oldInfo = _nodeTypes.FirstOrDefault(i => (i.Type.FullName ?? i.Type.Name) == fullTypeName);
                        if (oldInfo != null)
                        {
                            _nodeTypes.Remove(oldInfo);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                seenTypeNames.Add(fullTypeName);
                
                var info = new NodeTypeInfo(type);
                _nodeTypes.Add(info);
                
                if (!string.IsNullOrEmpty(type.FullName))
                {
                    _nodeTypesByName[type.FullName] = info;
                }
                if (isPriority || !_nodeTypesByName.ContainsKey(type.Name))
                {
                    _nodeTypesByName[type.Name] = info;
                }
            }
        }
        
        /// <summary>
        /// Get whether the type has a suitable constructor
        /// </summary>
        private bool HasSuitableConstructor(Type type)
        {
            var nameConstructor = type.GetConstructor(new[] { typeof(string) });
            if (nameConstructor != null) return true;
            
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
                {
                    bool allOptional = true;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        // 检查参数是否有默认值（可选参数）
                        bool hasDefaultValue = param.HasDefaultValue || param.IsOptional;
                        // 检查是否是类类型或可空类型
                        bool isClassOrNullable = param.ParameterType.IsClass || 
                            (param.ParameterType.IsGenericType && 
                             param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>));
                        
                        // 如果参数没有默认值，且不是类类型或可空类型，则不是可选的
                        if (!hasDefaultValue && !isClassOrNullable)
                        {
                            allOptional = false;
                            break;
                        }
                    }
                    if (allOptional) return true;
                }
            }
            
            var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor != null) return true;
            
            return false;
        }
        
        /// <summary>
        /// Create node instance (by type name)
        /// </summary>
        public Node? CreateNode(string typeName, string nodeName)
        {
            var typeInfo = GetNodeTypeInfo(typeName);
            if (typeInfo == null)
            {
                return null;
            }
            
            return CreateNode(typeInfo.Type, nodeName);
        }
        
        /// <summary>
        /// Create node instance (by Type)
        /// </summary>
        public Node? CreateNode(Type nodeType, string nodeName)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
            {
                return null;
            }
            
            try
            {
                var nameConstructor = nodeType.GetConstructor(new[] { typeof(string) });
                if (nameConstructor != null)
                {
                    return (Node)nameConstructor.Invoke(new object[] { nodeName });
                }
                
                var constructors = nodeType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
                    {
                        bool allOptional = true;
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            var param = parameters[i];
                            bool hasDefaultValue = param.HasDefaultValue || param.IsOptional;
                            bool isClassOrNullable = param.ParameterType.IsClass || 
                                (param.ParameterType.IsGenericType && 
                                 param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>));
                            
                            if (!hasDefaultValue && !isClassOrNullable)
                            {
                                allOptional = false;
                                break;
                            }
                        }
                        
                        if (allOptional)
                        {
                            var args = new object[parameters.Length];
                            args[0] = nodeName;
                            for (int i = 1; i < parameters.Length; i++)
                            {
                                var param = parameters[i];
                                if (param.HasDefaultValue)
                                {
                                    // 使用参数的默认值
                                    args[i] = param.DefaultValue ?? 
                                        (param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType)! : null!);
                                }
                                else if (param.ParameterType.IsClass)
                                {
                                    // 类类型使用 null
                                    args[i] = null!;
                                }
                                else if (param.ParameterType.IsGenericType && 
                                         param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    // 可空类型使用 null
                                    args[i] = null!;
                                }
                                else
                                {
                                    // 其他值类型使用默认值（这种情况理论上不应该发生，因为 allOptional 检查已经过滤了）
                                    args[i] = Activator.CreateInstance(param.ParameterType)!;
                                }
                            }
                            return (Node)constructor.Invoke(args);
                        }
                    }
                }
                
                var parameterlessConstructor = nodeType.GetConstructor(Type.EmptyTypes);
                if (parameterlessConstructor != null)
                {
                    var instance = (Node)parameterlessConstructor.Invoke(null);
                    instance.Name = nodeName;
                    return instance;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Creating node of type {nodeType.FullName} failed: {ex.Message}");
                return null;
            }
            
            return null;
        }
        
        /// <summary>
        /// Mark the registry as dirty to trigger re-discovery
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// Get node types grouped by category
        /// </summary>
        public Dictionary<string, List<NodeTypeInfo>> GetNodeTypesByCategory()
        {
            if (_isDirty)
            {
                DiscoverNodeTypes();
            }
            
            var result = new Dictionary<string, List<NodeTypeInfo>>();
            foreach (var typeInfo in _nodeTypes)
            {
                var category = typeInfo.Category ?? "Other";
                if (!result.ContainsKey(category))
                {
                    result[category] = new List<NodeTypeInfo>();
                }
                result[category].Add(typeInfo);
            }
            
            return result;
        }
    }
}

