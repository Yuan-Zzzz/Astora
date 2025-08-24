using Astora.Core;
using Astora.Core.Time;
using Astora.ECS;
using Astora.Engine.Components;
using Astora.Engine.Core;
using Astora.Engine; // 新增

namespace Astora.Engine.Systems;

public struct ScriptSystem : ILogicSystem
{
    private readonly Astora.ECS.World _world;
    public int Order => 320; // 逻辑阶段

    public ScriptSystem(World world) => _world = world;

    public void TickLogic(ITime t)
    {
        foreach (var e in _world.Query<ScriptComponent>())
        {
            ref var nsc = ref _world.GetComponent<ScriptComponent>(e);

            if (nsc.Instance is null)
            {
                if (nsc.Instantiate is null) continue;
                nsc.Instance = nsc.Instantiate();
                nsc.Instance.Entity = new Entity(e, _world); // 赋值包装实体
                nsc.Instance._world = _world;
                nsc.Instance.OnCreate();
            }

            nsc.Instance?.OnUpdate(t);
        }
    }
}