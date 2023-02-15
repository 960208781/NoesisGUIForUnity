using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class NoesisUpdater
{
    static NoesisUpdater()
    {
        EditorApplication.update += CheckVersion;
    }

    private static void CheckVersion()
    {
        EditorApplication.update -= CheckVersion;

        if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.noesis.noesisgui");

            if (package == null)
            {
                Debug.LogError("Can't find package 'com.noesis.noesisgui'. Please, make sure you didn't unzip the plugin inside '/Assets'");
            }
            else
            {
                var settings = NoesisSettings.Get();
                string localVersion = settings.Version;
                string version = package.version;
    
                if (localVersion != version)
                {
                    settings.Version = version;

                    Upgrade(localVersion);

                    if (version != "0.0.0")
                    {
                        GoogleAnalyticsHelper.LogEvent("Install", version, 0);
                    }

                    EditorApplication.update += ShowWelcomeWindow;
                    Debug.Log("NoesisGUI v" + version + " successfully installed");
                }
            }
        }
    }

    private static void ShowWelcomeWindow()
    {
        EditorApplication.update -= ShowWelcomeWindow;
        NoesisWelcome.Open();
    }

    private static string NormalizeVersion(string version)
    {
        string pattern = @"^(\d+).(\d+).(\d+)((a|b|rc|f)(\d*))?$";
        var match = Regex.Match(version.ToLower(), pattern);

        string normalized = "";

        if (match.Success)
        {
            normalized += match.Groups[1].Value.PadLeft(2, '0');
            normalized += ".";
            normalized += match.Groups[2].Value.PadLeft(2, '0');
            normalized += ".";
            normalized += match.Groups[3].Value.PadLeft(2, '0');

            if (match.Groups[4].Length > 0)
            {
                if (match.Groups[5].Value == "a")
                {
                    normalized += ".0.";
                }
                else if (match.Groups[5].Value == "b")
                {
                    normalized += ".1.";
                }
                else if (match.Groups[5].Value == "rc")
                {
                    normalized += ".2.";
                }
                else if (match.Groups[5].Value == "f")
                {
                    normalized += ".3.";
                }

                normalized += match.Groups[6].Value.PadLeft(2, '0');
            }
            else
            {
                normalized += ".3.00";
            }
        }
        else
        {
            Debug.LogError("Unexpected version format " + version);
        }

        return normalized;
    }

    private static bool PatchNeeded(string from, string to)
    {
        if (from.Length == 0)
        {
            return false;
        }
        else
        {
            return string.Compare(NormalizeVersion(from), NormalizeVersion(to)) < 0;
        }
    }

    private static void Upgrade(string version)
    {
        if (PatchNeeded(version, "3.1.0"))
        {
            UpdateAssets();
        }

        if (PatchNeeded(version, "3.1.5"))
        {
            UpdateTextures();
        }
    }

    private static void UpdateAssets()
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            var assets = AssetDatabase.FindAssets("")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(s => s.StartsWith("Assets/") && (IsFont(s) || IsXaml(s)))
                .Distinct().ToArray();

            foreach (var asset in assets)
            {
                // Remove old 3.0 asset if there is any
                AssetDatabase.DeleteAsset(Path.ChangeExtension(asset, ".asset"));

                // Reimport asset to assign proper importer
                if (AssetDatabase.GetImporterOverride(asset) == null)
                {
                    AssetDatabase.ImportAsset(asset);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private static void UpdateTextures()
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            var assets = AssetDatabase.FindAssets("l:Noesis t:texture")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(s => s.StartsWith("Assets/"))
                .Distinct().ToArray();

            foreach (var asset in assets)
            {
                // Remove isReadable flag from Noesis textures
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(asset);
                importer.isReadable = false;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
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
