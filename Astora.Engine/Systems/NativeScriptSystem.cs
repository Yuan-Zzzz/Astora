using Astora.Core;
using Astora.Core.Time;
using Astora.ECS;
using Astora.Engine.Components;
using Astora.Engine.Core;

namespace Astora.Engine.Systems;

public struct NativeScriptSystem : ILogicSystem
{
    private readonly Astora.ECS.World _world;
    public int Order => 320; // 逻辑阶段

    public NativeScriptSystem(Astora.ECS.World world) => _world = world;

    public void TickLogic(ITime t)
    {
        foreach (var e in _world.Query<ScriptComponent>())
        {
            // 你的 ComponentPool<T> 支持 ref 返回，这里拿 ref 便于写回 Instance 等字段
            ref var nsc = ref _world.GetComponent<ScriptComponent>(e);

            if (nsc.Instance is null)
            {
                if (nsc.Instantiate is null) continue;
                nsc.Instance = nsc.Instantiate();
                nsc.Instance.Entity = e;
                nsc.Instance.World  = _world;
                nsc.Instance.OnCreate();
            }

            nsc.Instance!.OnUpdate(t);
        }
    }
}