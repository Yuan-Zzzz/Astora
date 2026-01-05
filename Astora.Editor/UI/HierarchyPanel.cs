using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using ImGuiNET;

namespace Astora.Editor.UI
{
    public class HierarchyPanel
    {
        private SceneTree _sceneTree;
        
        public HierarchyPanel(SceneTree sceneTree)
        {
            _sceneTree = sceneTree;
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
                    var newNode = new Node2D("NewNode2D");
                    if (_sceneTree.Root != null)
                        _sceneTree.Root.AddChild(newNode);
                    else
                        _sceneTree.ChangeScene(newNode);
                }
                if (ImGui.MenuItem("Create Sprite"))
                {
                    // Texture required, simplified handling here
                }
                ImGui.EndPopup();
            }
            
            ImGui.End();
        }
        
        private void RenderNode(Node node, ref Node selectedNode)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (selectedNode == node)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (node.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;
            
            bool isOpen = ImGui.TreeNodeEx(node.Name, flags);
            
            if (ImGui.IsItemClicked())
            {
                selectedNode = node;
            }
            
            // Right-click context menu
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Delete"))
                {
                    node.QueueFree();
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
        }
    }
}