using Astora.Core.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// 如果你有自定义脚本，也需要引用，或者通过反射动态注册

namespace Astora.Core.Utils
{
    public class YamlSceneSerializer : ISceneSerializer
    {
        public string GetExtension() => ".scene";

        public void Save(Node rootNode, string path)
        {
        }

        public Node Load(string path)
        {
            return null;
        }
    }
}