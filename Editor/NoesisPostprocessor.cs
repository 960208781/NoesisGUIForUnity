//#define DEBUG_IMPORTER

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class NoesisPostprocessor : AssetPostprocessor
{
    private static HashSet<string> _pendingXamls = new HashSet<string>();

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var asset in importedAssets.Concat(movedAssets))
        {
            if (IsXaml(asset) && !_pendingXamls.Contains(asset))
            {
                EditorApplication.CallbackFunction d = null;

                // Delay error messages until scripts end compiling
                d = () =>
                {
                    if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
                    {
                        _pendingXamls.Remove(asset);
                        EditorApplication.update -= d;

                        try
                        {
                            // Show parsing errors in the console
                            NoesisXaml xaml = AssetDatabase.LoadAssetAtPath<NoesisXaml>(asset);

                            if (xaml != null)
                            {
                                xaml.Load();
                                ReloadXaml(asset);

                                if (xaml == NoesisSettings.Get().applicationResources)
                                {
                                    #if DEBUG_IMPORTER
                                        Debug.Log("=> Reloading Theme");
                                    #endif

                                    NoesisUnity.ReloadApplicationResources();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                };

                _pendingXamls.Add(asset);
                EditorApplication.update += d;
            }
        }
    }

    private static void ReloadXaml(string uri)
    {
        if (NoesisUnity.Initialized)
        {
            #if DEBUG_IMPORTER
                Debug.Log($"=> Reload {uri}");
            #endif

            NoesisUnity.MuteLog();
            NoesisXamlProvider.instance.ReloadXaml(uri);
            NoesisUnity.UnmuteLog();
        }
    }

    private static void ReloadFont(string uri)
    {
        if (NoesisUnity.Initialized)
        {
            #if DEBUG_IMPORTER
                Debug.Log($"=> Reload {uri}");
            #endif

            NoesisFontProvider.instance.ReloadFont(uri);
        }
    }

    private void OnPreprocessAsset()
    {
        if (assetPath.StartsWith("Assets/"))
        {
            if (IsXaml(assetPath))
            {
                // Update xaml importer for old xaml assets
                AssetDatabase.SetImporterOverride<NoesisXamlImporter>(assetPath);
            }
            else if (IsFont(assetPath))
            {
                // Noesis uses a custom font importer that replaces Unity's default one
                // Change the importer to Noesis the first time a font is added
                if (assetImporter.importSettingsMissing)
                {
                    AssetDatabase.SetImporterOverride<NoesisFontImporter>(assetPath);
                }
            }
        }
    }

    private bool HasNoesisLabel()
    {
        // 'Noesis' label is used to indicate that a given texture must be premultiplied by alpha
        // Noesis always require textures in alpha premultiplied format
        if (AssetDatabase.GetLabels(assetImporter).Contains("Noesis"))
        {
            return true;
        }

        // Sometimes (for example, when deleting the 'Library' folder), AssetDatabase.GetLabels
        // can't find labels. We need to use this hack
        if (File.Exists($"{assetPath}.meta"))
        {
            string meta = File.ReadAllText($"{assetPath}.meta");
            return meta.IndexOf("- Noesis") != -1;
        }

        return false;
    }

    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith("Assets/") && HasNoesisLabel())
        {
            // If the texture is going to be modified it is required to be readable
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.isReadable = true;
        }
    }

    private void OnPostprocessTexture(Texture2D texture)
    {
        if (assetPath.StartsWith("Assets/"))
        {
            if (HasNoesisLabel())
            {
                #if DEBUG_IMPORTER
                    Debug.Log($"=> Premult {assetPath}");
                #endif

                Color[] c = texture.GetPixels(0);

                // NoesisGUI needs premultipled alpha
                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        c[i].r = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].r) * c[i].a);
                        c[i].g = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].g) * c[i].a);
                        c[i].b = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].b) * c[i].a);
                    }
                }
                else
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        c[i].r = c[i].r * c[i].a;
                        c[i].g = c[i].g * c[i].a;
                        c[i].b = c[i].b * c[i].a;
                    }
                }

                // Set new content
                texture.SetPixels(c, 0);
                texture.Apply(true, true);

                // NOTE: We have to make the texture unreadable at runtime as indicated in issue #2352,
                // otherwise leaving texture assets with the isReadable=true take twice space because Unity
                // stores an additional copy of texture's pixel data in CPU-addressable memory
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.isReadable = false;
            }

            if (NoesisUnity.Initialized)
            {
                // Reloading of texture
                UpdateTextures();
            }
        }
    }

    private static Action<PlayModeStateChange> _updateTextures = null;

    private static void UpdateTextures()
    {
        // Texture native pointer is invalidated, update it before next frame is rendered. We cannot query
        // the new native pointer right now (during the import process) because it is not yet created
        NoesisTextureProvider.instance.dirtyTextures = true;

        // NOTE: When a texture is modified (while playing or not), next time Play is clicked it recreates
        // all live textures, so we need to set 'dirtyTextures' flag just after exiting Edit mode to
        // update texture native pointers before the first frame is rendered
        if (_updateTextures == null)
        {
            _updateTextures = (mode) =>
            {
                if (mode == PlayModeStateChange.ExitingEditMode)
                {
                    EditorApplication.playModeStateChanged -= _updateTextures;
                    _updateTextures = null;

                    NoesisTextureProvider.instance.dirtyTextures = true;
                }
            };

            EditorApplication.playModeStateChanged += _updateTextures;
        }
    }

    private static bool HasExtension(string filename, string extension)
    {
        return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsXaml(string filename)
    {
        return HasExtension(filename, ".xaml");
    }

    private static bool IsFont(string filename)
    {
        return HasExtension(filename, ".ttf") || HasExtension(filename, ".otf") || HasExtension(filename, ".ttc");
    }
}

public class NoesisBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        #if !UNITY_2021_2_OR_NEWER
            if (report.summary.platform == BuildTarget.WebGL)
            {
                Debug.LogError("Unity 2021.2+ is required for using NoesisGUI + WebGL");
            }
        #endif
    }
}
