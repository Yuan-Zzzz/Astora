using Astora.Core.Nodes;

namespace Astora.Core.Scene;

/// <summary>
/// Creates node instances by type name. Used by scene serialization and editor to support custom node types.
/// </summary>
public interface INodeFactory
{
    /// <summary>
    /// Creates a node of the given type with the given name. Returns null if the type is not supported.
    /// </summary>
    Node? CreateNode(string typeName, string nodeName);
}
