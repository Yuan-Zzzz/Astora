using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using ImGuiNET;

namespace Astora.Editor.UI
{
    public class HierarchyPanel
    {
        private SceneTree _sceneTree;
        private HashSet<Node> _expandedNodes = new HashSet<Node>();
        
        public HierarchyPanel(SceneTree sceneTree)
        {
            _sceneTree = sceneTree;
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
                if (ImGui.MenuItem("Create Node2D"))
                {
                    Node parentNode = selectedNode ?? _sceneTree.Root;
                    var name = GenerateUniqueNodeName(parentNode, "NewNode2D");
                    var newNode = new Node2D(name);
                    if (parentNode != null)
                    {
                        parentNode.AddChild(newNode);
                        _expandedNodes.Add(parentNode); // 自动展开父节点以显示新子节点
                    }
                    else
                        _sceneTree.ChangeScene(newNode);
                    selectedNode = newNode; // 自动选中新节点
                }
                if (ImGui.MenuItem("Create Sprite"))
                {
                    Node parentNode = selectedNode ?? _sceneTree.Root;
                    var name = GenerateUniqueNodeName(parentNode, "NewSprite");
                    var newNode = new Sprite(name, null);
                    if (parentNode != null)
                    {
                        parentNode.AddChild(newNode);
                        _expandedNodes.Add(parentNode); // 自动展开父节点以显示新子节点
                    }
                    else
                        _sceneTree.ChangeScene(newNode);
                    selectedNode = newNode; // 自动选中新节点
                }
                if (ImGui.MenuItem("Create Camera2D"))
                {
                    Node parentNode = selectedNode ?? _sceneTree.Root;
                    var name = GenerateUniqueNodeName(parentNode, "NewCamera");
                    var newNode = new Camera2D(name);
                    if (parentNode != null)
                    {
                        parentNode.AddChild(newNode);
                        _expandedNodes.Add(parentNode); // 自动展开父节点以显示新子节点
                        // 如果父节点是根节点，设置为活动摄像机
                        if (parentNode == _sceneTree.Root)
                        {
                            _sceneTree.SetCurrentCamera(newNode);
                        }
                    }
                    else
                    {
                        _sceneTree.ChangeScene(newNode);
                        _sceneTree.SetCurrentCamera(newNode);
                    }
                    selectedNode = newNode; // 自动选中新节点
                }
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
                    if (ImGui.MenuItem("Node2D"))
                    {
                        var name = GenerateUniqueNodeName(node, "NewNode2D");
                        var newNode = new Node2D(name);
                        node.AddChild(newNode);
                        _expandedNodes.Add(node); // 自动展开父节点以显示新子节点
                        selectedNode = newNode; // 自动选中新节点
                    }
                    if (ImGui.MenuItem("Sprite"))
                    {
                        var name = GenerateUniqueNodeName(node, "NewSprite");
                        var newNode = new Sprite(name, null);
                        node.AddChild(newNode);
                        _expandedNodes.Add(node); // 自动展开父节点以显示新子节点
                        selectedNode = newNode; // 自动选中新节点
                    }
                    if (ImGui.MenuItem("Camera2D"))
                    {
                        var name = GenerateUniqueNodeName(node, "NewCamera");
                        var newNode = new Camera2D(name);
                        node.AddChild(newNode);
                        _expandedNodes.Add(node); // 自动展开父节点以显示新子节点
                        // 如果当前节点是根节点，设置为活动摄像机
                        if (node == _sceneTree.Root)
                        {
                            _sceneTree.SetCurrentCamera(newNode);
                        }
                        selectedNode = newNode; // 自动选中新节点
                    }
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
    }
}