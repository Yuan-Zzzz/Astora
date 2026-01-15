# Code Style

## Overall Principles
- Keep code simple, readable, and maintainable.
- Prioritize API stability and backward compatibility.
- Program to interfaces/abstractions and minimize module coupling.

## Project Structure

- Core: Engine core (rendering, scenes, nodes, etc.)
- Editor: Visual editor (ImGui, tool panels, etc.)
- SandBox: Sample and development project
- Document: Documentation and site

## Naming Conventions

- Classes, interfaces, enums: PascalCase; interfaces prefixed with I (e.g. `ISceneSerializer`).
- Methods, properties, events: PascalCase.
- Fields:
  - Public fields: PascalCase.
  - Private fields: `_camelCase` (underscore prefix).
- Local variables and parameters: camelCase.
- Constants: PascalCase
- Names should express intent; avoid abbreviations and ambiguous names.

## Files and Namespaces

- Keep namespace and directory structure aligned, e.g. `Astora.Core.Nodes` corresponds to `Astora.Core/Nodes/`.
- Put each public type in its own file; file name should match the type name.

## Code Formatting

- Use .editorconfig or IDE default C# conventions (4-space indentation, UTF-8).
- Braces on new lines:
  - Place `{` on a new line for classes/methods/control statements.
- Use expression-bodied members only when they improve readability.
- Avoid excessively long lines (recommendation: no more than 120 columns).

## API Design

- Prefer immutability or minimal mutability.
- Write XML documentation comments for public APIs.

## Commit Guidelines

- Use imperative verbs in commit messages describing the change, for example: `Add SpriteFrames loader`.
- Link issues/PRs and provide a brief description of the change and motivation.

## Example Snippet

```csharp
namespace Astora.Core.Nodes
{
    public class Node
    {
        private int _updateOrder;

        public int UpdateOrder => _updateOrder;

        public void AddChild(Node child)
        {
            // ...
        }
    }
    
    public interface INode
    {

    }
}
```
