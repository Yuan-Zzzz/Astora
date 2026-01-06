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
        
        public HierarchyPanel(SceneTree sceneTree, NodeTypeRegistry? nodeTypeRegistry = null)
        {
            _sceneTree = sceneTree;
            _nodeTypeRegistry = nodeTypeRegistry;
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
        
        public void Render(ref Node selectedNode)
        {
            ImGui.Begin("Hierarchy");
            
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
            
            // Right-click context menu
            if (ImGui.BeginPopupContextWindow())
            {
                RenderNodeCreationMenu(selectedNode ?? _sceneTree.Root, ref selectedNode);
                ImGui.EndPopup();
            }
            
            ImGui.End();
        }
        
        private void RenderNode(Node node, ref Node selectedNode)
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
            
            // 使用唯一标识符确保右键菜单正确绑定到节点项
            if (ImGui.BeginPopupContextItem($"##node_{node.GetHashCode()}"))
            {
                // 创建子节点选项
                if (ImGui.BeginMenu("Create Child"))
                {
                    RenderNodeCreationMenu(node, ref selectedNode);
                    ImGui.EndMenu();
                }
                
                ImGui.Separator();
                
                if (ImGui.MenuItem("Delete"))
                {
                    // 从展开集合中移除该节点及其所有子节点
                    RemoveNodeFromExpandedSet(node);
                    
                    // 在编辑器模式下直接删除
                    if (node.Parent != null)
                    {
                        node.Parent.RemoveChild(node);
                    }
                    else if (node == _sceneTree.Root)
                    {
                        // 如果是根节点，清空场景
                        _sceneTree.ChangeScene(null);
                        _expandedNodes.Clear(); // 清空展开集合
                    }
                    
                    // 如果删除的是选中的节点，清除选择
                    if (selectedNode == node)
                    {
                        selectedNode = null;
                    }
                }
                if (ImGui.MenuItem("Copy"))
                {
                    // Implement copy functionality
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
                // 如果没有注册表，使用硬编码的默认节点类型
                RenderDefaultNodeMenu(parentNode, ref selectedNode);
                return;
            }
            
            var nodeTypesByCategory = _nodeTypeRegistry.GetNodeTypesByCategory();
            
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
        }
        
        /// <summary>
        /// 渲染默认节点菜单（当没有注册表时使用）
        /// </summary>
        private void RenderDefaultNodeMenu(Node? parentNode, ref Node? selectedNode)
        {
            if (ImGui.MenuItem("Node2D"))
            {
                var name = GenerateUniqueNodeName(parentNode, "NewNode2D");
                var newNode = new Node2D(name);
                CreateNodeInternal(newNode, parentNode, ref selectedNode);
            }
            if (ImGui.MenuItem("Sprite"))
            {
                var name = GenerateUniqueNodeName(parentNode, "NewSprite");
                var newNode = new Sprite(name, null);
                CreateNodeInternal(newNode, parentNode, ref selectedNode);
            }
            if (ImGui.MenuItem("Camera2D"))
            {
                var name = GenerateUniqueNodeName(parentNode, "NewCamera");
                var newNode = new Camera2D(name);
                CreateNodeInternal(newNode, parentNode, ref selectedNode);
                
                // 如果父节点是根节点，设置为活动摄像机
                if (parentNode == _sceneTree.Root)
                {
                    _sceneTree.SetCurrentCamera(newNode);
                }
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