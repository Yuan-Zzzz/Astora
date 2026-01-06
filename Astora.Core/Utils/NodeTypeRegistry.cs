using System.Reflection;
using Astora.Core;

namespace Astora.Core.Utils
{
    /// <summary>
    /// 节点类型信息
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
            // 可以添加特性来支持自定义显示名称
            // 目前直接使用类型名称
            return type.Name;
        }
        
        private static string? DetermineCategory(Type type)
        {
            // 根据命名空间或基类确定分类
            if (type.Namespace?.Contains("Astora.Core.Nodes") == true)
            {
                return "Core";
            }
            
            // 可以根据需要添加更多分类逻辑
            return type.Namespace;
        }
    }
    
    /// <summary>
    /// 节点类型注册表 - 负责发现和管理所有可用的节点类型
    /// </summary>
    public class NodeTypeRegistry
    {
        private readonly List<NodeTypeInfo> _nodeTypes = new List<NodeTypeInfo>();
        private readonly Dictionary<string, NodeTypeInfo> _nodeTypesByName = new Dictionary<string, NodeTypeInfo>();
        private bool _isDirty = true;
        private Assembly? _priorityAssembly; // 优先扫描的程序集（通常是项目程序集）
        
        /// <summary>
        /// 获取所有可用的节点类型
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
        /// 根据类型名称获取节点类型信息
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
        /// 设置优先扫描的程序集（通常是项目程序集）
        /// </summary>
        public void SetPriorityAssembly(Assembly? assembly)
        {
            _priorityAssembly = assembly;
        }

        /// <summary>
        /// 扫描并发现所有节点类型
        /// </summary>
        public void DiscoverNodeTypes()
        {
            _nodeTypes.Clear();
            _nodeTypesByName.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nodeBaseType = typeof(Node);
            var seenTypeNames = new HashSet<string>(); // 用于去重
            
            // 优先扫描项目程序集
            if (_priorityAssembly != null)
            {
                try
                {
                    DiscoverTypesFromAssembly(_priorityAssembly, nodeBaseType, seenTypeNames, isPriority: true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"扫描优先程序集失败: {ex.Message}");
                }
            }
            
            // 然后扫描 Core 程序集（Astora.Core）
            var coreAssembly = typeof(Node).Assembly;
            if (coreAssembly != _priorityAssembly)
            {
                try
                {
                    DiscoverTypesFromAssembly(coreAssembly, nodeBaseType, seenTypeNames, isPriority: false);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"扫描 Core 程序集失败: {ex.Message}");
                }
            }
            
            // 如果设置了优先程序集，只扫描 Core 和优先程序集，忽略其他程序集
            // 这样可以避免扫描到已卸载程序集中的类型
            if (_priorityAssembly != null)
            {
                // 只扫描 Core 和项目程序集，忽略其他程序集
                // 这样可以确保只显示当前有效的类型
                return;
            }
            
            // 如果没有优先程序集，扫描其他程序集（向后兼容）
            foreach (var assembly in assemblies)
            {
                // 跳过优先程序集（已经扫描过了）
                if (assembly == _priorityAssembly || assembly == coreAssembly)
                {
                    continue;
                }
                
                try
                {
                    // 跳过系统程序集和动态程序集
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
                    // 某些程序集可能无法完全加载（可能是已卸载的程序集），忽略错误
                    System.Console.WriteLine($"无法加载程序集 {assembly.FullName} 中的某些类型: {ex.Message}");
                }
                catch (InvalidOperationException)
                {
                    // 程序集来自已卸载的 AssemblyLoadContext，跳过
                    continue;
                }
                catch (Exception ex)
                {
                    // 忽略无法访问的程序集
                    System.Console.WriteLine($"扫描程序集 {assembly.FullName} 时出错: {ex.Message}");
                }
            }
            
            // 按分类和名称排序
            _nodeTypes.Sort((a, b) =>
            {
                // 先按分类排序
                var categoryCompare = string.Compare(a.Category ?? "", b.Category ?? "", StringComparison.Ordinal);
                if (categoryCompare != 0) return categoryCompare;
                
                // 再按显示名称排序
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });
            
            _isDirty = false;
        }
        
        /// <summary>
        /// 从指定程序集中发现节点类型
        /// </summary>
        private void DiscoverTypesFromAssembly(Assembly assembly, Type nodeBaseType, HashSet<string> seenTypeNames, bool isPriority)
        {
            // 检查程序集是否来自已卸载的 AssemblyLoadContext
            // 如果程序集来自可收集的上下文但上下文已卸载，GetTypes() 会抛出异常
            // 我们通过捕获异常来跳过这些程序集
            
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
                // 使用完整类型名作为唯一标识，避免重复
                var fullTypeName = type.FullName ?? type.Name;
                
                if (seenTypeNames.Contains(fullTypeName))
                {
                    // 如果已存在，且当前是优先程序集，则替换（优先使用新程序集中的类型）
                    if (isPriority)
                    {
                        // 移除旧的类型信息
                        var oldInfo = _nodeTypes.FirstOrDefault(i => (i.Type.FullName ?? i.Type.Name) == fullTypeName);
                        if (oldInfo != null)
                        {
                            _nodeTypes.Remove(oldInfo);
                        }
                    }
                    else
                    {
                        // 如果不是优先程序集，跳过（保留已存在的）
                        continue;
                    }
                }
                seenTypeNames.Add(fullTypeName);
                
                var info = new NodeTypeInfo(type);
                _nodeTypes.Add(info);
                
                // 使用完整类型名作为键，避免同名类型冲突
                if (!string.IsNullOrEmpty(type.FullName))
                {
                    _nodeTypesByName[type.FullName] = info;
                }
                // 如果类型名不存在，或当前是优先程序集，则更新简单名称
                if (isPriority || !_nodeTypesByName.ContainsKey(type.Name))
                {
                    _nodeTypesByName[type.Name] = info;
                }
            }
        }
        
        /// <summary>
        /// 检查类型是否有合适的构造函数
        /// </summary>
        private bool HasSuitableConstructor(Type type)
        {
            // 检查是否有接受 string 参数的构造函数
            var nameConstructor = type.GetConstructor(new[] { typeof(string) });
            if (nameConstructor != null) return true;
            
            // 检查是否有接受 string 和其他参数（可为null类型）的构造函数
            // 例如：Sprite(string name, Texture2D texture) 其中 texture 可以为 null
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
                {
                    // 检查后续参数是否都可以为null（引用类型或可空值类型）
                    bool allOptional = true;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        // 引用类型可以为null，可空值类型也可以为null
                        if (!param.ParameterType.IsClass && 
                            !(param.ParameterType.IsGenericType && 
                              param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            allOptional = false;
                            break;
                        }
                    }
                    if (allOptional) return true;
                }
            }
            
            // 检查是否有无参构造函数
            var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor != null) return true;
            
            return false;
        }
        
        /// <summary>
        /// 创建节点实例
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
        /// 创建节点实例（通过类型）
        /// </summary>
        public Node? CreateNode(Type nodeType, string nodeName)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
            {
                return null;
            }
            
            try
            {
                // 首先尝试使用接受 string 参数的构造函数
                var nameConstructor = nodeType.GetConstructor(new[] { typeof(string) });
                if (nameConstructor != null)
                {
                    return (Node)nameConstructor.Invoke(new object[] { nodeName });
                }
                
                // 然后尝试接受 string 和其他可选参数的构造函数
                // 例如：Sprite(string name, Texture2D texture) 其中 texture 可以为 null
                var constructors = nodeType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
                    {
                        // 检查后续参数是否都可以为null（引用类型或可空值类型）
                        bool allOptional = true;
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            var param = parameters[i];
                            if (!param.ParameterType.IsClass && 
                                !(param.ParameterType.IsGenericType && 
                                  param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                            {
                                allOptional = false;
                                break;
                            }
                        }
                        
                        if (allOptional)
                        {
                            // 构建参数数组：第一个是 nodeName，其余为 null
                            var args = new object[parameters.Length];
                            args[0] = nodeName;
                            for (int i = 1; i < parameters.Length; i++)
                            {
                                args[i] = null!; // 引用类型或可空值类型可以为 null
                            }
                            return (Node)constructor.Invoke(args);
                        }
                    }
                }
                
                // 如果没有，尝试无参构造函数
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
                System.Console.WriteLine($"创建节点 {nodeType.Name} 失败: {ex.Message}");
                return null;
            }
            
            return null;
        }
        
        /// <summary>
        /// 标记为需要重新发现（当程序集重新加载时调用）
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// 按分类获取节点类型
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

