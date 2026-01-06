using System;

namespace Astora.Core
{
    /// <summary>
    /// 标记字段需要序列化并在检查器中显示
    /// 类似于 Unity 的 [SerializeField] 特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeFieldAttribute : Attribute
    {
    }
}

