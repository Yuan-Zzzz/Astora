using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.Utils;
using ImGuiNET;

namespace Astora.Editor.UI
{
    public class HierarchyPanel
    {
        private SceneTree _sceneTree;
        private HashSet<Node> _expandedNodes = new HashSet<Node>();
        private NodeTypeRegistry? _nodeTypeRegistry;
        private Node? _lastSelectedNode;
        private Queue<Node> _nodesToDelete = new Queue<Node>();
        
        public HierarchyPanel(SceneTree sceneTree, NodeTypeRegistry? nodeTypeRegistry = null)
        {
            _sceneTree = sceneTree;
            _nodeTypeRegistry = nodeTypeRegistry;
            
            if (_nodeTypeRegistry == null)
            {
                System.Console.WriteLine("警告: HierarchyPanel 初始化时 NodeTypeRegistry 为 null，节点创建菜单将无法正常工作");
            }
        }
        
        /// <summary>
        /// 设置节点类型注册表
        /// </summary>
        public void SetNodeTypeRegistry(NodeTypeRegistry registry)
        {
            _nodeTypeRegistry = registry;
        }
        
        /// <summary>
        /// 从展开集合中移除节点及其所有子节点
        /// </summary>
        private void RemoveNodeFromExpandedSet(Node node)
        {
            _expandedNodes.Remove(node);
            foreach (var child in node.Children)
            {
                RemoveNodeFromExpandedSet(child);
            }
        }
        
        /// <summary>
        /// Expand to the target node by expanding all parent nodes
        /// </summary>
        private void ExpandToNode(Node targetNode)
        {
            if (targetNode == null) return;
            
            // Traverse from target node up to root, expanding all parent nodes
            var current = targetNode;
            while (current != null && current != _sceneTree.Root)
            {
                if (current.Parent != null)
                {
                    _expandedNodes.Add(current.Parent);
                }
                current = current.Parent;
            }
        }
        
        /// <summary>
        /// 生成唯一的节点名称
        /// </summary>
        private string GenerateUniqueNodeName(Node parent, string baseName)
        {
            var existingNames = new HashSet<string>();
            if (parent != null)
            {
                foreach (var child in parent.Children)
                {
                    existingNames.Add(child.Name);
                }
            }
            else if (_sceneTree.Root != null)
            {
                existingNames.Add(_sceneTree.Root.Name);
            }
            
            string name = baseName;
            int counter = 1;
            while (existingNames.Contains(name))
            {
                name = $"{baseName}{counter}";
                counter++;
            }
            
            return name;
        }
        
