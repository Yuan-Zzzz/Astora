using Astora.Core;
using Astora.Core.Nodes;
using Astora.Editor.Project;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using System.Reflection;
using Astora.Core.Attributes;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Astora.Editor.UI
{
    public class InspectorPanel
    {
        private readonly ProjectManager? _projectManager;
        private readonly ImGuiRenderer? _imGuiRenderer;
        
        // 纹理预览缓存
        private readonly Dictionary<Texture2D, IntPtr> _texturePreviewCache = new();
        
        public InspectorPanel(ProjectManager? projectManager = null, ImGuiRenderer? imGuiRenderer = null)
        {
            _projectManager = projectManager;
            _imGuiRenderer = imGuiRenderer;
        }
        
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
            
            // Display node type
            ImGui.Text("Type:");
            ImGui.SameLine();
            ImGui.Text(node.GetType().Name);
            
            ImGui.Separator();
            
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
                
                // 纹理显示和拖拽接收
                ImGui.Text("Texture:");
                ImGui.SameLine();
                
                // 显示当前纹理状态
                if (sprite.Texture != null)
                {
                    ImGui.TextColored(new Vector4(0, 1, 0, 1), $"Loaded ({sprite.Texture.Width}x{sprite.Texture.Height})");
                }
                else
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "None (Default: 64x64 white)");
                }
                
                // 纹理预览区域（可拖拽）
                var previewSize = new Vector2(128, 128);
                var texturePreviewId = sprite.Texture != null ? GetTexturePreviewId(sprite.Texture) : IntPtr.Zero;
                
                // 使用一个组来包装预览区域，使其成为拖拽目标
                ImGui.BeginGroup();
                
                if (texturePreviewId != IntPtr.Zero)
                {
                    ImGui.Image(texturePreviewId, previewSize);
                }
                else
                {
                    // 显示占位符
                    var drawList = ImGui.GetWindowDrawList();
                    var min = ImGui.GetCursorScreenPos();
                    var max = new System.Numerics.Vector2(min.X + previewSize.X, min.Y + previewSize.Y);
                    drawList.AddRectFilled(min, max, ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1)));
                    drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 1)));
                    ImGui.Dummy(previewSize);
                }
                
                // 拖拽接收区域（整个预览区域都可以接收拖拽）
                if (ImGui.BeginDragDropTarget())
                {
                    unsafe
                    {
                        var payload = ImGui.AcceptDragDropPayload("TEXTURE_FILE_PATH");
                        if (payload.NativePtr != (void*)IntPtr.Zero)
                        {
                            unsafe
                            {
                                var dataSize = (int)payload.DataSize;
                                if (dataSize > 0)
                                {
                                    var data = new byte[dataSize];
                                    System.Runtime.InteropServices.Marshal.Copy(payload.Data, data, 0, dataSize);
                                    var filePath = System.Text.Encoding.UTF8.GetString(data);
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        LoadTextureForSprite(sprite, filePath);
                                    }
                                }
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                }
                
                ImGui.EndGroup();
                
                // 清除纹理按钮
                if (sprite.Texture != null)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Clear"))
                    {
                        sprite.Texture = null!; // 允许设置为null，Sprite类会处理默认纹理
                        sprite.Origin = new Microsoft.Xna.Framework.Vector2(32, 32); // 默认64x64的中心
                    }
                }
                
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
            
            // Render serialized fields (properties that are not already displayed above)
            RenderSerializedFields(node);
            
            ImGui.End();
        }
        
        private IntPtr GetTexturePreviewId(Texture2D texture)
        {
            if (_imGuiRenderer == null) return IntPtr.Zero;
            
            if (!_texturePreviewCache.TryGetValue(texture, out var previewId))
            {
                previewId = _imGuiRenderer.BindTexture(texture);
                _texturePreviewCache[texture] = previewId;
            }
            
            return previewId;
        }
        
        private void LoadTextureForSprite(Sprite sprite, string filePath)
        {
            try
            {
                // 转换为Content相对路径
                var contentPath = ConvertToContentPath(filePath);
                if (string.IsNullOrEmpty(contentPath))
                {
                    System.Console.WriteLine($"无法找到Content目录: {filePath}");
                    return;
                }
                
                // 加载纹理
                if (Engine.Content != null)
                {
                    try
                    {
                        var texture = Engine.Content.Load<Texture2D>(contentPath);
                        sprite.Texture = texture;
                        sprite.Origin = new Microsoft.Xna.Framework.Vector2(texture.Width / 2f, texture.Height / 2f);
                        
                        // 更新预览缓存
                        if (_imGuiRenderer != null && !_texturePreviewCache.ContainsKey(texture))
                        {
                            var previewId = _imGuiRenderer.BindTexture(texture);
                            _texturePreviewCache[texture] = previewId;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"加载纹理失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"处理纹理路径失败: {ex.Message}");
            }
        }
        
        private string? ConvertToContentPath(string filePath)
        {
            if (_projectManager?.CurrentProject == null)
            {
                return null;
            }
            
            var projectRoot = _projectManager.CurrentProject.ProjectRoot;
            if (string.IsNullOrEmpty(projectRoot))
            {
                return null;
            }
            
            // 查找Content目录
            var contentDir = Path.Combine(projectRoot, "Content");
            if (!Directory.Exists(contentDir))
            {
                // 尝试在项目根目录的子目录中查找
                var possibleContentDirs = Directory.GetDirectories(projectRoot, "Content", SearchOption.TopDirectoryOnly);
                if (possibleContentDirs.Length > 0)
                {
                    contentDir = possibleContentDirs[0];
                }
                else
                {
                    return null;
                }
            }
            
            // 检查文件是否在Content目录中
            var fullPath = Path.GetFullPath(filePath);
            var contentFullPath = Path.GetFullPath(contentDir);
            
            if (!fullPath.StartsWith(contentFullPath, StringComparison.OrdinalIgnoreCase))
            {
                // 文件不在Content目录中，尝试复制或返回null
                System.Console.WriteLine($"文件不在Content目录中: {filePath}");
                return null;
            }
            
            // 转换为相对路径（相对于Content目录）
            var relativePath = Path.GetRelativePath(contentFullPath, fullPath);
            
            // 移除文件扩展名（MonoGame Content管道要求）
            var pathWithoutExtension = Path.ChangeExtension(relativePath, null);
            
            // 规范化路径分隔符（使用/而不是\）
            return pathWithoutExtension.Replace('\\', '/');
        }
        
        /// <summary>
        /// 渲染节点的序列化字段（只显示标记了 [SerializeField] 的字段）
        /// </summary>
        private void RenderSerializedFields(Node node)
        {
            // 与 YamlSceneSerializer 相同的忽略字段列表
            var ignoredFields = new HashSet<string>
            {
                "_parent",
                "_children",
                "_isQueuedForDeletion"
            };
            
            // 获取所有实例字段（包括私有和公共）
            var type = node.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !ignoredFields.Contains(f.Name));
            
            var serializedFields = new List<FieldInfo>();
            
            foreach (var field in fields)
            {
                // 只显示标记了 [SerializeField] 的字段
                if (!field.IsDefined(typeof(SerializeFieldAttribute), false))
                    continue;
                
                var value = field.GetValue(node);
                if (value == null) continue;
                
                var fieldType = field.FieldType;
                
                // 检查是否是支持的类型
                if (IsSerializableType(fieldType))
                {
                    serializedFields.Add(field);
                }
            }
            
            // 如果有序列化字段，显示它们
            if (serializedFields.Count > 0)
            {
                ImGui.Separator();
                ImGui.Text("Serialized Fields");
                
                foreach (var field in serializedFields)
                {
                    RenderField(node, field);
                }
            }
        }
        
        /// <summary>
        /// 检查类型是否可序列化（与 YamlSceneSerializer.SerializeField 的逻辑一致）
        /// </summary>
        private bool IsSerializableType(Type fieldType)
        {
            if (fieldType == typeof(Vector2))
                return true;
            
            if (fieldType == typeof(Color))
                return true;
            
            if (fieldType == typeof(Texture2D))
                return false; // Texture2D 不序列化
            
            if (fieldType.IsPrimitive ||
                fieldType == typeof(string) ||
                fieldType == typeof(float) ||
                fieldType == typeof(double) ||
                fieldType == typeof(int) ||
                fieldType == typeof(bool))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 渲染单个字段
        /// </summary>
        private void RenderField(Node node, FieldInfo field)
        {
            var value = field.GetValue(node);
            if (value == null) return;
            
            var fieldType = field.FieldType;
            var fieldName = field.Name;
            
            // 根据类型渲染不同的控件
            if (fieldType == typeof(float))
            {
                var floatValue = (float)value;
                if (ImGui.DragFloat(fieldName, ref floatValue))
                {
                    field.SetValue(node, floatValue);
                }
            }
            else if (fieldType == typeof(double))
            {
                var doubleValue = (double)value;
                var floatValue = (float)doubleValue;
                if (ImGui.DragFloat(fieldName, ref floatValue))
                {
                    field.SetValue(node, (double)floatValue);
                }
            }
            else if (fieldType == typeof(int))
            {
                var intValue = (int)value;
                if (ImGui.DragInt(fieldName, ref intValue))
                {
                    field.SetValue(node, intValue);
                }
            }
            else if (fieldType == typeof(bool))
            {
                var boolValue = (bool)value;
                if (ImGui.Checkbox(fieldName, ref boolValue))
                {
                    field.SetValue(node, boolValue);
                }
            }
            else if (fieldType == typeof(string))
            {
                var stringValue = (string)value ?? string.Empty;
                var buffer = stringValue.ToCharArray();
                Array.Resize(ref buffer, 256);
                var newString = new string(buffer);
                if (ImGui.InputText(fieldName, ref newString, 256))
                {
                    field.SetValue(node, newString.TrimEnd('\0'));
                }
            }
            else if (fieldType == typeof(Color))
            {
                var color = (Color)value;
                var colorVec = new Vector4(
                    color.R / 255f,
                    color.G / 255f,
                    color.B / 255f,
                    color.A / 255f
                );
                if (ImGui.ColorEdit4(fieldName, ref colorVec))
                {
                    field.SetValue(node, new Color(
                        (byte)(colorVec.X * 255),
                        (byte)(colorVec.Y * 255),
                        (byte)(colorVec.Z * 255),
                        (byte)(colorVec.W * 255)
                    ));
                }
            }
            else if (fieldType == typeof(Vector2))
            {
                var vec = (Microsoft.Xna.Framework.Vector2)value;
                var vec2 = new Vector2(vec.X, vec.Y);
                if (ImGui.DragFloat2(fieldName, ref vec2))
                {
                    field.SetValue(node, new Microsoft.Xna.Framework.Vector2(vec2.X, vec2.Y));
                }
            }
        }
    }
}