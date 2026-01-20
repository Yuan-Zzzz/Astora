using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class SceneGeneratorGame : Game
{
    private readonly string _targetAssemblyPath;
    private readonly string _projectRoot;
    private readonly ContentManager _contentManager;
    private GraphicsDeviceManager _graphicsDeviceManager;

    public SceneGeneratorGame(string targetAssemblyPath, string projectRoot, string contentRoot)
    {
        _targetAssemblyPath = targetAssemblyPath;
        _projectRoot = projectRoot;

        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        _graphicsDeviceManager.IsFullScreen = false; 

        _contentManager = new ContentManager(Services, contentRoot);
    }

    protected override void Initialize()
    {
        base.Initialize();

        Console.WriteLine($"[SceneGenerator] Initializing Engine for content root: {_contentManager.RootDirectory}");

        try
        {
            Engine.Initialize(_contentManager, _graphicsDeviceManager);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SceneGenerator] Warning: Engine initialization failed or was partial. Proceeding with generation. Error: {ex.Message}");
        }

        GenerateScenes();
    }

    private void GenerateScenes()
    {
        Console.WriteLine($"[SceneGenerator] Loading assembly: {_targetAssemblyPath}");
        Assembly? assembly = null;
        try
        {
            assembly = Assembly.LoadFrom(_targetAssemblyPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SceneGenerator] Error: Could not load assembly at {_targetAssemblyPath}. {ex.Message}");
            return;
        }

        var sceneTypes = assembly.GetTypes()
            .Where(t => typeof(IScene).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        Console.WriteLine($"[SceneGenerator] Found {sceneTypes.Count()} scene types.");

        foreach (var type in sceneTypes)
        {
            Console.WriteLine($"[SceneGenerator] Processing Scene: {type.FullName}");
            try
            {
                var method = type.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
                var property = type.GetProperty("ScenePath", BindingFlags.Public | BindingFlags.Static);

                if (method == null || property == null)
                {
                    Console.WriteLine($"[SceneGenerator] Error: Type {type.Name} missing Build or ScenePath.");
                    continue;
                }

                var node = (Node)method.Invoke(null, null)!;
                var relativePath = (string)property.GetValue(null)!;

                var fullPath = Path.Combine(_projectRoot, relativePath);
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Console.WriteLine($"[SceneGenerator] Generating scene to: {fullPath}");

                if (Engine.Serializer != null)
                {
                    Engine.Serializer.Save(node, fullPath);
                    Console.WriteLine($"[SceneGenerator] Successfully saved: {relativePath}");
                }
                else
                {
                    Console.WriteLine("[SceneGenerator] Error: Engine.Serializer is null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SceneGenerator] Error generating scene {type.Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        Console.WriteLine("[SceneGenerator] Generation complete.");
        Exit();
    }
}

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: Astora.SceneGenerator <AssemblyPath> <ProjectRoot> <ContentRoot>");
            return;
        }

        var assemblyPath = args[0];
        var projectRoot = args[1];
        var contentRoot = args[2];

        Console.WriteLine($"[SceneGenerator] Starting...");
        Console.WriteLine($"[SceneGenerator] Assembly: {assemblyPath}");
        Console.WriteLine($"[SceneGenerator] Root: {projectRoot}");

        using var game = new SceneGeneratorGame(assemblyPath, projectRoot, contentRoot);
        game.Run();
    }
}
