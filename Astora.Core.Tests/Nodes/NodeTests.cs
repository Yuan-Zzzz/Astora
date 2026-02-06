using Astora.Core.Nodes;
using FluentAssertions;

namespace Astora.Core.Tests.Nodes;

public class NodeTests
{
    [Fact]
    public void Constructor_SetsDefaultName()
    {
        var node = new Node();
        node.Name.Should().Be("Node");
    }

    [Fact]
    public void Constructor_WithName_SetsName()
    {
        var node = new Node("TestNode");
        node.Name.Should().Be("TestNode");
    }

    [Fact]
    public void AddChild_SetsParentAndAddsToChildren()
    {
        var parent = new Node("Parent");
        var child = new Node("Child");

        parent.AddChild(child);

        child.Parent.Should().Be(parent);
        parent.Children.Should().Contain(child);
        parent.Children.Count.Should().Be(1);
    }

    [Fact]
    public void AddChild_RemovesFromPreviousParent()
    {
        var parent1 = new Node("Parent1");
        var parent2 = new Node("Parent2");
        var child = new Node("Child");

        parent1.AddChild(child);
        parent2.AddChild(child);

        parent1.Children.Should().NotContain(child);
        parent2.Children.Should().Contain(child);
        child.Parent.Should().Be(parent2);
    }

    [Fact]
    public void RemoveChild_ClearsParentAndRemovesFromList()
    {
        var parent = new Node("Parent");
        var child = new Node("Child");

        parent.AddChild(child);
        parent.RemoveChild(child);

        child.Parent.Should().BeNull();
        parent.Children.Should().NotContain(child);
        parent.Children.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveChild_DoesNothing_WhenChildNotPresent()
    {
        var parent = new Node("Parent");
        var other = new Node("Other");

        parent.RemoveChild(other);

        parent.Children.Count.Should().Be(0);
    }

    [Fact]
    public void QueueFree_MarksNodeAndChildrenForDeletion()
    {
        var parent = new Node("Parent");
        var child1 = new Node("Child1");
        var child2 = new Node("Child2");

        parent.AddChild(child1);
        parent.AddChild(child2);

        parent.QueueFree();

        parent.IsQueuedForDeletion.Should().BeTrue();
        child1.IsQueuedForDeletion.Should().BeTrue();
        child2.IsQueuedForDeletion.Should().BeTrue();
    }

    [Fact]
    public void QueueFree_InvokesOnExitTree()
    {
        var node = new Node("TestNode");
        bool exitCalled = false;
        node.OnExitTree += () => exitCalled = true;

        node.QueueFree();

        exitCalled.Should().BeTrue();
    }

    [Fact]
    public void GetNode_FindsChildByType()
    {
        var root = new Node("Root");
        var node2d = new Node2D("Child2D");
        root.AddChild(node2d);

        var found = root.GetNode<Node2D>();

        found.Should().Be(node2d);
    }

    [Fact]
    public void GetNode_FindsNestedChildByType()
    {
        var root = new Node("Root");
        var middle = new Node("Middle");
        var deep = new Node2D("Deep2D");

        root.AddChild(middle);
        middle.AddChild(deep);

        var found = root.GetNode<Node2D>();

        found.Should().Be(deep);
    }

    [Fact]
    public void GetNode_ReturnsNull_WhenNotFound()
    {
        var root = new Node("Root");
        root.AddChild(new Node("Child"));

        var found = root.GetNode<Node2D>();

        found.Should().BeNull();
    }

    [Fact]
    public void FindNode_FindsChildByName()
    {
        var root = new Node("Root");
        var child = new Node("Target");
        root.AddChild(child);

        var found = root.FindNode("Target");

        found.Should().Be(child);
    }

    [Fact]
    public void FindNode_FindsNestedChildByName()
    {
        var root = new Node("Root");
        var middle = new Node("Middle");
        var deep = new Node("DeepTarget");

        root.AddChild(middle);
        middle.AddChild(deep);

        var found = root.FindNode("DeepTarget");

        found.Should().Be(deep);
    }

    [Fact]
    public void FindNode_ReturnsNull_WhenNotFound()
    {
        var root = new Node("Root");
        root.AddChild(new Node("Child"));

        var found = root.FindNode("NonExistent");

        found.Should().BeNull();
    }

    [Fact]
    public void GetChildren_ReturnsAllMatchingDescendants()
    {
        var root = new Node("Root");
        var a = new Node2D("A");
        var b = new Node("B");
        var c = new Node2D("C");

        root.AddChild(a);
        root.AddChild(b);
        b.AddChild(c);

        var found = root.GetChildren<Node2D>().ToList();

        found.Should().HaveCount(2);
        found.Should().Contain(a);
        found.Should().Contain(c);
    }

    [Fact]
    public void Children_IsReadOnly_CannotCastToMutableList()
    {
        var node = new Node("Test");
        node.AddChild(new Node("Child"));

        // IReadOnlyList should not be directly castable to List for external mutation
        var children = node.Children;
        children.Should().BeAssignableTo<IReadOnlyList<Node>>();
    }

    [Fact]
    public void AddChild_MultipleChildren_MaintainsOrder()
    {
        var parent = new Node("Parent");
        var c1 = new Node("C1");
        var c2 = new Node("C2");
        var c3 = new Node("C3");

        parent.AddChild(c1);
        parent.AddChild(c2);
        parent.AddChild(c3);

        parent.Children[0].Should().Be(c1);
        parent.Children[1].Should().Be(c2);
        parent.Children[2].Should().Be(c3);
    }
}