        public void Render(ref Node? selectedNode)
        {
            ImGui.Begin("Hierarchy");
            
            // If selected node changed, expand to that node
            if (selectedNode != _lastSelectedNode && selectedNode != null)
            {
                ExpandToNode(selectedNode);
                _lastSelectedNode = selectedNode;
            }
            else if (selectedNode == null)
            {
                _lastSelectedNode = null;
            }
            
            if (_sceneTree.Root != null)
            {
                RenderNode(_sceneTree.Root, ref selectedNode);
            }
            else
            {
                ImGui.Text("Scene is empty");
                if (ImGui.Button("Create Root Node"))
                {
                    _sceneTree.ChangeScene(new Node2D("Root"));
                }
            }
            
            // Right-click context menu for window background (only shows when clicking on empty area)
            // Use NoOpenOverItems flag to ensure window menu doesn't show when hovering over nodes
            if (ImGui.BeginPopupContextWindow("", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            {
                // Window-level menu: create nodes at root or selected node
                if (ImGui.BeginMenu("Create"))
                {
                    RenderNodeCreationMenu(selectedNode ?? _sceneTree.Root, ref selectedNode);
                    ImGui.EndMenu();
                }
                ImGui.EndPopup();
            }
            
            // Process all pending node deletions after rendering is complete
            // This prevents "Collection was modified during enumeration" exceptions
            while (_nodesToDelete.Count > 0)
            {
                var nodeToDelete = _nodesToDelete.Dequeue();
                
                // Perform the actual deletion
                if (nodeToDelete.Parent != null)
                {
                    nodeToDelete.Parent.RemoveChild(nodeToDelete);
                }
                else if (nodeToDelete == _sceneTree.Root)
                {
                    // If it's the root node, clear the scene
                    _sceneTree.ChangeScene(null);
                    _expandedNodes.Clear(); // Clear expanded set
                }
            }
            
            ImGui.End();
        }
        
        private void RenderNode(Node node, ref Node? selectedNode)
        {
            // 使用 PushID 确保 ID 作用域正确
            ImGui.PushID(node.GetHashCode());
            
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (selectedNode == node)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (node.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;
            
            // 如果节点在展开集合中，设置为默认展开
            if (_expandedNodes.Contains(node))
            {
                flags |= ImGuiTreeNodeFlags.DefaultOpen;
            }
            
            // 只使用节点名称作为标签，ID 由 PushID 管理
            bool isOpen = ImGui.TreeNodeEx(node.Name, flags);
            
            // 跟踪展开状态
            if (isOpen)
            {
                _expandedNodes.Add(node);
            }
            else
            {
                _expandedNodes.Remove(node);
            }
            
            if (ImGui.IsItemClicked())
            {
                selectedNode = node;
            }
            
            // 检测右键释放事件（现代游戏引擎的标准行为：点击松开后才显示菜单）
            if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup($"##node_context_{node.GetHashCode()}");
            }
            
            // 显示右键菜单（菜单会保持打开直到用户点击其他地方或选择菜单项）
            if (ImGui.BeginPopup($"##node_context_{node.GetHashCode()}"))
            {
                // Create 子菜单
                if (ImGui.BeginMenu("Create"))
                {
                    RenderNodeCreationMenu(node, ref selectedNode);
                    ImGui.EndMenu();
                }
                
                // Separator
                ImGui.Separator();
                
                // Delete 菜单项 - 确保总是显示
                bool deleteClicked = ImGui.MenuItem("Delete");
                if (deleteClicked)
                {
                    // 将节点添加到删除队列，延迟到渲染完成后删除
                    // 这样可以避免在遍历集合时修改集合导致的异常
                    _nodesToDelete.Enqueue(node);
                    
                    // 从展开集合中移除该节点及其所有子节点（立即执行，不影响遍历）
                    RemoveNodeFromExpandedSet(node);
                    
                    // 如果删除的是选中的节点，清除选择（立即执行）
                    if (selectedNode == node)
                    {
                        selectedNode = null;
                    }
                    
                    ImGui.CloseCurrentPopup();
                }
                
                // Copy 菜单项 - 确保总是显示
                bool copyClicked = ImGui.MenuItem("Copy");
                if (copyClicked)
                {
                    // Implement copy functionality
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
            
            if (isOpen)
            {
                foreach (var child in node.Children)
                {
                    RenderNode(child, ref selectedNode);
                }
                ImGui.TreePop();
            }
            
            ImGui.PopID();
        }
        
        /// <summary>
        /// 渲染节点创建菜单
        /// </summary>
        private void RenderNodeCreationMenu(Node? parentNode, ref Node? selectedNode)
        {
            if (_nodeTypeRegistry == null)
            {
                // 如果没有注册表，显示错误提示
                ImGui.TextDisabled("节点类型注册表未初始化");
                ImGui.Separator();
                if (ImGui.MenuItem("重新初始化注册表"))
                {
                    System.Console.WriteLine("警告: NodeTypeRegistry 为 null，无法重新初始化");
                }
                return;
            }
            
            var nodeTypesByCategory = _nodeTypeRegistry.GetNodeTypesByCategory();
            
            // 检查是否有可用的节点类型
            if (nodeTypesByCategory.Count == 0 || nodeTypesByCategory.All(kvp => kvp.Value.Count == 0))
            {
                ImGui.TextDisabled("未发现可用的节点类型");
                ImGui.Separator();
                if (ImGui.MenuItem("刷新节点列表"))
                {
                    _nodeTypeRegistry.MarkDirty();
                    _nodeTypeRegistry.DiscoverNodeTypes();
                }
                return;
            }
            
            // 优先显示 Core 分类的节点
            if (nodeTypesByCategory.TryGetValue("Core", out var coreNodes))
            {
                foreach (var nodeTypeInfo in coreNodes)
                {
                    if (ImGui.MenuItem(nodeTypeInfo.DisplayName))
                    {
                        CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                    }
                }
                
                if (coreNodes.Count > 0 && nodeTypesByCategory.Count > 1)
                {
                    ImGui.Separator();
                }
            }
            
            // 显示其他分类的节点
            foreach (var category in nodeTypesByCategory.Keys)
            {
                if (category == "Core") continue;
                
                var nodeTypes = nodeTypesByCategory[category];
                if (nodeTypes.Count == 0) continue;
                
                // 如果有命名空间，可以按命名空间分组
                if (!string.IsNullOrEmpty(category) && category != "Other")
                {
                    if (ImGui.BeginMenu(category))
                    {
                        foreach (var nodeTypeInfo in nodeTypes)
                        {
                            if (ImGui.MenuItem(nodeTypeInfo.DisplayName))
                            {
                                CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                            }
                        }
                        ImGui.EndMenu();
                    }
                }
                else
                {
                    // 直接显示节点
                    foreach (var nodeTypeInfo in nodeTypes)
                    {
                        if (ImGui.MenuItem(nodeTypeInfo.DisplayName))
                        {
                            CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                        }
                    }
                }
            }
            
            // 在菜单底部添加刷新选项
            ImGui.Separator();
            if (ImGui.MenuItem("刷新节点列表"))
            {
                _nodeTypeRegistry.MarkDirty();
                _nodeTypeRegistry.DiscoverNodeTypes();
            }
        }
        
        /// <summary>
        /// 从类型信息创建节点
        /// </summary>
        private void CreateNodeFromType(NodeTypeInfo nodeTypeInfo, Node? parentNode, ref Node? selectedNode)
        {
            if (_nodeTypeRegistry == null) return;
            
            var baseName = $"New{nodeTypeInfo.DisplayName}";
            var name = GenerateUniqueNodeName(parentNode, baseName);
            var newNode = _nodeTypeRegistry.CreateNode(nodeTypeInfo.TypeName, name);
            
            if (newNode != null)
            {
                CreateNodeInternal(newNode, parentNode, ref selectedNode);
                
                // 特殊处理：如果是 Camera2D，设置为活动摄像机
                if (newNode is Camera2D camera && parentNode == _sceneTree.Root)
                {
                    _sceneTree.SetCurrentCamera(camera);
                }
            }
        }
        
        /// <summary>
        /// 创建节点的内部逻辑
        /// </summary>
        private void CreateNodeInternal(Node newNode, Node? parentNode, ref Node? selectedNode)
        {
            if (parentNode != null)
            {
                parentNode.AddChild(newNode);
                _expandedNodes.Add(parentNode); // 自动展开父节点以显示新子节点
            }
            else
            {
                _sceneTree.ChangeScene(newNode);
            }
            selectedNode = newNode; // 自动选中新节点
        }
    }
}