using Astora.Core.Nodes;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.Nodes;

public class Node2DTests
{
    private const float Epsilon = 0.001f;

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var node = new Node2D("Test");

        node.Position.Should().Be(Vector2.Zero);
        node.Rotation.Should().Be(0f);
        node.Scale.Should().Be(Vector2.One);
    }

    [Fact]
    public void GlobalTransform_NoParent_EqualsLocalTransform()
    {
        var node = new Node2D("Test")
        {
            Position = new Vector2(100, 200),
            Scale = new Vector2(2, 2),
            Rotation = 0f
        };

        var transform = node.GlobalTransform;
        var translation = new Vector2(transform.Translation.X, transform.Translation.Y);

        translation.X.Should().BeApproximately(100f, Epsilon);
        translation.Y.Should().BeApproximately(200f, Epsilon);
    }

    [Fact]
    public void GlobalTransform_WithParent_CombinesTransforms()
    {
        var parent = new Node2D("Parent")
        {
            Position = new Vector2(100, 0)
        };
        var child = new Node2D("Child")
        {
            Position = new Vector2(50, 0)
        };

        parent.AddChild(child);

        var globalPos = child.GlobalPosition;

        globalPos.X.Should().BeApproximately(150f, Epsilon);
        globalPos.Y.Should().BeApproximately(0f, Epsilon);
    }

    [Fact]
    public void GlobalPosition_Get_ReturnsWorldPosition()
    {
        var parent = new Node2D("Parent") { Position = new Vector2(10, 20) };
        var child = new Node2D("Child") { Position = new Vector2(5, 5) };

        parent.AddChild(child);

        var globalPos = child.GlobalPosition;

        globalPos.X.Should().BeApproximately(15f, Epsilon);
        globalPos.Y.Should().BeApproximately(25f, Epsilon);
    }

    [Fact]
    public void GlobalPosition_Set_ConvertsToLocalSpace()
    {
        var parent = new Node2D("Parent") { Position = new Vector2(100, 100) };
        var child = new Node2D("Child");
        parent.AddChild(child);

        child.GlobalPosition = new Vector2(150, 200);

        child.Position.X.Should().BeApproximately(50f, Epsilon);
        child.Position.Y.Should().BeApproximately(100f, Epsilon);
    }

    [Fact]
    public void GlobalPosition_Set_NoParent_SetsLocalPosition()
    {
        var node = new Node2D("Test");

        node.GlobalPosition = new Vector2(42, 84);

        node.Position.X.Should().BeApproximately(42f, Epsilon);
        node.Position.Y.Should().BeApproximately(84f, Epsilon);
    }

    [Fact]
    public void GlobalTransform_WithScale_AppliesCorrectly()
    {
        var parent = new Node2D("Parent")
        {
            Position = new Vector2(0, 0),
            Scale = new Vector2(2, 2)
        };
        var child = new Node2D("Child")
        {
            Position = new Vector2(10, 0)
        };

        parent.AddChild(child);

        var globalPos = child.GlobalPosition;

        // child local (10,0) scaled by parent (2x) = (20,0)
        globalPos.X.Should().BeApproximately(20f, Epsilon);
        globalPos.Y.Should().BeApproximately(0f, Epsilon);
    }

    [Fact]
    public void GlobalTransform_ThreeLevelHierarchy()
    {
        var grandparent = new Node2D("GP") { Position = new Vector2(10, 0) };
        var parent = new Node2D("P") { Position = new Vector2(20, 0) };
        var child = new Node2D("C") { Position = new Vector2(30, 0) };

        grandparent.AddChild(parent);
        parent.AddChild(child);

        var globalPos = child.GlobalPosition;

        globalPos.X.Should().BeApproximately(60f, Epsilon);
        globalPos.Y.Should().BeApproximately(0f, Epsilon);
    }
}
