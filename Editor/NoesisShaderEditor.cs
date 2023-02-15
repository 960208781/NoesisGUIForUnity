using UnityEditor;
using System.IO;

[CustomEditor(typeof(NoesisShader))]
public class NoesisShaderEditor: Editor
{
    public void OnEnable()
    {
        _text = File.ReadAllText(AssetDatabase.GetAssetPath(target));
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.TextArea(_text);
    }

    private string _text;
}