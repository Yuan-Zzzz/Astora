using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.Utils;
using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 场景树面板 - Godot 风格：搜索、图标、拖拽、可见性/锁定
    /// </summary>
    public class HierarchyPanel
    {
        private SceneTree _sceneTree;
        private HashSet<Node> _expandedNodes = new HashSet<Node>();
        private NodeTypeRegistry? _nodeTypeRegistry;
        private Node? _lastSelectedNode;
        private Queue<Node> _nodesToDelete = new Queue<Node>();

        // 搜索
        private string _searchQuery = string.Empty;
        private HashSet<Node> _searchMatchNodes = new HashSet<Node>();

        // 可见性和锁定（编辑器层面跟踪，不修改 Node 核心类）
        private HashSet<Node> _hiddenNodes = new HashSet<Node>();
        private HashSet<Node> _lockedNodes = new HashSet<Node>();

        // 拖拽
        private Node? _dragSourceNode;

        // 节点类型图标映射
        private static readonly Dictionary<string, (string Icon, Vector4 Color)> NodeTypeIcons = new()
        {
            { "Node",           ("[N]",   new Vector4(0.7f, 0.7f, 0.7f, 1f)) },
            { "Node2D",         ("[2D]",  new Vector4(0.55f, 0.75f, 1.0f, 1f)) },
            { "Sprite",         ("[S]",   new Vector4(0.55f, 0.85f, 0.45f, 1f)) },
            { "Camera2D",       ("[C]",   new Vector4(0.95f, 0.75f, 0.3f, 1f)) },
            { "AnimatedSprite", ("[AS]",  new Vector4(0.85f, 0.55f, 0.85f, 1f)) },
            { "CPUParticles2D", ("[P]",   new Vector4(1.0f, 0.55f, 0.35f, 1f)) },
            { "Control",        ("[UI]",  new Vector4(0.45f, 0.85f, 0.45f, 1f)) },
            { "Label",          ("[Lbl]", new Vector4(0.45f, 0.85f, 0.45f, 1f)) },
            { "Button",         ("[Btn]", new Vector4(0.45f, 0.85f, 0.45f, 1f)) },
            { "Panel",          ("[Pnl]", new Vector4(0.45f, 0.85f, 0.45f, 1f)) },
            { "CanvasLayer",    ("[CL]",  new Vector4(0.6f, 0.6f, 0.9f, 1f)) },
        };

        public HierarchyPanel(SceneTree sceneTree, NodeTypeRegistry? nodeTypeRegistry = null)
        {
            _sceneTree = sceneTree;
            _nodeTypeRegistry = nodeTypeRegistry;
        }

        public void SetNodeTypeRegistry(NodeTypeRegistry registry)
        {
            _nodeTypeRegistry = registry;
        }

        /// <summary>
        /// 检查节点是否被隐藏（编辑器层面）
        /// </summary>
        public bool IsNodeHidden(Node node) => _hiddenNodes.Contains(node);

        /// <summary>
        /// 检查节点是否被锁定（编辑器层面）
        /// </summary>
        public bool IsNodeLocked(Node node) => _lockedNodes.Contains(node);

        private void RemoveNodeFromExpandedSet(Node node)
        {
            _expandedNodes.Remove(node);
            foreach (var child in node.Children)
                RemoveNodeFromExpandedSet(child);
        }

        private void ExpandToNode(Node targetNode)
        {
            if (targetNode == null) return;
            var current = targetNode;
            while (current != null && current != _sceneTree.Root)
            {
                if (current.Parent != null)
                    _expandedNodes.Add(current.Parent);
                current = current.Parent;
            }
        }

        private string GenerateUniqueNodeName(Node parent, string baseName)
        {
            var existingNames = new HashSet<string>();
            if (parent != null)
            {
                foreach (var child in parent.Children)
                    existingNames.Add(child.Name);
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

        /// <summary>
        /// 执行搜索
        /// </summary>
        private void UpdateSearch()
        {
            _searchMatchNodes.Clear();
            if (string.IsNullOrWhiteSpace(_searchQuery) || _sceneTree.Root == null)
                return;

            CollectMatchingNodes(_sceneTree.Root);
        }

        private bool CollectMatchingNodes(Node node)
        {
            bool nameMatch = node.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase);
            bool childMatch = false;

            foreach (var child in node.Children)
            {
                if (CollectMatchingNodes(child))
                    childMatch = true;
            }

            if (nameMatch || childMatch)
            {
                _searchMatchNodes.Add(node);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取节点类型的图标和颜色
        /// </summary>
        private (string Icon, Vector4 Color) GetNodeIcon(Node node)
        {
            var typeName = node.GetType().Name;
            if (NodeTypeIcons.TryGetValue(typeName, out var info))
                return info;
            return ("[?]", new Vector4(0.6f, 0.6f, 0.6f, 1f));
        }

        public void Render(ref Node? selectedNode)
        {
            ImGui.Begin("Hierarchy");

            // === 工具栏区域 ===
            RenderToolbar(ref selectedNode);

            ImGui.Separator();

            // === 可滚动的节点树区域 ===
            ImGui.BeginChild("HierarchyTree", new Vector2(0, -1), ImGuiChildFlags.None);

            // 同步选中节点展开
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
                bool isSearching = !string.IsNullOrWhiteSpace(_searchQuery);
                RenderNode(_sceneTree.Root, ref selectedNode, isSearching);
            }
            else
            {
                // 空场景引导
                RenderEmptyState(ref selectedNode);
            }

            // 空白区域右键菜单
            if (ImGui.BeginPopupContextWindow("", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            {
                if (ImGui.BeginMenu("Create"))
                {
                    RenderNodeCreationMenu(selectedNode ?? _sceneTree.Root, ref selectedNode);
                    ImGui.EndMenu();
                }
                ImGui.EndPopup();
            }

            ImGui.EndChild();

            // 延迟删除
            while (_nodesToDelete.Count > 0)
            {
                var nodeToDelete = _nodesToDelete.Dequeue();
                if (nodeToDelete.Parent != null)
                {
                    nodeToDelete.Parent.RemoveChild(nodeToDelete);
                }
                else if (nodeToDelete == _sceneTree.Root)
                {
                    _sceneTree.ChangeScene(null);
                    _expandedNodes.Clear();
                }
                _hiddenNodes.Remove(nodeToDelete);
                _lockedNodes.Remove(nodeToDelete);
            }

            ImGui.End();
        }

        /// <summary>
        /// 渲染工具栏（搜索框 + 操作按钮）
        /// </summary>
        private void RenderToolbar(ref Node? selectedNode)
        {
            // 搜索框
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 32);
            var prevQuery = _searchQuery;
            if (ImGui.InputTextWithHint("##HierarchySearch", "Search nodes...", ref _searchQuery, 256))
            {
                if (_searchQuery != prevQuery)
                    UpdateSearch();
            }

            ImGui.SameLine();

            // 添加节点按钮
            if (ImGui.Button("+", new Vector2(24, 0)))
            {
                ImGui.OpenPopup("AddNodePopup");
            }

            if (ImGui.BeginPopup("AddNodePopup"))
            {
                RenderNodeCreationMenu(selectedNode ?? _sceneTree.Root, ref selectedNode);
                ImGui.EndPopup();
            }
        }

        /// <summary>
        /// 空场景时的引导提示
        /// </summary>
        private void RenderEmptyState(ref Node? selectedNode)
        {
            ImGui.Dummy(new Vector2(0, 20));

            var emptyMsg = "Scene is empty";
            var textSize = ImGui.CalcTextSize(emptyMsg);
            float textIndent = (ImGui.GetContentRegionAvail().X - textSize.X) * 0.5f;
            if (textIndent > 0) ImGui.Indent(textIndent);
            ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), emptyMsg);
            if (textIndent > 0) ImGui.Unindent(textIndent);

            ImGui.Spacing();

            var btnText = "Create Root Node";
            var btnSize = ImGui.CalcTextSize(btnText);
            float btnIndent = (ImGui.GetContentRegionAvail().X - btnSize.X - 20) * 0.5f;
            if (btnIndent > 0) ImGui.Indent(btnIndent);
            if (ImGui.Button(btnText))
            {
                _sceneTree.ChangeScene(new Node2D("Root"));
            }
            if (btnIndent > 0) ImGui.Unindent(btnIndent);
        }

        private void RenderNode(Node node, ref Node? selectedNode, bool isSearching)
        {
            // 搜索过滤：不在匹配集中的节点跳过
            if (isSearching && !_searchMatchNodes.Contains(node))
                return;

            ImGui.PushID(node.GetHashCode());

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
            if (selectedNode == node)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (node.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;
            if (_expandedNodes.Contains(node))
                flags |= ImGuiTreeNodeFlags.DefaultOpen;
            // 搜索时默认全部展开
            if (isSearching)
                flags |= ImGuiTreeNodeFlags.DefaultOpen;

            // --- 节点行渲染 ---
            // 判断是否隐藏/锁定，调整透明度
            bool isHidden = _hiddenNodes.Contains(node);
            bool isLocked = _lockedNodes.Contains(node);
            if (isHidden)
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.4f);

            // 图标 + 名称
            var (icon, iconColor) = GetNodeIcon(node);
            ImGui.TextColored(iconColor, icon);
            ImGui.SameLine(0, 4);

            bool isOpen = ImGui.TreeNodeEx(node.Name, flags);

            if (isHidden)
                ImGui.PopStyleVar();

            // 展开状态跟踪
            if (isOpen)
                _expandedNodes.Add(node);
            else
                _expandedNodes.Remove(node);

            // 点击选中
            if (ImGui.IsItemClicked() && !isLocked)
                selectedNode = node;

            // --- 拖拽源 ---
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
            {
                _dragSourceNode = node;
                unsafe
                {
                    var hash = node.GetHashCode();
                    ImGui.SetDragDropPayload("HIERARCHY_NODE", new IntPtr(&hash), sizeof(int));
                }
                ImGui.Text($"Move: {node.Name}");
                ImGui.EndDragDropSource();
            }

            // --- 拖拽目标 ---
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("HIERARCHY_NODE");
                unsafe
                {
                    if (payload.NativePtr != null && _dragSourceNode != null)
                    {
                        // 防止循环：不能把父节点拖到自己的子节点下
                        if (!IsDescendantOf(_dragSourceNode, node) && _dragSourceNode != node)
                        {
                            // 从旧父节点移除
                            _dragSourceNode.Parent?.RemoveChild(_dragSourceNode);
                            // 添加到新父节点
                            node.AddChild(_dragSourceNode);
                            _expandedNodes.Add(node);
                            _dragSourceNode = null;
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }

            // --- 行尾的可见性/锁定按钮 ---
            RenderNodeButtons(node);

            // --- 右键菜单 ---
            if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                ImGui.OpenPopup($"##ctx_{node.GetHashCode()}");

            if (ImGui.BeginPopup($"##ctx_{node.GetHashCode()}"))
            {
                if (ImGui.BeginMenu("Create"))
                {
                    RenderNodeCreationMenu(node, ref selectedNode);
                    ImGui.EndMenu();
                }
                ImGui.Separator();

                if (ImGui.MenuItem("Delete"))
                {
                    _nodesToDelete.Enqueue(node);
                    RemoveNodeFromExpandedSet(node);
                    if (selectedNode == node)
                        selectedNode = null;
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("Copy"))
                {
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("Rename"))
                {
                    // TODO: 内联重命名
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (isOpen)
            {
                foreach (var child in node.Children)
                    RenderNode(child, ref selectedNode, isSearching);
                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        /// <summary>
        /// 在节点行末尾渲染可见性和锁定按钮
        /// </summary>
        private void RenderNodeButtons(Node node)
        {
            // 计算按钮位置（行末，相对于窗口内容区域）
            float buttonWidth = 18;
            float availWidth = ImGui.GetContentRegionAvail().X;
            // SameLine 参数是相对于窗口内容区域左边的偏移
            float xOffset = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X - buttonWidth * 2 - 8;

            ImGui.SameLine(xOffset);

            // 可见性按钮
            bool isHidden = _hiddenNodes.Contains(node);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 1, 1, 0.1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 1, 1, 0.15f));

            var eyeColor = isHidden ? ImGuiStyleManager.GetTextDisabledColor() : ImGuiStyleManager.GetTextColor();
            ImGui.PushStyleColor(ImGuiCol.Text, eyeColor);
            if (ImGui.SmallButton(isHidden ? "x##vis" : "o##vis"))
            {
                if (isHidden)
                    _hiddenNodes.Remove(node);
                else
                    _hiddenNodes.Add(node);
            }
            ImGui.PopStyleColor();

            ImGui.SameLine(0, 2);

            // 锁定按钮
            bool isLocked = _lockedNodes.Contains(node);
            var lockColor = isLocked ? new Vector4(1f, 0.6f, 0.2f, 1f) : ImGuiStyleManager.GetTextDisabledColor();
            ImGui.PushStyleColor(ImGuiCol.Text, lockColor);
            if (ImGui.SmallButton(isLocked ? "L##lck" : "-##lck"))
            {
                if (isLocked)
                    _lockedNodes.Remove(node);
                else
                    _lockedNodes.Add(node);
            }
            ImGui.PopStyleColor();
            ImGui.PopStyleColor(3);
        }

        /// <summary>
        /// 检查 potentialChild 是否是 node 的后代
        /// </summary>
        private bool IsDescendantOf(Node potentialParent, Node potentialChild)
        {
            var current = potentialChild;
            while (current != null)
            {
                if (current == potentialParent)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        private void RenderNodeCreationMenu(Node? parentNode, ref Node? selectedNode)
        {
            if (_nodeTypeRegistry == null)
            {
                ImGui.TextDisabled("Node type registry not initialized");
                return;
            }

            var nodeTypesByCategory = _nodeTypeRegistry.GetNodeTypesByCategory();

            if (nodeTypesByCategory.Count == 0 || nodeTypesByCategory.All(kvp => kvp.Value.Count == 0))
            {
                ImGui.TextDisabled("No node types found");
                ImGui.Separator();
                if (ImGui.MenuItem("Refresh"))
                {
                    _nodeTypeRegistry.MarkDirty();
                    _nodeTypeRegistry.DiscoverNodeTypes();
                }
                return;
            }

            // Core 节点优先
            if (nodeTypesByCategory.TryGetValue("Core", out var coreNodes))
            {
                foreach (var nodeTypeInfo in coreNodes)
                {
                    // 显示带图标的菜单项
                    var typeName = nodeTypeInfo.DisplayName;
                    if (NodeTypeIcons.TryGetValue(typeName, out var iconInfo))
                    {
                        ImGui.TextColored(iconInfo.Color, iconInfo.Icon);
                        ImGui.SameLine(0, 4);
                    }
                    if (ImGui.MenuItem(typeName))
                        CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                }

                if (coreNodes.Count > 0 && nodeTypesByCategory.Count > 1)
                    ImGui.Separator();
            }

            foreach (var category in nodeTypesByCategory.Keys)
            {
                if (category == "Core") continue;
                var nodeTypes = nodeTypesByCategory[category];
                if (nodeTypes.Count == 0) continue;

                if (!string.IsNullOrEmpty(category) && category != "Other")
                {
                    if (ImGui.BeginMenu(category))
                    {
                        foreach (var nodeTypeInfo in nodeTypes)
                        {
                            if (ImGui.MenuItem(nodeTypeInfo.DisplayName))
                                CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                        }
                        ImGui.EndMenu();
                    }
                }
                else
                {
                    foreach (var nodeTypeInfo in nodeTypes)
                    {
                        if (ImGui.MenuItem(nodeTypeInfo.DisplayName))
                            CreateNodeFromType(nodeTypeInfo, parentNode, ref selectedNode);
                    }
                }
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Refresh"))
            {
                _nodeTypeRegistry.MarkDirty();
                _nodeTypeRegistry.DiscoverNodeTypes();
            }
        }

        private void CreateNodeFromType(NodeTypeInfo nodeTypeInfo, Node? parentNode, ref Node? selectedNode)
        {
            if (_nodeTypeRegistry == null) return;

            var baseName = $"New{nodeTypeInfo.DisplayName}";
            var name = GenerateUniqueNodeName(parentNode, baseName);
            var newNode = _nodeTypeRegistry.CreateNode(nodeTypeInfo.TypeName, name);

            if (newNode != null)
            {
                CreateNodeInternal(newNode, parentNode, ref selectedNode);
                if (newNode is Camera2D camera && parentNode == _sceneTree.Root)
                    _sceneTree.SetCurrentCamera(camera);
            }
        }

        private void CreateNodeInternal(Node newNode, Node? parentNode, ref Node? selectedNode)
        {
            if (parentNode != null)
            {
                parentNode.AddChild(newNode);
                _expandedNodes.Add(parentNode);
            }
            else
            {
                _sceneTree.ChangeScene(newNode);
            }
            selectedNode = newNode;
        }
    }
}
