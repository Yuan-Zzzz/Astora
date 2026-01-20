using Astora.Core.Nodes;

public interface IScene
{
    public static abstract string ScenePath{get;}
    public static abstract Node Build();
}
