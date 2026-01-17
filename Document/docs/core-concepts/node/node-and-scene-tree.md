# Node and SceneTree
One of the core concepts is SceenTree, which is the hierarchy of nodes.
## What is Node?
Node is the most basic block in creating game entity.Everything is a node in scene.A node contains a Parent and a list of Children.It builds a tree structure.
## What is SceenTree?
A **Scene Tree** is the structure you build out of nodes. A scene has a single "root" node,and all other nodes are descendants of it.
This hierarchical structure is powerful:
- **Organization**: Group related nodes together. For example, a `Player` node might have a
      `Sprite` and a `Camera2D` as its children.
- **Relative Transformations**: A child node's position, rotation, and scale are relative toits parent. If you move the parent, all its children move with it, maintaining their relativpositions.
AttributeTargets.Field
