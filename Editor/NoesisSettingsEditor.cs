using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[CustomEditor(typeof(NoesisSettings))]
public class NoesisSettingsEditor: Editor
{
    public override void OnInspectorGUI()
    {
        OnInspectorGUI(new SerializedObject((NoesisSettings)target));
    }

    private static bool _showCursors = false;

    public static void OnInspectorGUI(SerializedObject settings)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(settings.FindProperty("licenseName"));
        EditorGUILayout.PropertyField(settings.FindProperty("licenseKey"));
        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            NoesisUnity.ReloadLicense();
        }

        if (((NoesisSettings)settings.targetObject).licenseName == "" || ((NoesisSettings)settings.targetObject).licenseKey == "")
        {
            GUILayout.Space(10);
            bool richText = GUI.skin.GetStyle("HelpBox").richText;
            GUI.skin.GetStyle("HelpBox").richText = true;
            EditorGUILayout.HelpBox("License not set. Get one at <a>https://www.noesisengine.com/trial</a>", MessageType.Warning);
            GUI.skin.GetStyle("HelpBox").richText = richText;
            EditorGUIUtility.AddCursorRect(  GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://www.noesisengine.com/trial");
            }
        }

        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(settings.FindProperty("applicationResources"));
        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            NoesisUnity.ReloadApplicationResources();

            // Flush changes to disk for thumbnail processes spawned by Unity
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(settings.FindProperty("defaultFont"));
        EditorGUILayout.PropertyField(settings.FindProperty("loadPlatformFonts"));
        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            NoesisUnity.ReloadDefaultFont();

            // Flush changes to disk for thumbnail processes spawned by Unity
            AssetDatabase.SaveAssets();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(settings.FindProperty("defaultFontSize"));
        EditorGUILayout.PropertyField(settings.FindProperty("defaultFontWeight"));
        EditorGUILayout.PropertyField(settings.FindProperty("defaultFontStretch"));
        EditorGUILayout.PropertyField(settings.FindProperty("defaultFontStyle"));
        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            NoesisUnity.ReloadDefaultFontParams();

            // Flush changes to disk for thumbnail processes spawned by Unity
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.PropertyField(settings.FindProperty("glyphTextureSize"));
        EditorGUILayout.PropertyField(settings.FindProperty("offscreenSampleCount"));
        EditorGUILayout.PropertyField(settings.FindProperty("offscreenInitSurfaces"));
        EditorGUILayout.PropertyField(settings.FindProperty("offscreenMaxSurfaces"));
        EditorGUILayout.PropertyField(settings.FindProperty("linearRendering"));

        EditorGUILayout.PropertyField(settings.FindProperty("previewEnabled"));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(settings.FindProperty("generalLogLevel"));
        EditorGUILayout.PropertyField(settings.FindProperty("bindingLogLevel"));
        if (EditorGUI.EndChangeCheck())
        {
            settings.ApplyModifiedProperties();
            NoesisUnity.ReloadLogLevel();
        }

        GUILayout.Space(10);
        GUILayout.Label("Native Memory", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Allocated");
        EditorGUILayout.HelpBox(Noesis.Memory.Current.ToString("#,0,. KB"), MessageType.None);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        EditorStyles.foldout.fontStyle = FontStyle.Bold;
        _showCursors = EditorGUILayout.Foldout(_showCursors, "Cursors", false, EditorStyles.foldout);
        if (_showCursors)
        {
            EditorStyles.foldout.fontStyle = FontStyle.Normal;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settings.FindProperty("AppStarting"), new GUIContent("AppStarting"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Arrow"), new GUIContent("Arrow"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ArrowCD"), new GUIContent("ArrowCD"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Cross"), new GUIContent("Cross"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Hand"), new GUIContent("Hand"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Help"), new GUIContent("Help"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("IBeam"), new GUIContent("IBeam"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("No"), new GUIContent("No"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("None"), new GUIContent("None"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Pen"), new GUIContent("Pen"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollAll"), new GUIContent("ScrollAll"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollE"), new GUIContent("ScrollE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollN"), new GUIContent("ScrollN"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollNE"), new GUIContent("ScrollNE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollNS"), new GUIContent("ScrollNS"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollNW"), new GUIContent("ScrollNW"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollS"), new GUIContent("ScrollS"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollSE"), new GUIContent("ScrollSE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollSW"), new GUIContent("ScrollSW"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollW"), new GUIContent("ScrollW"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("ScrollWE"), new GUIContent("ScrollWE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("SizeAll"), new GUIContent("SizeAll"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("SizeNESW"), new GUIContent("SizeNESW"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("SizeNS"), new GUIContent("SizeNS"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("SizeNWSE"), new GUIContent("SizeNWSE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("SizeWE"), new GUIContent("SizeWE"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("UpArrow"), new GUIContent("UpArrow"), true);
            EditorGUILayout.PropertyField(settings.FindProperty("Wait"), new GUIContent("Wait"), true);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("(*) Unity restart needed for updates to take effect", MessageType.None);

        if (settings.hasModifiedProperties)
        {
            settings.ApplyModifiedProperties();
        }
    }

#if UNITY_2018_3_OR_NEWER
    [SettingsProvider]
    public static SettingsProvider CreateNoesisSettingsProvider()
    {
        var provider = new SettingsProvider("Project/NoesisSettings", SettingsScope.Project)
        {
            label = "NoesisGUI",
            guiHandler = (searchContext) =>
            {
                OnInspectorGUI(new SerializedObject(NoesisSettings.Get()));
            },

            titleBarGuiHandler = () =>
            {
                if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Help"), FocusType.Passive, EditorStyles.label))
                {
                    Application.OpenURL("https://www.noesisengine.com/docs/Gui.Core.Unity3DTutorial.html");
                }

                if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Popup"), FocusType.Passive, EditorStyles.label))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Reset"), false, () =>
                    {
                        NoesisSettings.Get().Reset();
                    });
                    menu.ShowAsContext();
                }
            },

            keywords = new HashSet<string>(new[] { "License Name", "License Key", "Application Resources", 
                "Default Font", "Default Font Size", "Default Font Weight", "Default Font Stretch", "Default Font Style",
                "Glyph Texture Size", "Offscreen Sample Count", "Offscreen Init Surfaces", "Offscreen Max Surfaces",
                "Linear Rendering", "Preview Enabled", "Log Verbosity", 
                "AppStarting", "Arrow", "ArrowCD", "Cross", "Hand", "Help", "IBeam", "No", "None", "Pen", "ScrollAll",
                "ScrollE", "ScrollN", "ScrollNE", "ScrollNS", "ScrollNW", "ScrollS", "ScrollSE", "ScrollSW",
                "ScrollW", "ScrollWE", "SizeAll", "SizeNESW", "SizeNS", "SizeNWSE","SizeWE", "UpArrow", "Wait" })
        };

        return provider;
    }
#endif
}