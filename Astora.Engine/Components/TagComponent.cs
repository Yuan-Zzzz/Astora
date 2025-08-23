namespace Astora.Engine.Components;

public struct TagComponent
{
    public string Tag;
    public TagComponent(string tag) => Tag = tag;
    public override string ToString() => Tag;
}