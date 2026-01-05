using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Astora.SandBox
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneTree _sceneTree;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // 1. 初始化场景树
            _sceneTree = new SceneTree();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 2. 注入全局依赖 (让引擎能访问 Content 和 Graphics)
            Engine.Content = Content;
            Engine.GraphicsDevice = GraphicsDevice;
            Engine.SpriteBatch = _spriteBatch;

            // --- 3. 动态生成测试纹理 (避免处理文件路径) ---
            
            // 生成一个白色方块 (玩家)
            var playerTexture = new Texture2D(GraphicsDevice, 32, 32);
            Color[] data = new Color[32 * 32];
            for(int i=0; i<data.Length; ++i) data[i] = Color.White;
            playerTexture.SetData(data);

            // 生成一个红色方块 (武器)
            var weaponTexture = new Texture2D(GraphicsDevice, 32, 32);
            Color[] data2 = new Color[32 * 32];
            for(int i=0; i<data2.Length; ++i) data2[i] = Color.Red;
            weaponTexture.SetData(data2);

            // --- 4. 启动场景 ---
            var mainScene = new MainScene(playerTexture, weaponTexture);
            _sceneTree.ChangeScene(mainScene);
        }

        protected override void Update(GameTime gameTime)
        {
            // 5. 将 Update 委托给引擎
            _sceneTree.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // 6. 将 Draw 委托给引擎
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp); // PointClamp 适合像素风
            _sceneTree.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    
    public class MainScene : Node2D
    {
        private Texture2D _playerTex;
        private Texture2D _weaponTex;

        // 我们通过构造函数传入纹理，或者在 Ready 里加载
        public MainScene(Texture2D playerTex, Texture2D weaponTex) : base("MainScene")
        {
            _playerTex = playerTex;
            _weaponTex = weaponTex;
        }

        public override void Ready()
        {
            // --- 1. 创建玩家 ---
            var player = new Player(_playerTex);
            player.Position = new Vector2(400, 300); // 屏幕中心
            AddChild(player); // 把玩家加入场景

            // --- 2. 创建一个“武器” (子节点测试) ---
            // 这个 Sprite 直接使用引擎原生的类，没有自定义逻辑
            var weapon = new Sprite("Weapon", _weaponTex);
            weapon.Position = new Vector2(30, 0); // 在玩家右边 30 像素
            weapon.Scale = new Vector2(0.5f, 0.5f); // 小一点
            
            // 关键点：把武器加给玩家，而不是加给场景！
            // 这样武器就会跟随玩家移动和旋转。
            player.AddChild(weapon);
        }
    }
    
    public class Player : Sprite
    {
        public float MoveSpeed = 200f;
        public float RotateSpeed = 3f;

        public Player(Texture2D texture) : base("Player", texture)
        {
            // 设置初始属性
            Scale = new Vector2(2, 2); // 放大一点
        }

        public override void Update(float delta)
        {
            // 1. 移动逻辑 (使用封装的 Input.GetAxis)
            var direction = new Vector2(
                Input.GetAxis(Keys.A, Keys.D), // A/D 控制左右
                Input.GetAxis(Keys.W, Keys.S)  // W/S 控制上下
            );

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Position += direction * MoveSpeed * delta;
            }

            // 2. 旋转逻辑 (按 Q/E 旋转)
            if (Input.IsKeyDown(Keys.E)) Rotation += RotateSpeed * delta;
            if (Input.IsKeyDown(Keys.Q)) Rotation -= RotateSpeed * delta;
            
            // 3. 测试：点击空格键销毁自己 (测试 QueueFree)
            if (Input.IsKeyPressed(Keys.Space))
            {
                System.Console.WriteLine("Player destroyed!");
                this.QueueFree();
            }
        }
    }
}