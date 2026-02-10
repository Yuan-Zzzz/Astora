using Astora.Core.Nodes;
using Astora.Core.Scene;
using FluentAssertions;

namespace Astora.Core.Tests.Scene;

public class SceneBuilderTests
{
    [Fact]
    public void Create_BuildsRootNode()
    {
        var root = SceneBuilder.Create<Node>("Root").Build();

        root.Should().NotBeNull();
        root.Name.Should().Be("Root");
        root.Should().BeOfType<Node>();
    }

    [Fact]
    public void Create_WithNode2D_BuildsCorrectType()
    {
        var root = SceneBuilder.Create<Node2D>("Root2D").Build();

        root.Should().BeOfType<Node2D>();
        root.Name.Should().Be("Root2D");
    }

    [Fact]
    public void Add_AddsChildWithConfig()
    {
        var root = SceneBuilder.Create<Node>("Root")
            .Add<Node2D>("Child", n =>
            {
                n.Position = new Microsoft.Xna.Framework.Vector2(42, 84);
            })
            .Build();

        root.Children.Should().HaveCount(1);
        var child = root.Children[0] as Node2D;
        child.Should().NotBeNull();
        child!.Name.Should().Be("Child");
        child.Position.X.Should().Be(42f);
        child.Position.Y.Should().Be(84f);
    }

    [Fact]
    public void Add_MultipleChildren_AddsAll()
    {
        var root = SceneBuilder.Create<Node>("Root")
            .Add<Node>("A")
            .Add<Node>("B")
            .Add<Node>("C")
            .Build();

        root.Children.Should().HaveCount(3);
        root.Children[0].Name.Should().Be("A");
        root.Children[1].Name.Should().Be("B");
        root.Children[2].Name.Should().Be("C");
    }

    [Fact]
    public void Add_WithoutConfig_AddsChild()
    {
        var root = SceneBuilder.Create<Node>("Root")
            .Add<Node2D>("SimpleChild")
            .Build();

        root.Children.Should().HaveCount(1);
        root.Children[0].Name.Should().Be("SimpleChild");
    }

    [Fact]
    public void Configure_SetsPropertiesOnCurrentParent()
    {
        var root = SceneBuilder.Create<Node>("Root")
            .AddChild<Node2D>("Branch", b => b
                .Configure<Node2D>(n =>
                {
                    n.Position = new Microsoft.Xna.Framework.Vector2(10, 20);
                })
                .Add<Node>("Leaf")
            )
            .Build();

        root.Children.Should().HaveCount(1);
        var branch = root.Children[0] as Node2D;
        branch.Should().NotBeNull();
        branch!.Name.Should().Be("Branch");
        branch.Position.X.Should().Be(10f);
        branch.Position.Y.Should().Be(20f);
        branch.Children.Should().HaveCount(1);
        branch.Children[0].Name.Should().Be("Leaf");
    }
}
