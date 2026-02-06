using Astora.Core.Nodes;

namespace Astora.Core.Scene;

public interface IScene
{
    public static abstract string ScenePath { get; }
    public static abstract Node Build();
}
