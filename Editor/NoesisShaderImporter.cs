//#define DEBUG_IMPORTER

using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using System.Linq;

[ScriptedImporter(2, new string[] { "noesiseffect", "noesisbrush" })]
class NoesisShaderImporter : ScriptedImporter
{
    private static string FindFxc()
    {
        var folders = Directory.GetDirectories(@"C:\Program Files (x86)\Windows Kits\10\bin");
        foreach (var folder in folders.OrderByDescending(x => x))
        {
            if (File.Exists(folder + @"\x64\fxc.exe"))
            {
                return folder + @"\x64\fxc.exe";
            }
        }

        if (File.Exists(@"C:\Program Files (x86)\Windows Kits\10\bin\x64\fxc.exe"))
        {
            return @"C:\Program Files (x86)\Windows Kits\10\bin\x64\fxc.exe";
        }

        if (File.Exists(@"C:\Program Files (x86)\Windows Kits\8.1\bin\x64\fxc.exe"))
        {
            return @"C:\Program Files (x86)\Windows Kits\8.1\bin\x64\fxc.exe";
        }

        return null;
    }

    private static byte[] HLSLCompile(AssetImportContext ctx, string fxc, string defines = "")
    {
        string includes = Path.GetFullPath("Packages/com.noesis.noesisgui/Shaders");

        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = fxc;
        process.StartInfo.Arguments = $"/T ps_4_0 /O3 /Qstrip_reflect /I \"{includes}\" {defines} /Fo Temp/shader.pso \"{ctx.assetPath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        string err = process.StandardError.ReadToEnd();
        ctx.LogImportError(err.Replace("\\", "/").Replace(Application.dataPath, "Assets"));

        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            return File.ReadAllBytes(@"Temp\shader.pso");
        }

        return null;
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
        #if DEBUG_IMPORTER
            Debug.Log($"=> Import {ctx.assetPath}");
        #endif

        if (ctx.selectedBuildTarget == BuildTarget.StandaloneWindows || ctx.selectedBuildTarget == BuildTarget.StandaloneWindows64)
        {
            string fxc = FindFxc();

            NoesisShader shader = (NoesisShader)ScriptableObject.CreateInstance<NoesisShader>();

            if (ctx.assetPath.EndsWith(".noesiseffect"))
            {
                shader.effect_bytecode = HLSLCompile(ctx, fxc);

                ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/EffectHelpers.h");
            }
            else
            {
                shader.brush_path_bytecode = HLSLCompile(ctx, fxc, "/DPAINT_PATTERN /DCUSTOM_PATTERN /DEFFECT_PATH");
                shader.brush_path_aa_bytecode = HLSLCompile(ctx, fxc, "/DPAINT_PATTERN /DCUSTOM_PATTERN /DEFFECT_PATH_AA");
                shader.brush_sdf_bytecode = HLSLCompile(ctx, fxc, "/DPAINT_PATTERN /DCUSTOM_PATTERN /DEFFECT_SDF");
                shader.brush_opacity_bytecode = HLSLCompile(ctx, fxc, "/DPAINT_PATTERN /DCUSTOM_PATTERN /DEFFECT_OPACITY");

                ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/BrushHelpers.h");
            }

            ctx.AddObjectToAsset("Shader", shader);
        }
        else
        {
            ctx.LogImportError($"{ctx.assetPath}: '{ctx.selectedBuildTarget}' is not supported");
        }
    }
}
