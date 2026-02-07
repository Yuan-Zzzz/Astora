using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: Label font rendering at multiple sizes.</summary>
public sealed class LabelFontSizesDemoCase : IUIDemoCase
{
    public string Name => "Label: Font sizes";

    public void Build(Node root)
    {
        LabelDemos.BuildFontSizes(root);
    }
}

/// <summary>Demo case: Button with Label child and click feedback.</summary>
public sealed class LabelButtonDemoCase : IUIDemoCase
{
    public string Name => "Label: Button with text";

    public void Build(Node root)
    {
        LabelDemos.BuildButtonWithLabel(root);
    }
}
