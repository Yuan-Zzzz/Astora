using Astora.Core.Nodes;

namespace Astora.Core.Scene;

public class SceneBuilder
{
    private Node _currentParent;
    private Node _rootNode;

    private SceneBuilder(Node rootNode)
    {
        _rootNode = rootNode;
        _currentParent = rootNode;
    }

    public static SceneBuilder Create<T>(string name) where T : Node, new()
    {
        var rootNode = new T()
        {
            Name = name 
        };
        return new SceneBuilder(rootNode);
    }

    public SceneBuilder AddChild<T>(string name, Action<SceneBuilder>? configure = null) where T : Node, new()
    {
       var nodeToAdd = new T()
       {
            Name = name
       };
       _currentParent.AddChild(nodeToAdd);

       if (configure != null)
       {
            var sceneBuilder = new SceneBuilder(nodeToAdd);
            configure(sceneBuilder);
       }

       return this;
    }

    public SceneBuilder Add<T>(string name, Action<T>? configure = null) where T : Node, new()
    {
        var nodeToAdd = new T() 
        {
            Name = name
        };
        configure?.Invoke(nodeToAdd);
        _currentParent.AddChild(nodeToAdd);
        return this;
    }

    /// <summary>
    /// Configure the current parent node's properties. Used inside AddChild callbacks
    /// to set properties on a branch node that also has children.
    /// </summary>
    public SceneBuilder Configure<T>(Action<T> configure) where T : Node
    {
        if (_currentParent is T typed)
            configure(typed);
        return this;
    }

    public Node Build()
    {
        return _rootNode; 
    }

    public void Save(string path)
    {
        Engine.Serializer.Save(_rootNode, path);
    }
}


