using Astora.Core.Nodes;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.Nodes;

public class Camera2DTests
{
    private const float Epsilon = 0.01f;

    private Camera2D CreateCamera(Vector2 position, float zoom = 1f, Vector2? origin = null)
    {
        var cam = new Camera2D("TestCam")
        {
            Position = position,
            Zoom = zoom,
            Origin = origin ?? Vector2.Zero
        };
        return cam;
    }

    [Fact]
    public void GetViewMatrix_DefaultValues_CentersOnOrigin()
    {
        var cam = CreateCamera(Vector2.Zero, 1f, Vector2.Zero);

        var matrix = cam.GetViewMatrix();

        // With position (0,0), zoom 1, origin (0,0), view matrix should be identity
        matrix.Should().Be(Matrix.Identity);
    }

    [Fact]
    public void GetViewMatrix_WithPosition_TranslatesCorrectly()
    {
        var cam = CreateCamera(new Vector2(100, 50), 1f, Vector2.Zero);

        var matrix = cam.GetViewMatrix();

        // A point at (100, 50) in world should map to (0, 0) on screen
        var result = Vector2.Transform(new Vector2(100, 50), matrix);
        result.X.Should().BeApproximately(0f, Epsilon);
        result.Y.Should().BeApproximately(0f, Epsilon);
    }

    [Fact]
    public void GetViewMatrix_WithZoom_ScalesCorrectly()
    {
        var cam = CreateCamera(Vector2.Zero, 2f, Vector2.Zero);

        var matrix = cam.GetViewMatrix();

        // With 2x zoom, a point at (50, 50) should appear at (100, 100)
        var result = Vector2.Transform(new Vector2(50, 50), matrix);
        result.X.Should().BeApproximately(100f, Epsilon);
        result.Y.Should().BeApproximately(100f, Epsilon);
    }

    [Fact]
    public void GetViewMatrix_WithOrigin_OffsetsCorrectly()
    {
        var cam = CreateCamera(Vector2.Zero, 1f, new Vector2(960, 540));

        var matrix = cam.GetViewMatrix();

        // Origin offset should shift everything: (0,0) world -> (960,540) screen
        var result = Vector2.Transform(Vector2.Zero, matrix);
        result.X.Should().BeApproximately(960f, Epsilon);
        result.Y.Should().BeApproximately(540f, Epsilon);
    }

    [Fact]
    public void ScreenToWorld_RoundTrip_ReturnsOriginal()
    {
        var cam = CreateCamera(new Vector2(200, 100), 1.5f, new Vector2(960, 540));

        var worldPoint = new Vector2(300, 400);
        var screenPoint = cam.WorldToScreen(worldPoint);
        var backToWorld = cam.ScreenToWorld(screenPoint);

        backToWorld.X.Should().BeApproximately(worldPoint.X, Epsilon);
        backToWorld.Y.Should().BeApproximately(worldPoint.Y, Epsilon);
    }

    [Fact]
    public void WorldToScreen_ScreenToWorld_AreInverse()
    {
        var cam = CreateCamera(new Vector2(50, -30), 2f, new Vector2(480, 270));

        var screenPoint = new Vector2(100, 200);
        var worldPoint = cam.ScreenToWorld(screenPoint);
        var backToScreen = cam.WorldToScreen(worldPoint);

        backToScreen.X.Should().BeApproximately(screenPoint.X, Epsilon);
        backToScreen.Y.Should().BeApproximately(screenPoint.Y, Epsilon);
    }

    [Fact]
    public void ScreenToWorld_WithZoom_UnscalesCorrectly()
    {
        var cam = CreateCamera(Vector2.Zero, 2f, Vector2.Zero);

        // Screen (100, 100) at 2x zoom should map to world (50, 50)
        var world = cam.ScreenToWorld(new Vector2(100, 100));

        world.X.Should().BeApproximately(50f, Epsilon);
        world.Y.Should().BeApproximately(50f, Epsilon);
    }
}
