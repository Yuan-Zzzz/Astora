using Astora.Core;
using Astora.Core.Nodes;
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
        /// 自定义属性：速度
        /// </summary>
        public float Speed { get; set; } = 100.0f;
        
        /// <summary>
        /// 自定义属性：颜色
        /// </summary>
        public Color NodeColor { get; set; } = Color.Red;
        
        /// <summary>
        /// 自定义属性：是否自动旋转
        /// </summary>
        public bool AutoRotate { get; set; } = false;
        
        /// <summary>
        /// 自定义属性：旋转速度（弧度/秒）
        /// </summary>
        public float RotationSpeed { get; set; } = 1.0f;
        
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
            System.Console.WriteLine($"ExampleCustomNode '{Name}' is ready!");
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public override void Update(float delta)
        {
            base.Update(delta);
            
            // 示例：自动旋转
       
                Rotation += RotationSpeed * delta;
            
            // 可以在这里添加其他更新逻辑
        }
        
        /// <summary>
        /// 绘制节点
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            // 这里可以添加自定义绘制逻辑
            // 例如：绘制一个简单的矩形或圆形
            
            // 注意：在实际使用中，你可能需要使用 Sprite 节点来绘制图像
            // 或者使用更高级的绘制方法
        }
    }
    
    /// <summary>
    /// 另一个示例：继承自 Node 的自定义节点
    /// 这个节点不包含 2D 变换功能，适合用于逻辑节点
    /// </summary>
    public class LogicNode : Node
    {
        /// <summary>
        /// 自定义属性：是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// 自定义属性：计数器
        /// </summary>
        public int Counter { get; set; } = 0;
        
        public LogicNode(string name = "LogicNode") : base(name)
        {
        }
        
        public override void Update(float delta)
        {
            base.Update(delta);
            
            if (Enabled)
            {
                Counter++;
            }
        }
    }
}


