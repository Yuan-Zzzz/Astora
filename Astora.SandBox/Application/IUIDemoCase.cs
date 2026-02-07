using Astora.Core.Nodes;

namespace Astora.SandBox.Application;

/// <summary>
/// A UI demo case that builds a UI tree under the given scene root.
/// Implementations are responsible only for constructing the Control subtree.
/// </summary>
public interface IUIDemoCase
{
    /// <summary>Display name for the demo.</summary>
    string Name { get; }

    /// <summary>Builds the UI tree and attaches it to <paramref name="root"/>.</summary>
    void Build(Node root);
}
