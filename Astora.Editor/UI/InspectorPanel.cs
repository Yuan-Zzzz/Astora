using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Resources;
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
        
       public void Render(Node? node)
        {
            ImGui.Begin("Inspector");
            
            if (node == null)
            {
                ImGui.Spacing();
                ImGui.Spacing();
                var msg = "No node selected";
                var msgSize = ImGui.CalcTextSize(msg);
                float msgIndent = (ImGui.GetContentRegionAvail().X - msgSize.X) * 0.5f;
                if (msgIndent > 0) ImGui.Indent(msgIndent);
                ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), msg);
                if (msgIndent > 0) ImGui.Unindent(msgIndent);
                ImGui.End();
                return;
            }
            
            // 节点类型标题（带颜色标记）
            var typeName = node.GetType().Name;
            ImGui.TextColored(ImGuiStyleManager.GetAccentColor(), typeName);
            ImGui.SameLine();
            ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), $"({node.Name})");
            
            ImGui.Separator();
            ImGui.Spacing();
            
            // 自动渲染所有可序列化字段
            RenderSerializableFields(node);
            
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
        
        /// <summary>
        /// 计算自适应的预览大小，保持纹理的宽高比
        /// </summary>
        private Vector2 CalculateAdaptivePreviewSize(int textureWidth, int textureHeight, float maxSize = 256f, float minSize = 64f)
        {
            if (textureWidth <= 0 || textureHeight <= 0)
            {
                return new Vector2(128, 128); // 默认大小
            }
            
            float aspectRatio = (float)textureWidth / textureHeight;
            float previewWidth, previewHeight;
            
            // 根据宽高比计算预览尺寸
            if (aspectRatio > 1.0f)
            {
                // 宽图：宽度为基准
                previewWidth = Math.Min(maxSize, textureWidth);
                previewHeight = previewWidth / aspectRatio;
                
                // 确保高度不会太小
                if (previewHeight < minSize && textureHeight >= minSize)
                {
                    previewHeight = minSize;
                    previewWidth = previewHeight * aspectRatio;
                }
            }
            else
            {
                // 高图：高度为基准
                previewHeight = Math.Min(maxSize, textureHeight);
                previewWidth = previewHeight * aspectRatio;
                
                // 确保宽度不会太小
                if (previewWidth < minSize && textureWidth >= minSize)
                {
                    previewWidth = minSize;
                    previewHeight = previewWidth / aspectRatio;
                }
            }
            
            // 对于特别小的纹理，保持原始大小或适当放大
            if (textureWidth < minSize && textureHeight < minSize)
            {
                float scale = minSize / Math.Max(textureWidth, textureHeight);
                previewWidth = textureWidth * scale;
                previewHeight = textureHeight * scale;
            }
            
            return new Vector2(previewWidth, previewHeight);
        }
        
        private void LoadTextureForSprite(Sprite sprite, string filePath)
        {
            try
            {
                // 验证文件存在
                if (!File.Exists(filePath))
                {
                    System.Console.WriteLine($"文件不存在: {filePath}");
                    return;
                }
                
                // 获取文件的绝对路径
                var absolutePath = Path.GetFullPath(filePath);
                
                System.Console.WriteLine($"[InspectorPanel] 加载纹理: {Path.GetFileName(absolutePath)}");
                System.Console.WriteLine($"[InspectorPanel] 完整路径: {absolutePath}");
                
                // 使用 ResourceLoader 加载纹理资源（传递绝对路径）
                // ResourceLoader 会利用 Path.Combine 的特性：当第二个参数是绝对路径时，直接使用该路径
                try
                {
                    var textureResource = ResourceLoader.Load<Texture2DResource>(absolutePath);
                    if (textureResource != null && textureResource.Texture != null)
                    {
                        var texture = textureResource.Texture;
                        sprite.Texture = texture;
                        sprite.Origin = new Microsoft.Xna.Framework.Vector2(texture.Width / 2f, texture.Height / 2f);
                        
                        // 更新预览缓存
                        if (_imGuiRenderer != null && !_texturePreviewCache.ContainsKey(texture))
                        {
                            var previewId = _imGuiRenderer.BindTexture(texture);
                            _texturePreviewCache[texture] = previewId;
                        }
                        
                        System.Console.WriteLine($"✓ 成功加载纹理: {Path.GetFileName(absolutePath)} ({texture.Width}x{texture.Height})");
                    }
                    else
                    {
                        System.Console.WriteLine($"✗ 加载的纹理资源为空: {absolutePath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"✗ 加载纹理失败: {Path.GetFileName(absolutePath)}");
                    System.Console.WriteLine($"  错误详情: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Console.WriteLine($"  内部错误: {ex.InnerException.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ 处理纹理路径失败: {filePath}");
                System.Console.WriteLine($"  错误详情: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 从路径加载纹理到 Sprite（用于 Inspector 中的路径变更）
        /// </summary>
        private void LoadTextureForSpriteFromPath(Sprite sprite, string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
            {
                sprite.Texture = null;
                return;
            }
            
            try
            {
                // 确定加载路径
                string loadPath = texturePath;
                
                // 如果是相对路径，需要转换为绝对路径
                if (!Path.IsPathRooted(texturePath))
                {
                    if (Engine.Content != null && !string.IsNullOrEmpty(Engine.Content.RootDirectory))
                    {
                        var contentDir = Engine.Content.RootDirectory;
                        // 如果 RootDirectory 是相对路径，需要从项目根目录解析
                        if (!Path.IsPathRooted(contentDir))
                        {
                            if (_projectManager?.CurrentProject != null)
                            {
                                contentDir = Path.Combine(_projectManager.CurrentProject.ProjectRoot, contentDir);
                            }
                        }
                        
                        if (Directory.Exists(contentDir))
                        {
                            loadPath = Path.Combine(contentDir, texturePath);
                        }
                        else
                        {
                            System.Console.WriteLine($"Content 目录不存在: {contentDir}");
                            return;
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Engine.Content 未初始化");
                        return;
                    }
                }
                
                // 验证文件存在
                if (!File.Exists(loadPath))
                {
                    System.Console.WriteLine($"纹理文件不存在: {loadPath}");
                    return;
                }
                
                // 使用 ResourceLoader 加载纹理资源
                var textureResource = ResourceLoader.Load<Texture2DResource>(loadPath);
                if (textureResource != null && textureResource.Texture != null)
                {
                    var texture = textureResource.Texture;
                    sprite.Texture = texture;
                    sprite.Origin = new Microsoft.Xna.Framework.Vector2(texture.Width / 2f, texture.Height / 2f);
                    
                    // 更新预览缓存
                    if (_imGuiRenderer != null && !_texturePreviewCache.ContainsKey(texture))
                    {
                        var previewId = _imGuiRenderer.BindTexture(texture);
                        _texturePreviewCache[texture] = previewId;
                    }
                    
                    System.Console.WriteLine($"✓ 成功加载纹理到场景: {Path.GetFileName(loadPath)} ({texture.Width}x{texture.Height})");
                }
                else
                {
                    System.Console.WriteLine($"✗ 加载的纹理资源为空: {loadPath}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ 加载纹理失败: {texturePath}");
                System.Console.WriteLine($"  错误详情: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"  内部错误: {ex.InnerException.Message}");
                }
            }
        }
        
        private string? ConvertToContentPath(string filePath)
        {
            if (Engine.Content == null)
            {
                System.Console.WriteLine("Engine.Content 未初始化");
                return null;
            }
            
            // 使用 Engine.Content.RootDirectory 而不是硬编码 "Content"
            var contentDir = Engine.Content.RootDirectory;
            if (string.IsNullOrEmpty(contentDir))
            {
                System.Console.WriteLine("Content.RootDirectory 为空");
                return null;
            }
            
            // 如果 RootDirectory 是相对路径，需要转换为绝对路径
            string contentFullPath;
            if (Path.IsPathRooted(contentDir))
            {
                contentFullPath = Path.GetFullPath(contentDir);
            }
            else
            {
                // 如果是相对路径，尝试从项目根目录解析
                if (_projectManager?.CurrentProject != null)
                {
                    var projectRoot = _projectManager.CurrentProject.ProjectRoot;
                    contentFullPath = Path.GetFullPath(Path.Combine(projectRoot, contentDir));
                }
                else
                {
                    contentFullPath = Path.GetFullPath(contentDir);
                }
            }
            
            if (!Directory.Exists(contentFullPath))
            {
                System.Console.WriteLine($"Content 目录不存在: {contentFullPath}");
                return null;
            }
            
            // 检查文件是否在Content目录中
            var fullPath = Path.GetFullPath(filePath);
            
            if (!fullPath.StartsWith(contentFullPath, StringComparison.OrdinalIgnoreCase))
            {
                // 文件不在Content目录中
                System.Console.WriteLine($"文件不在Content目录中: {filePath}");
                System.Console.WriteLine($"Content目录: {contentFullPath}");
                return null;
            }
            
            // 转换为相对路径（相对于Content目录）
            var relativePath = Path.GetRelativePath(contentFullPath, fullPath);
            
            // 保留文件扩展名（ResourceLoader 和 Texture2DImporter 需要扩展名来识别文件类型）
            // 规范化路径分隔符（使用/而不是\）
            return relativePath.Replace('\\', '/');
        }
        
        /// <summary>
        /// 渲染节点的所有可序列化字段（Unity风格：公共字段 + 标记了 [SerializeField] 的私有字段）
        /// </summary>
        private void RenderSerializableFields(Node node)
        {
            // 与 YamlSceneSerializer 相同的忽略字段列表
            var ignoredFields = new HashSet<string>
            {
                "_parent",
                "_children",
                "_isQueuedForDeletion",
                "_defaultWhiteTexture",
                "_texture",          // Sprite runtime field
                "_effect",           // Sprite runtime field
                "_blendState"        // Sprite runtime field
            };
            
            // 收集所有类型的字段（从基类到派生类）
            var nodeType = node.GetType();
            var typeHierarchy = new List<Type>();
            var currentType = nodeType;
            
            // 构建类型层次结构（从派生类到基类）
            while (currentType != null && currentType != typeof(object))
            {
                typeHierarchy.Add(currentType);
                currentType = currentType.BaseType;
            }
            
            // 反转，使其从基类到派生类
            typeHierarchy.Reverse();
            
            // 按类型层次遍历并显示字段
            foreach (var declaringType in typeHierarchy)
            {
                var fields = declaringType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !ignoredFields.Contains(f.Name) && !f.IsStatic)
                    .ToList();
                
                var displayableFields = new List<FieldInfo>();
                
                foreach (var field in fields)
                {
                    // Unity风格规则：公共字段 或 标记了 [SerializeField] 的私有字段
                    bool shouldDisplay = field.IsPublic || field.IsDefined(typeof(SerializeFieldAttribute), false);
                    
                    if (!shouldDisplay)
                        continue;
                    
                    var fieldType = field.FieldType;
                    
                    // 检查是否是支持的类型
                    if (IsDisplayableType(fieldType))
                    {
                        displayableFields.Add(field);
                    }
                }
                
                // 如果有可显示的字段，使用 CollapsingHeader 分组
                if (displayableFields.Count > 0)
                {
                    ImGui.Spacing();
                    if (ImGui.CollapsingHeader(declaringType.Name, ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Indent(4);
                        foreach (var field in displayableFields)
                        {
                            RenderField(node, field);
                        }
                        ImGui.Unindent(4);
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查类型是否可以在Inspector中显示
        /// </summary>
        private bool IsDisplayableType(Type fieldType)
        {
            if (fieldType == typeof(Microsoft.Xna.Framework.Vector2))
                return true;
            
            if (fieldType == typeof(Color))
                return true;
                
            if (fieldType == typeof(Rectangle) || fieldType == typeof(Rectangle?))
                return true;
            
            // Don't display runtime objects
            if (fieldType == typeof(Texture2D) || 
                fieldType == typeof(Effect) || 
                fieldType == typeof(BlendState))
                return false;
            
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
        /// 判断字段是否为图片路径字段
        /// </summary>
        private bool IsTexturePathField(FieldInfo field, string? fieldValue)
        {
            // 检查字段名是否包含 "texture"（不区分大小写）
            if (field.Name.Contains("texture", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // 如果字段值非空，检查是否为有效的图片文件路径
            if (!string.IsNullOrEmpty(fieldValue))
            {
                var extension = Path.GetExtension(fieldValue).ToLower();
                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tga", ".dds" };
                if (imageExtensions.Contains(extension))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 渲染图片路径字段（包括预览和拖拽功能）
        /// </summary>
        private void RenderTexturePathField(Node node, FieldInfo field, string fieldName)
        {
            var currentValue = (string?)field.GetValue(node) ?? string.Empty;
            var buffer = currentValue.ToCharArray();
            Array.Resize(ref buffer, 256);
            var newString = new string(buffer);
            
            // 渲染输入框
            bool pathChanged = false;
            if (ImGui.InputText(fieldName, ref newString, 256))
            {
                var trimmedValue = newString.TrimEnd('\0');
                if (trimmedValue != currentValue)
                {
                    field.SetValue(node, trimmedValue);
                    currentValue = trimmedValue;
                    pathChanged = true;
                }
            }
            
            // 设置拖拽目标（覆盖整个字段区域）
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("TEXTURE_FILE_PATH");
                // 检查 payload 是否有效（Data 不为 IntPtr.Zero）
                unsafe
                {
                    if (payload.NativePtr != null && payload.DataSize > 0)
                    {
                        var pathBytes = new byte[payload.DataSize];
                        System.Runtime.InteropServices.Marshal.Copy(payload.Data, pathBytes, 0, (int)payload.DataSize);
                        var draggedPath = System.Text.Encoding.UTF8.GetString(pathBytes);
                        
                        // 转换为相对于 Content 目录的路径
                        var contentPath = ConvertToContentPath(draggedPath);
                        string? newPath = null;
                        if (contentPath != null)
                        {
                            newPath = contentPath;
                        }
                        else
                        {
                            // 如果转换失败，尝试使用绝对路径
                            // 但首先检查文件是否存在
                            if (File.Exists(draggedPath))
                            {
                                newPath = draggedPath;
                            }
                        }
                        
                        if (newPath != null && newPath != currentValue)
                        {
                            field.SetValue(node, newPath);
                            currentValue = newPath;
                            pathChanged = true;
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
            
            // 如果路径发生变化，且节点是 Sprite，需要加载纹理并更新场景
            if (pathChanged && node is Sprite sprite)
            {
                LoadTextureForSpriteFromPath(sprite, currentValue);
            }
            
            // 如果路径不为空，尝试加载并显示预览
            if (!string.IsNullOrEmpty(currentValue))
            {
                Texture2D? texture = null;
                string? errorMessage = null;
                
                try
                {
                    // 尝试加载纹理
                    string loadPath = currentValue;
                    
                    // 如果是相对路径，需要转换为绝对路径
                    if (!Path.IsPathRooted(currentValue))
                    {
                        if (Engine.Content != null && !string.IsNullOrEmpty(Engine.Content.RootDirectory))
                        {
                            var contentDir = Engine.Content.RootDirectory;
                            // 如果 RootDirectory 是相对路径，需要从项目根目录解析
                            if (!Path.IsPathRooted(contentDir))
                            {
                                if (_projectManager?.CurrentProject != null)
                                {
                                    contentDir = Path.Combine(_projectManager.CurrentProject.ProjectRoot, contentDir);
                                }
                            }
                            
                            if (Directory.Exists(contentDir))
                            {
                                loadPath = Path.Combine(contentDir, currentValue);
                            }
                            else
                            {
                                errorMessage = "Content 目录不存在";
                            }
                        }
                        else
                        {
                            errorMessage = "Engine.Content 未初始化";
                        }
                    }
                    
                    // 确保路径存在
                    if (errorMessage == null)
                    {
                        if (File.Exists(loadPath))
                        {
                            // ResourceLoader 可以处理绝对路径（Path.Combine 会忽略第一个参数）
                            var textureResource = ResourceLoader.Load<Texture2DResource>(loadPath);
                            if (textureResource != null && textureResource.Texture != null)
                            {
                                texture = textureResource.Texture;
                            }
                            else
                            {
                                errorMessage = "纹理资源加载失败";
                            }
                        }
                        else
                        {
                            errorMessage = $"文件不存在: {loadPath}";
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    errorMessage = $"文件未找到: {Path.GetFileName(ex.FileName)}";
                }
                catch (Exception ex)
                {
                    // 加载失败，显示错误信息
                    errorMessage = $"无法加载纹理: {ex.Message}";
                }
                
                // 显示预览
                if (texture != null && _imGuiRenderer != null)
                {
                    var previewId = GetTexturePreviewId(texture);
                    if (previewId != IntPtr.Zero)
                    {
                        var previewSize = CalculateAdaptivePreviewSize(texture.Width, texture.Height);
                        ImGui.Image(previewId, previewSize);
                        
                        // 显示纹理信息
                        ImGui.Text($"尺寸: {texture.Width} x {texture.Height}");
                    }
                    else
                    {
                        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "无法创建预览");
                    }
                }
                else if (errorMessage != null)
                {
                    // 显示错误提示
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), errorMessage);
                }
                else
                {
                    // 显示占位符
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "无法加载预览");
                }
            }
            else
            {
                // 路径为空时显示提示
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "拖拽图片文件到此处或输入路径");
            }
        }
        
        /// <summary>
        /// 渲染单个字段
        /// </summary>
        private void RenderField(Node node, FieldInfo field)
        {
            var value = field.GetValue(node);
            var fieldType = field.FieldType;
            var fieldName = GetDisplayName(field.Name);
            
            // Special handling for rotation (convert to degrees)
            if (field.Name == "_rotation" && fieldType == typeof(float))
            {
                var rotationRad = (float)value;
                var rotationDeg = MathHelper.ToDegrees(rotationRad);
                if (ImGui.DragFloat(fieldName, ref rotationDeg))
                {
                    field.SetValue(node, MathHelper.ToRadians(rotationDeg));
                }
                return;
            }
            
            // 检查是否为图片路径字段
            if (fieldType == typeof(string))
            {
                var stringValue = (string?)value ?? string.Empty;
                if (IsTexturePathField(field, stringValue))
                {
                    RenderTexturePathField(node, field, fieldName);
                    return;
                }
            }
            
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
                if (ImGui.ColorEdit4(fieldName, ref colorVec, ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreviewHalf))
                {
                    field.SetValue(node, new Color(
                        (byte)(colorVec.X * 255),
                        (byte)(colorVec.Y * 255),
                        (byte)(colorVec.Z * 255),
                        (byte)(colorVec.W * 255)
                    ));
                }
            }
            else if (fieldType == typeof(Microsoft.Xna.Framework.Vector2))
            {
                var vec = (Microsoft.Xna.Framework.Vector2)value;
                var vec2 = new Vector2(vec.X, vec.Y);
                if (ImGui.DragFloat2(fieldName, ref vec2))
                {
                    field.SetValue(node, new Microsoft.Xna.Framework.Vector2(vec2.X, vec2.Y));
                }
            }
            else if (fieldType == typeof(Rectangle) || fieldType == typeof(Rectangle?))
            {
                if (value is Rectangle rect)
                {
                    ImGui.Text($"{fieldName}:");
                    
                    var x = rect.X;
                    var y = rect.Y;
                    var width = rect.Width;
                    var height = rect.Height;
                    
                    ImGui.Indent();
                    bool changed = false;
                    changed |= ImGui.DragInt("X", ref x);
                    changed |= ImGui.DragInt("Y", ref y);
                    changed |= ImGui.DragInt("Width", ref width);
                    changed |= ImGui.DragInt("Height", ref height);
                    ImGui.Unindent();
                    
                    if (changed)
                    {
                        field.SetValue(node, new Rectangle(x, y, width, height));
                    }
                }
                else
                {
                    ImGui.Text($"{fieldName}: None");
                    if (ImGui.Button($"Create##{fieldName}"))
                    {
                        field.SetValue(node, new Rectangle(0, 0, 100, 100));
                    }
                }
            }
        }
        
        /// <summary>
        /// 将字段名称转换为显示名称（移除下划线前缀，转换为友好格式）
        /// </summary>
        private string GetDisplayName(string fieldName)
        {
            // 移除下划线前缀
            if (fieldName.StartsWith("_"))
                fieldName = fieldName.Substring(1);
            
            // 首字母大写
            if (fieldName.Length > 0)
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            
            return fieldName;
        }
    }
}