namespace Astora.Core.Rendering.RenderPipeline.RenderPass;

public interface IRenderPass
{
    string Name { get; }
    bool Enabled { get; set; }
    void Execute(RenderContext context);
}

public abstract class RenderPass : IRenderPass
{
    public string Name { get; protected set; }
    public bool Enabled { get; set; } = true;

    public RenderPass(string name)
    {
        Name = name;
    }

    public abstract void Execute(RenderContext context);
}