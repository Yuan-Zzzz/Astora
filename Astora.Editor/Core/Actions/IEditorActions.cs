using Astora.Core.Nodes;
using Astora.Editor.Project;

namespace Astora.Editor.Core.Actions;

/// <summary>
/// 编辑器动作门面：UI 不直接触碰宿主 Game，统一通过 Actions 调用业务能力。
/// </summary>
public interface IEditorActions
{
    bool LoadProject(string csprojPath);
    void CloseProject();

    bool RebuildProject();

    void LoadScene(SceneInfo sceneInfo);
    void SaveScene();
    void CreateNewScene(string sceneName);

    void SetPlaying(bool playing);

    Node? GetSelectedNode();
    void SetSelectedNode(Node? node);
}
