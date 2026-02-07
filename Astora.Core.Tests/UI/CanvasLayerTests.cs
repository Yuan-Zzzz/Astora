using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using FluentAssertions;

namespace Astora.Core.Tests.UI;

public class CanvasLayerTests
{
    [Fact]
    public void CanvasLayer_LayerProperty_StoresValue()
    {
        var layer = new CanvasLayer();
        layer.Layer.Should().Be(0);
        layer.Layer = 5;
        layer.Layer.Should().Be(5);
    }

    [Fact]
    public void CanvasLayer_DefaultName_IsCanvasLayer()
    {
        var layer = new CanvasLayer();
        layer.Name.Should().Be("CanvasLayer");
    }

    [Fact]
    public void GetUIRoots_IncludesDirectControlChildrenOfRoot_AsLayerZero()
    {
        var scene = new SceneTree();
        var root = new Node("Root");
        var uiRoot = new Control { Name = "UIRoot" };
        root.AddChild(uiRoot);
        scene.AttachScene(root);

        var roots = scene.GetUIRoots().ToList();
        roots.Should().HaveCount(1);
        roots[0].Layer.Should().Be(0);
        roots[0].Root.Should().Be(uiRoot);
    }

    [Fact]
    public void GetUIRoots_IncludesControlUnderCanvasLayer_WithCorrectLayer()
    {
        var scene = new SceneTree();
        var root = new Node("Root");
        var canvasLayer = new CanvasLayer { Layer = 2 };
        var uiRoot = new Control { Name = "Layer2Root" };
        canvasLayer.AddChild(uiRoot);
        root.AddChild(canvasLayer);
        scene.AttachScene(root);

        var roots = scene.GetUIRoots().ToList();
        roots.Should().HaveCount(1);
        roots[0].Layer.Should().Be(2);
        roots[0].Root.Should().Be(uiRoot);
    }

    [Fact]
    public void GetUIRoots_OrdersByLayerAscending()
    {
        var scene = new SceneTree();
        var root = new Node("Root");
        var c0 = new Control { Name = "Layer0" };
        root.AddChild(c0);
        var cl1 = new CanvasLayer { Layer = 1 };
        var c1 = new Control { Name = "Layer1" };
        cl1.AddChild(c1);
        root.AddChild(cl1);
        var clMinus = new CanvasLayer { Layer = -1 };
        var cMinus = new Control { Name = "LayerMinus" };
        clMinus.AddChild(cMinus);
        root.AddChild(clMinus);
        scene.AttachScene(root);

        var roots = scene.GetUIRoots().ToList();
        roots.Should().HaveCount(3);
        roots[0].Layer.Should().Be(-1);
        roots[1].Layer.Should().Be(0);
        roots[2].Layer.Should().Be(1);
    }
}
