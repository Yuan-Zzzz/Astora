using System.Text.Json.Serialization;

namespace Astora.Engine.Scene;

/// <summary>场景文件的顶层结构（JSON）。</summary>
public sealed class SceneData
{
    [JsonPropertyName("version")] public int Version { get; set; } = 1;
    [JsonPropertyName("name")]    public string Name { get; set; } = "Untitled";
    /// <summary>渲染顺序（叠加场景时用，越小越先渲染）。</summary>
    [JsonPropertyName("renderOrder")] public int RenderOrder { get; set; } = 0;

    /// <summary>实体列表。</summary>
    [JsonPropertyName("entities")] public List<EntityData> Entities { get; set; } = new();
}

public sealed class EntityData
{
    /// <summary>场景内唯一ID（字符串，便于可视化编辑器）。</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    [JsonPropertyName("name")] public string? Name { get; set; }

    /// <summary>组件字典：键=组件名（如 "LocalTransform"），值=任意 JSON 对象。</summary>
    [JsonPropertyName("components")] public Dictionary<string, object> Components { get; set; } = new();
}