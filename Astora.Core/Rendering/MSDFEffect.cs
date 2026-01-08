using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering
{
    public class MSDFEffect : Effect
    {
        private EffectParameter? _pxRangeParameter;
        private EffectParameter? _textureSizeParameter;
        private EffectParameter? _bgColorParameter;
        private EffectParameter? _fgColorParameter;

        private float _pxRange = 5.0f;
        private Vector2 _textureSize = Vector2.One;
        private Vector4 _bgColor = new Vector4(0, 0, 0, 0);
        private Vector4 _fgColor = new Vector4(1, 1, 1, 1);
        
        private static MSDFEffect _instance;

        public static MSDFEffect Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Astora.Core.Rendering.Shaders.Build.MSDF.fxc";
        
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
                }
        
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
        
                _instance = new MSDFEffect(Engine.GraphicsDevice, bytes);
                return _instance;
            }
        }

        /// <summary>
        /// 像素范围，控制MSDF的平滑度。默认值为5.0
        /// </summary>
        public float PxRange
        {
            get => _pxRange;
            set
            {
                _pxRange = value;
                _pxRangeParameter?.SetValue(value);
            }
        }

        /// <summary>
        /// 纹理尺寸，用于计算MSDF单位
        /// </summary>
        public Vector2 TextureSize
        {
            get => _textureSize;
            set
            {
                _textureSize = value;
                _textureSizeParameter?.SetValue(value);
            }
        }

        /// <summary>
        /// 背景颜色（RGBA）
        /// </summary>
        public Vector4 BackgroundColor
        {
            get => _bgColor;
            set
            {
                _bgColor = value;
                _bgColorParameter?.SetValue(value);
            }
        }

        /// <summary>
        /// 前景颜色（RGBA）
        /// </summary>
        public Vector4 ForegroundColor
        {
            get => _fgColor;
            set
            {
                _fgColor = value;
                _fgColorParameter?.SetValue(value);
            }
        }

        /// <summary>
        /// 从字节数组创建MSDFEffect
        /// </summary>
        /// <param name="graphicsDevice">图形设备</param>
        /// <param name="effectCode">编译后的着色器字节码</param>
        public MSDFEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
            : base(graphicsDevice, effectCode)
        {
            Initialize();
        }

        /// <summary>
        /// 从另一个Effect克隆创建MSDFEffect
        /// </summary>
        /// <param name="cloneSource">源Effect</param>
        public MSDFEffect(Effect cloneSource)
            : base(cloneSource)
        {
            Initialize();
        }
        

        /// <summary>
        /// 初始化着色器参数引用
        /// </summary>
        private void Initialize()
        {
            _pxRangeParameter = Parameters["pxRange"];
            _textureSizeParameter = Parameters["textureSize"];
            _bgColorParameter = Parameters["bgColor"];
            _fgColorParameter = Parameters["fgColor"];

            // 设置默认值
            if (_pxRangeParameter != null)
                _pxRangeParameter.SetValue(_pxRange);
            if (_textureSizeParameter != null)
                _textureSizeParameter.SetValue(_textureSize);
            if (_bgColorParameter != null)
                _bgColorParameter.SetValue(_bgColor);
            if (_fgColorParameter != null)
                _fgColorParameter.SetValue(_fgColor);
        }

        /// <summary>
        /// 根据纹理自动设置纹理尺寸
        /// </summary>
        /// <param name="texture">纹理对象</param>
        public void SetTextureSize(Texture2D texture)
        {
            if (texture != null)
            {
                TextureSize = new Vector2(texture.Width, texture.Height);
            }
        }

        /// <summary>
        /// 根据纹理的高度自动设置纹理尺寸（兼容旧代码）
        /// </summary>
        /// <param name="textureHeight">纹理高度</param>
        public void SetTextureSize(int textureHeight)
        {
            TextureSize = new Vector2(textureHeight, textureHeight);
        }
    }
}

