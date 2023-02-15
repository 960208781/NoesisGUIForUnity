//#define DEBUG_IMPORTER

using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
using UnityEngine.Video;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

[ScriptedImporter(1, "xaml")]
class NoesisXamlImporter : ScriptedImporter
{
    private static IEnumerable<string> FindFonts(string uri)
    {
        int index = uri.IndexOf('#');
        if (index != -1)
        {
            string folder = uri.Substring(0, index);
            if (Directory.Exists(folder))
            {
                string family = uri.Substring(index + 1);
                var files = Directory.EnumerateFiles(folder).Where(s => IsFont(s));

                foreach (var font in files)
                {
                    using (FileStream file = File.Open(font, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (NoesisUnity.HasFamily(file, family))
                        {
                            yield return font;
                        }
                    }
                }
            }
        }
    }

    private struct Dependency
    {
        public enum Type
        {
            File,
            Font,
            UserControl,
            Shader
        }

        public Type type;
        public string uri;
    }

    struct Dependencies
    {
        public List<Dependency> items;
        public DateTime timestamp;
        public string root;
    }

    private static Dictionary<string, Dependencies> CachedDependencies = new Dictionary<string, Dependencies>();

    static Dependencies GetDependencies(string path)
    {
        Dependencies deps;
        DateTime timestamp = File.GetLastWriteTime(path);

        if (!CachedDependencies.TryGetValue(path, out deps) || deps.timestamp != timestamp)
        {
        #if DEBUG_IMPORTER
            Debug.Log($"=> Dependencies {path}");
        #endif

            deps = new Dependencies();
            deps.items = new List<Dependency>();
            deps.timestamp = timestamp;

            string[] xamlList = null;
            string[] effectList = null;
            string[] brushList = null;

            using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Noesis.GUI.GetXamlDependencies(file, path, (uri_, type) =>
                {
                    try
                    {
                        string uri = Noesis.UriHelper.GetPath(uri_);

                        if (type == Noesis.XamlDependencyType.Filename)
                        {
                            deps.items.Add(new Dependency{ type = Dependency.Type.File, uri = uri});
                        }
                        else if (type == Noesis.XamlDependencyType.Font)
                        {
                            foreach (var font in FindFonts(uri))
                            {
                                deps.items.Add(new Dependency{ type = Dependency.Type.Font, uri = font});
                            }
                        }
                        else if (type == Noesis.XamlDependencyType.UserControl)
                        {
                            if (uri.EndsWith("Effect"))
                            {
                                if (effectList == null)
                                {
                                    effectList = Directory.EnumerateFiles(Application.dataPath, "*.noesiseffect", SearchOption.AllDirectories)
                                        .Select(path => path.Replace(Application.dataPath, "Assets"))
                                        .Select(path => path.Replace("\\", "/"))
                                        .ToArray();
                                }

                                foreach (var effect in effectList)
                                {
                                    if (Path.GetFileNameWithoutExtension(effect) == uri.Replace("Effect", ""))
                                    {
                                        deps.items.Add(new Dependency{ type = Dependency.Type.Shader, uri = effect});
                                        break;
                                    }
                                }
                            }
                            else if (uri.EndsWith("Brush"))
                            {
                                if (brushList == null)
                                {
                                    brushList = Directory.EnumerateFiles(Application.dataPath, "*.noesisbrush", SearchOption.AllDirectories)
                                        .Select(path => path.Replace(Application.dataPath, "Assets"))
                                        .Select(path => path.Replace("\\", "/"))
                                        .ToArray();
                                }

                                foreach (var brush in brushList)
                                {
                                    if (Path.GetFileNameWithoutExtension(brush) == uri.Replace("Brush", ""))
                                    {
                                        deps.items.Add(new Dependency{ type = Dependency.Type.Shader, uri = brush});
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (xamlList == null)
                                {
                                    xamlList = Directory.EnumerateFiles(Application.dataPath, "*.xaml", SearchOption.AllDirectories)
                                        .Select(path => path.Replace(Application.dataPath, "Assets"))
                                        .Select(path => path.Replace("\\", "/"))
                                        .ToArray();
                                }

                                foreach (var xaml in xamlList)
                                {
                                    if (Path.GetFileNameWithoutExtension(xaml) == uri)
                                    {
                                        if (xaml != path)
                                        {
                                            deps.items.Add(new Dependency{ type = Dependency.Type.UserControl, uri = xaml});
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                        else if (type == Noesis.XamlDependencyType.Root)
                        {
                            deps.root = uri;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }

            CachedDependencies[path] = deps;
        }

        return deps;
    }

    static string[] GatherDependenciesFromSourceFile(string path)
    {
        NoesisUnity.InitCore();
        List<string> deps = new List<string>();

        try
        {
            foreach (var dep in GetDependencies(path).items)
            {
                if (File.Exists(dep.uri))
                {
                    deps.Add(dep.uri);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return deps.ToArray();
    }

    private static void AddFont(string uri, ref List<NoesisFont> fonts)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            NoesisFont font = AssetDatabase.LoadAssetAtPath<NoesisFont>(uri);

            if (font != null)
            {
                fonts.Add(font);
            }
        }
    }

    private static void AddTexture(string uri, ref List<Texture> textures, ref List<Sprite> sprites)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            if (AssetImporter.GetAtPath(uri) is TextureImporter textureImporter)
            {
                if (textureImporter.textureType == TextureImporterType.Sprite &&
                    textureImporter.spriteImportMode == SpriteImportMode.Single)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(uri);
                    sprites.Add(sprite);
                }
                else
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(uri);
                    textures.Add(texture);
                }
            }
        }
    }

    private static void AddAudio(string uri, ref List<AudioClip> audios)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            AudioClip audio = AssetDatabase.LoadAssetAtPath<AudioClip>(uri);

            if (audio != null)
            {
                audios.Add(audio);
            }
        }
    }

    private static void AddVideo(string uri, ref List<VideoClip> videos)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            VideoClip video = AssetDatabase.LoadAssetAtPath<VideoClip>(uri);

            if (video != null)
            {
                videos.Add(video);
            }
        }
    }

    private static void AddXaml(string uri, ref List<NoesisXaml> xamls)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            NoesisXaml xaml = AssetDatabase.LoadAssetAtPath<NoesisXaml>(uri);

            if (xaml != null)
            {
                xamls.Add(xaml);
            }
        }
    }

    private static void AddShader(string uri, ref List<NoesisShader> shaders)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            NoesisShader shader = AssetDatabase.LoadAssetAtPath<NoesisShader>(uri);

            if (shader != null)
            {
                shaders.Add(shader);
            }
        }
    }

