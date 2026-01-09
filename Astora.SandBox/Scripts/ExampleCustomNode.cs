using Astora.Core;
using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.SandBox.Scripts
{
    /// <summary>
    /// 示例自定义节点 - 演示如何创建自定义节点类型
    /// 这个节点继承自 Node2D，具有位置、旋转和缩放功能
    /// </summary>
    public class ExampleCustomNode : Node2D
    {
       
       
        
        /// <summary>
        /// 自定义字段：旋转速度（弧度/秒）
        /// </summary>
        [SerializeField]
        private float _rotationSpeed = 1.0f;
        
        public ExampleCustomNode(string name = "ExampleCustomNode") : base(name)
        {
            // 初始化代码可以在这里
            
        }
        
        /// <summary>
        /// 节点准备就绪时调用
        /// </summary>
        public override void Ready()
        {
            base.Ready();
            // 可以在这里进行初始化
            System.Console.WriteLine($"ExampleCustomNode '{Name}' is readyyyyyyyy!");
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public override void Update(float delta)
        {
            base.Update(delta);
            //Rotation += _rotationSpeed * delta;
            // 可以在这里添加其他更新逻辑
        }
        
        /// <summary>
        /// 绘制节点
        /// </summary>
        public override void Draw(RenderBatcher renderBatcher)
        {
            base.Draw(renderBatcher);
        }
    }
}


