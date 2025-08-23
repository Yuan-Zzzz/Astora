using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Astora.Core;
using Astora.Core.Time; // Time
using Astora.Engine.Core;               // EngineScheduler, ServiceRegistry
using Astora.Sandbox;                   // Bootstrap.Configure(..)

    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager _gdm;
        private readonly ServiceRegistry _services = new();

        // 把时间在 Update 里做成快照，Draw 中使用（也可直接在 Draw 用 gameTime 做）。
        private Time _time;

        public Game1()
        {
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth  = 1280,
                PreferredBackBufferHeight = 720,
                PreferMultiSampling = false, 
                SynchronizeWithVerticalRetrace = true
            };

            IsFixedTimeStep = true;               
            TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / 60.0);

            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Bootstrap.Configure(_services, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            _time = new Time((float)gameTime.ElapsedGameTime.TotalSeconds,
                (float)gameTime.TotalGameTime.TotalSeconds);
            _services.Get<LogicScheduler>().Tick(_time);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _services.Get<RenderScheduler>().Tick(_time);
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, System.EventArgs args)
        {
            base.OnExiting(sender, args);
        }
    }
