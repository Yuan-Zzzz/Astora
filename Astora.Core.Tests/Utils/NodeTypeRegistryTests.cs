using Astora.Core.Nodes;
using Astora.Core.Utils;
using FluentAssertions;

namespace Astora.Core.Tests.Utils;

public class NodeTypeRegistryTests
{
    [Fact]
    public void DiscoverNodeTypes_FindsCoreNodes()
    {
        var registry = new NodeTypeRegistry();
        var types = registry.GetAvailableNodeTypes().ToList();

        types.Should().NotBeEmpty();

        var typeNames = types.Select(t => t.TypeName).ToList();
        typeNames.Should().Contain("Node2D");
        typeNames.Should().Contain("Camera2D");
        typeNames.Should().Contain("Sprite");
        typeNames.Should().Contain("CPUParticles2D");
        typeNames.Should().Contain("AnimatedSprite");
    }

    [Fact]
    public void CreateNode_ByTypeName_ReturnsCorrectInstance()
    {
        var registry = new NodeTypeRegistry();

        var node = registry.CreateNode("Node2D", "TestNode");

        node.Should().NotBeNull();
        node.Should().BeOfType<Node2D>();
        node!.Name.Should().Be("TestNode");
    }

    [Fact]
    public void CreateNode_UnknownType_ReturnsNull()
    {
        var registry = new NodeTypeRegistry();

        var node = registry.CreateNode("NonExistentNodeType", "Test");

        node.Should().BeNull();
    }

    [Fact]
    public void GetNodeTypeInfo_ReturnsInfoForKnownType()
    {
        var registry = new NodeTypeRegistry();

        var info = registry.GetNodeTypeInfo("Camera2D");

        info.Should().NotBeNull();
        info!.TypeName.Should().Be("Camera2D");
        info.Type.Should().Be(typeof(Camera2D));
    }

    [Fact]
    public void GetNodeTypeInfo_ReturnsNull_ForUnknownType()
    {
        var registry = new NodeTypeRegistry();

        var info = registry.GetNodeTypeInfo("FakeNode");

        info.Should().BeNull();
    }

    [Fact]
    public void GetNodeTypesByCategory_GroupsCorrectly()
    {
        var registry = new NodeTypeRegistry();
        var grouped = registry.GetNodeTypesByCategory();

        grouped.Should().NotBeEmpty();
        // Core nodes should be in a "Core" category
        grouped.Should().ContainKey("Core");
        grouped["Core"].Should().NotBeEmpty();
    }

    [Fact]
    public void MarkDirty_TriggersRediscovery()
    {
        var registry = new NodeTypeRegistry();
        
        // First discovery
        var types1 = registry.GetAvailableNodeTypes().ToList();
        
        registry.MarkDirty();
        
        // Second discovery after dirty
        var types2 = registry.GetAvailableNodeTypes().ToList();
        
        types2.Count.Should().Be(types1.Count);
    }
}
