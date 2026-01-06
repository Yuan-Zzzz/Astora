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
        /// 自定义字段：速度
        /// </summary>
        [SerializeField]
        private float _speed = 100.0f;
        
        /// <summary>
        /// 自定义字段：颜色
        /// </summary>
        [SerializeField]
        private Color _nodeColor = Color.Red;
        
        /// <summary>
        /// 自定义字段：是否自动旋转
        /// </summary>
        [SerializeField]
        private bool _autoRotate = false;
        
        /// <summary>
        /// 自定义字段：旋转速度（弧度/秒）
        /// </summary>
        [SerializeField]
        private float _rotationSpeed = 1.0f;
        
        /// <summary>
        /// 自定义字段：生命值（用于测试 int 类型）
        /// </summary>
        [SerializeField]
        private int _health = 100;
        
        /// <summary>
        /// 自定义字段：分数（用于测试 int 类型）
        /// </summary>
        [SerializeField]
        private int _score = 0;
        
        /// <summary>
        /// 自定义字段：描述（用于测试 string 类型）
        /// </summary>
        [SerializeField]
        private string _description = "Example Custom Node";
        
        /// <summary>
        /// 自定义字段：标签（用于测试 string 类型）
        /// </summary>
        [SerializeField]
        private string _tag = "Default";
        
        /// <summary>
        /// 自定义字段：精确值（用于测试 double 类型）
        /// </summary>
        [SerializeField]
        private double _precisionValue = 3.141592653589793;
        
        // 公共属性访问器（可选，用于外部访问）
        public float Speed { get => _speed; set => _speed = value; }
        public Color NodeColor { get => _nodeColor; set => _nodeColor = value; }
        public bool AutoRotate { get => _autoRotate; set => _autoRotate = value; }
        public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }
        public int Health { get => _health; set => _health = value; }
        public int Score { get => _score; set => _score = value; }
        public string Description { get => _description; set => _description = value; }
        public string Tag { get => _tag; set => _tag = value; }
        public double PrecisionValue { get => _precisionValue; set => _precisionValue = value; }
        
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
            if (AutoRotate)
            {
                Rotation += RotationSpeed * delta;
            }
            
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
        /// 自定义字段：是否启用
        /// </summary>
        [SerializeField]
        private bool _enabled = true;
        
        /// <summary>
        /// 自定义字段：计数器
        /// </summary>
        [SerializeField]
        private int _counter = 0;
        
        // 公共属性访问器（可选，用于外部访问）
        public bool Enabled { get => _enabled; set => _enabled = value; }
        public int Counter { get => _counter; set => _counter = value; }
        
        public LogicNode(string name = "LogicNode") : base(name)
        {
        }
        
        public override void Update(float delta)
        {
            base.Update(delta);
            
            if (_enabled)
            {
                _counter++;
            }
        }
    }
}


