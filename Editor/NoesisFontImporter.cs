//#define DEBUG_IMPORTER

using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(2, null, new string[] { "ttf", "otf", "ttc" })]
class NoesisFontImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        #if DEBUG_IMPORTER
            Debug.Log($"=> Import {ctx.assetPath}");
        #endif

        NoesisFont font = (NoesisFont)ScriptableObject.CreateInstance<NoesisFont>();
        font.uri = ctx.assetPath;
        font.content = File.ReadAllBytes(ctx.assetPath);

        ctx.AddObjectToAsset("Font", font);
        ctx.SetMainObject(font);
    }
}
