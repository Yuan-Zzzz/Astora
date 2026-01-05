using Astora.Core;
using Astora.Core.Nodes;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Astora.Editor.UI
{
    public class InspectorPanel
    {
       public void Render(Node node)
        {
            ImGui.Begin("Inspector");
            
            if (node == null)
            {
                ImGui.Text("No node selected");
                ImGui.End();
                return;
            }
            
            // Basic properties
            ImGui.Text("Name:");
            ImGui.SameLine();
            var name = node.Name;
            if (ImGui.InputText("##Name", ref name, 256))
            {
                node.Name = name;
            }
            
            // If Node2D, show transform properties
            if (node is Node2D node2d)
            {
                ImGui.Separator();
                ImGui.Text("Transform");
                
                var pos = new Vector2(node2d.Position.X, node2d.Position.Y);
                if (ImGui.DragFloat2("Position", ref pos))
                {
                    node2d.Position = new Microsoft.Xna.Framework.Vector2(pos.X, pos.Y);
                }
                
                // Rotation
                var rot = MathHelper.ToDegrees(node2d.Rotation);
                if (ImGui.DragFloat("Rotation", ref rot))
                {
                    node2d.Rotation = MathHelper.ToRadians(rot);
                }
                
                
                var scale = new Vector2(node2d.Scale.X, node2d.Scale.Y);
                if (ImGui.DragFloat2("Scale", ref scale))
                {
                    node2d.Scale = new Microsoft.Xna.Framework.Vector2(scale.X, scale.Y);
                }
            }
            
            // If Sprite, show Sprite properties
            if (node is Sprite sprite)
            {
                ImGui.Separator();
                ImGui.Text("Sprite");
                
                var modulate = new Vector4(
                    sprite.Modulate.R / 255f,
                    sprite.Modulate.G / 255f,
                    sprite.Modulate.B / 255f,
                    sprite.Modulate.A / 255f
                );
                if (ImGui.ColorEdit4("Color", ref modulate))
                {
                    sprite.Modulate = new Microsoft.Xna.Framework.Color(
                        (byte)(modulate.X * 255),
                        (byte)(modulate.Y * 255),
                        (byte)(modulate.Z * 255),
                        (byte)(modulate.W * 255)
                    );
                }
            }
            
            // If Camera2D, show camera properties
            if (node is Camera2D camera)
            {
                ImGui.Separator();
                ImGui.Text("Camera");
                
                var zoom = camera.Zoom;
                if (ImGui.DragFloat("Zoom", ref zoom, 0.1f))
                {
                    camera.Zoom = zoom;
                }
            }
            
            ImGui.End();
        }
    }
}