    private static string ScanDependencies(AssetImportContext ctx, out List<NoesisFont> fonts_,
        out List<Texture> textures_, out List<Sprite> sprites_,
        out List<AudioClip> audios_, out List<VideoClip> videos_,
        out List<NoesisXaml> xamls_, out List<NoesisShader> shaders_)
    {
        List<NoesisFont> fonts = new List<NoesisFont>();
        List<Texture> textures = new List<Texture>();
        List<Sprite> sprites = new List<Sprite>();
        List<AudioClip> audios = new List<AudioClip>();
        List<VideoClip> videos = new List<VideoClip>();
        List<NoesisXaml> xamls = new List<NoesisXaml>();
        List<NoesisShader> shaders = new List<NoesisShader>();

        string filename = ctx.assetPath;
        string root = "";

        try
        {
            if (HasExtension(filename, ".xaml"))
            {
                // Add dependency to code-behind, just the source, we don't need the artifact
                // Even if the file doesn't exist we add it to get a reimport first time code-behind is created
                ctx.DependsOnSourceAsset(filename + ".cs");
            }

            var dependencies = GetDependencies(filename);
            root = dependencies.root;

            foreach (var dep in dependencies.items)
            {
                if (dep.type == Dependency.Type.File)
                {
                    ctx.DependsOnArtifact(dep.uri);

                    AddXaml(dep.uri, ref xamls);
                    AddTexture(dep.uri, ref textures, ref sprites);
                    AddAudio(dep.uri, ref audios);
                    AddVideo(dep.uri, ref videos);
                }
                else if (dep.type == Dependency.Type.Font)
                {
                    ctx.DependsOnArtifact(dep.uri);
                    AddFont(dep.uri, ref fonts);
                }
                else if (dep.type == Dependency.Type.UserControl)
                {
                    ctx.DependsOnArtifact(dep.uri);
                    AddXaml(dep.uri, ref xamls);
                }
                else if (dep.type == Dependency.Type.Shader)
                {
                    ctx.DependsOnArtifact(dep.uri);
                    AddShader(dep.uri, ref shaders);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        fonts_ = fonts;
        textures_ = textures;
        sprites_ = sprites;
        audios_ = audios;
        videos_ = videos;
        xamls_ = xamls;
        shaders_ = shaders;

        return root;
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
        NoesisUnity.InitCore();

        #if DEBUG_IMPORTER
            Debug.Log($"=> Import {ctx.assetPath}");
        #endif

        NoesisXaml xaml = (NoesisXaml)ScriptableObject.CreateInstance<NoesisXaml>();
        xaml.uri = ctx.assetPath;
        xaml.content = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(ctx.assetPath));

        // Add dependencies
        List<NoesisFont> fonts;
        List<Texture> textures;
        List<Sprite> sprites;
        List<AudioClip> audios;
        List<VideoClip> videos;
        List<NoesisXaml> xamls;
        List<NoesisShader> shaders;

        string root = ScanDependencies(ctx, out fonts, out textures, out sprites, out audios, out videos, out xamls, out shaders);

        xaml.UnregisterDependencies();
        xaml.xamls = xamls.Select(x => new NoesisXaml.Xaml { uri = AssetDatabase.GetAssetPath(x), xaml = x }).ToArray();
        xaml.textures = textures.Select(x => new NoesisXaml.Texture { uri = AssetDatabase.GetAssetPath(x), texture = x }).ToArray();
        xaml.sprites = sprites.Select(x => new NoesisXaml.Sprite { uri = AssetDatabase.GetAssetPath(x), sprite = x }).ToArray();
        xaml.audios = audios.Select(x => new NoesisXaml.Audio { uri = AssetDatabase.GetAssetPath(x), audio = x }).ToArray();
        xaml.videos = videos.Select(x => new NoesisXaml.Video { uri = AssetDatabase.GetAssetPath(x), video = x }).ToArray();
        xaml.fonts = fonts.Select(x => new NoesisXaml.Font { uri = AssetDatabase.GetAssetPath(x), font = x }).ToArray();
        xaml.shaders = shaders.Select(x => new NoesisXaml.Shader { uri = Path.GetFileName(AssetDatabase.GetAssetPath(x)), shader = x} ).ToArray();
        xaml.RegisterDependencies();

        if (ctx.assetPath.StartsWith("Assets/") && root != "ResourceDictionary")
        {
            ctx.DependsOnCustomDependency("Noesis_ApplicationResources");
        }

        ctx.AddObjectToAsset("XAML", xaml);
        ctx.SetMainObject(xaml);
    }

    private static bool HasExtension(string filename, string extension)
    {
        return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFont(string filename)
    {
        return HasExtension(filename, ".ttf") || HasExtension(filename, ".otf") || HasExtension(filename, ".ttc");
    }
}
