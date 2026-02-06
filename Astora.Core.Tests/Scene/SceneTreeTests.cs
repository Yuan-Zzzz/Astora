using Astora.Core.Nodes;
using Astora.Core.Scene;
using FluentAssertions;

namespace Astora.Core.Tests.Scene;

public class SceneTreeTests
{
    [Fact]
    public void AttachScene_SetsRoot()
    {
        var tree = new SceneTree();
        var root = new Node("Root");

        tree.AttachScene(root);

        tree.Root.Should().Be(root);
    }

    [Fact]
    public void AttachScene_Null_ClearsRootAndCamera()
    {
        var tree = new SceneTree();
        tree.AttachScene(new Node("OldRoot"));

        tree.AttachScene((Node)null);

        tree.Root.Should().BeNull();
        tree.ActiveCamera.Should().BeNull();
    }

    [Fact]
    public void AttachScene_FindsCamera2D()
    {
        var tree = new SceneTree();
        var root = new Node("Root");
        var camera = new Camera2D("MainCamera");
        root.AddChild(camera);

        tree.AttachScene(root);

        tree.ActiveCamera.Should().Be(camera);
    }

    [Fact]
    public void AttachScene_NoCamera_ActiveCameraRemainsNull()
    {
        var tree = new SceneTree();
        var root = new Node("Root");
        root.AddChild(new Node2D("NotACamera"));

        tree.AttachScene(root);

        tree.ActiveCamera.Should().BeNull();
    }

    [Fact]
    public void GetNode_ReturnsCorrectType()
    {
        var tree = new SceneTree();
        var root = new Node("Root");
        var node2d = new Node2D("Child");
        root.AddChild(node2d);
        tree.AttachScene(root);

        var found = tree.GetNode<Node2D>();

        found.Should().Be(node2d);
    }

    [Fact]
    public void FindNode_ReturnsCorrectName()
    {
        var tree = new SceneTree();
        var root = new Node("Root");
        var target = new Node("Target");
        root.AddChild(target);
        tree.AttachScene(root);

        var found = tree.FindNode("Target");

        found.Should().Be(target);
    }

    [Fact]
    public void FindNode_ReturnsRootIfNameMatches()
    {
        var tree = new SceneTree();
        var root = new Node("MyRoot");
        tree.AttachScene(root);

        var found = tree.FindNode("MyRoot");

        found.Should().Be(root);
    }

    [Fact]
    public void SetCurrentCamera_UpdatesActiveCamera()
    {
        var tree = new SceneTree();
        var cam = new Camera2D("Cam");

        tree.SetCurrentCamera(cam);

        tree.ActiveCamera.Should().Be(cam);
    }

    [Fact]
    public void ChangeScene_IsAliasForAttachScene()
    {
        var tree = new SceneTree();
        var root = new Node("Root");

        tree.ChangeScene(root);

        tree.Root.Should().Be(root);
    }
}
