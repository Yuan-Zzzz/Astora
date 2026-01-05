namespace Astora.Core.Utils;

public interface ISceneSerializer
{
    /// <summary>
    /// Save the node tree to the specified path
    /// </summary>
    void Save(Node rootNode, string path);

    /// <summary>
    /// Load the node tree from the specified path
    /// </summary>
    Node Load(string path);

    /// <summary>
    /// Get the file extension used by this serializer
    /// </summary>
    string GetExtension();
}