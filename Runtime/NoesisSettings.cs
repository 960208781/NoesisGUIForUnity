//#define DEBUG_IMPORTER

using UnityEngine;
using System.Linq;
using System.IO;
using System;

/// <summary>
/// Noesis global settings
/// </summary>
public class NoesisSettings: ScriptableObject
{
    private static NoesisSettings _settings;

#if UNITY_EDITOR
    private static readonly string Name = "Noesis.settings.asset";
    private static readonly string DefaultResources = "Packages/com.noesis.noesisgui/Theme/NoesisTheme.DarkBlue.xaml";
    private static readonly string DefaultFont = "Packages/com.noesis.noesisgui/Theme/Fonts/PT Root UI_Regular.otf";

    private static NoesisSettings CreateDefault()
    {
        NoesisSettings settings = (NoesisSettings)ScriptableObject.CreateInstance<NoesisSettings>();
        settings.applicationResources = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisXaml>(DefaultResources);
        settings.defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisFont>(DefaultFont);
        return settings;
    }
#endif

    public static NoesisSettings Get()
    {
        if (_settings == null)
        {
        #if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.FindAssets("t:NoesisSettings")
                .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .FirstOrDefault();

            _settings = (NoesisSettings)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(NoesisSettings));

            #if DEBUG_IMPORTER
                // IMPORTANT: Settings must not be loaded while importing assets
                Debug.Log($"=> Settings {_settings},{_settings?.licenseName},{_settings?.applicationResources},{_settings?.defaultFont}");
            #endif

            if (_settings == null)
            {
                string uri = Directory.GetFiles(Application.dataPath, Name, SearchOption.AllDirectories)
                    .Select(x => x.Replace(Application.dataPath, "Assets"))
                    .Select(x => x.Replace("\\", "/"))
                    .FirstOrDefault();

                if (!String.IsNullOrEmpty(uri))
                {
                    // In rare situations (for example when upgrading the project to a new version of Unity),
                    // NoesisSettings exists but Unity doesn't load it because it is not registered yet.
                    // In this case, we return an uncached temporary instance
                    Debug.LogWarning($"{uri} not fully loaded. Returning a temporary instance");
                    return CreateDefault();
                }

                _settings = CreateDefault();

                UnityEditor.AssetDatabase.CreateAsset(_settings, $"Assets/{Name}");
                UnityEditor.EditorGUIUtility.PingObject(_settings);
                Debug.Log($"A new settings file was created in 'Assets/{Name}'", _settings);

                UnityEditor.AssetDatabase.SaveAssets();
            }

            // Update NoesisSettings object stored in NoesisUnity
            NoesisUnity.UpdateSettings(_settings);
            NoesisUnity.ReloadLogLevel();
            NoesisUnity.ReloadLicense();
            NoesisUnity.ReloadApplicationResources();
            NoesisUnity.ReloadDefaultFont();
            NoesisUnity.ReloadDefaultFontParams();

        #else
            _settings = Resources.FindObjectsOfTypeAll<NoesisSettings>().FirstOrDefault();

            if (_settings == null)
            {
                Debug.LogError("Noesis settings asset not found");
            }
        #endif
         }

        return _settings;
    }

#if UNITY_EDITOR
    public void Reset()
    {
        licenseName = "";
        licenseKey = "";
        applicationResourcesHash = new Hash128();
        applicationResources = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisXaml>(DefaultResources);
        defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisFont>(DefaultFont);
        loadPlatformFonts = true;
        defaultFontSize = 15.0f;
        defaultFontWeight = Noesis.FontWeight.Normal;
        defaultFontStretch = Noesis.FontStretch.Normal;
        defaultFontStyle = Noesis.FontStyle.Normal;
        glyphTextureSize = TextureSize._1024x1024;
        offscreenSampleCount = OffscreenSampleCount._1x;
        offscreenInitSurfaces = 0;
        offscreenMaxSurfaces = 0;
        linearRendering = LinearRendering._SamesAsUnity;
        previewEnabled = true;
        generalLogLevel = LogLevel.Warning;
        bindingLogLevel = LogLevel.Warning;

        AppStarting.Texture = null;
        AppStarting.HotSpot = Vector2.zero;
        Arrow.Texture = null;
        Arrow.HotSpot = Vector2.zero;
        ArrowCD.Texture = null;
        ArrowCD.HotSpot = Vector2.zero;
        Cross.Texture = null;
        Cross.HotSpot = Vector2.zero;
        Hand.Texture = null;
        Hand.HotSpot = Vector2.zero;
        Help.Texture = null;
        Help.HotSpot = Vector2.zero;
        IBeam.Texture = null;
        IBeam.HotSpot = Vector2.zero;
        No.Texture = null;
        No.HotSpot = Vector2.zero;
        None.Texture = null;
        None.HotSpot = Vector2.zero;
        Pen.Texture = null;
        Pen.HotSpot = Vector2.zero;
        ScrollAll.Texture = null;
        ScrollAll.HotSpot = Vector2.zero;
        ScrollE.Texture = null;
        ScrollE.HotSpot = Vector2.zero;
        ScrollN.Texture = null;
        ScrollN.HotSpot = Vector2.zero;
        ScrollNE.Texture = null;
        ScrollNE.HotSpot = Vector2.zero;
        ScrollNS.Texture = null;
        ScrollNS.HotSpot = Vector2.zero;
        ScrollNW.Texture = null;
        ScrollNW.HotSpot = Vector2.zero;
        ScrollS.Texture = null;
        ScrollS.HotSpot = Vector2.zero;
        ScrollSE.Texture = null;
        ScrollSE.HotSpot = Vector2.zero;
        ScrollSW.Texture = null;
        ScrollSW.HotSpot = Vector2.zero;
        ScrollW.Texture = null;
        ScrollW.HotSpot = Vector2.zero;
        ScrollWE.Texture = null;
        ScrollWE.HotSpot = Vector2.zero;
        SizeAll.Texture = null;
        SizeAll.HotSpot = Vector2.zero;
        SizeNESW.Texture = null;
        SizeNESW.HotSpot = Vector2.zero;
        SizeNS.Texture = null;
        SizeNS.HotSpot = Vector2.zero;
        SizeNWSE.Texture = null;
        SizeNWSE.HotSpot = Vector2.zero;
        SizeWE.Texture = null;
        SizeWE.HotSpot = Vector2.zero;
        UpArrow.Texture = null;
        UpArrow.HotSpot = Vector2.zero;
        Wait.Texture = null;
        Wait.HotSpot = Vector2.zero;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        NoesisUnity.ReloadLogLevel();
        NoesisUnity.ReloadLicense();
        NoesisUnity.ReloadApplicationResources();
        NoesisUnity.ReloadDefaultFont();
        NoesisUnity.ReloadDefaultFontParams();
    }
#endif

    [Header("License")]
    [Tooltip("Fill with the Name value your were given when purchasing your Noesis license")]
    public string licenseName = "";

    [Tooltip("Fill with the Key value your were given when purchasing your Noesis license")]
    public string licenseKey = "";

    [Header("XAML")]
    [Tooltip("Sets a collection of application-scope resources, such as styles and brushes. " +
        "Provides a simple way to support a consistent theme across your application")]
    public NoesisXaml applicationResources;
    public Hash128 applicationResourcesHash = new Hash128();

    [Tooltip("Default value for FontFamily when it is not specified in a control or text element.")]
    public NoesisFont defaultFont;

    [Tooltip("Loads platform specific font fallbacks to be able to render a wide range of unicode " +
        "characters like chinese, korean, japanese or emojis")]
    public bool loadPlatformFonts = true;

    [Tooltip("Default value for FontSize when it is not specified in a control or text element")]
    public float defaultFontSize = 15.0f;

    [Tooltip("Default value for FontWeight when it is not specified in a control or text element")]
    public Noesis.FontWeight defaultFontWeight = Noesis.FontWeight.Normal;

    [Tooltip("Default value for FontStretch when it is not specified in a control or text element")]
    public Noesis.FontStretch defaultFontStretch = Noesis.FontStretch.Normal;

    [Tooltip("Default value for FontStyle when it is not specified in a control or text element")]
    public Noesis.FontStyle defaultFontStyle = Noesis.FontStyle.Normal;

    public enum TextureSize
    {
        _256x256,
        _512x512,
        _1024x1024,
        _2048x2048,
        _4096x4096
    }

    public enum OffscreenSampleCount
    {
        _SameAsUnity,
        _1x,
        _2x,
        _4x,
        _8x
    }

    public enum LinearRendering
    {
        _SamesAsUnity,
        _Enabled,
        _Disabled
    }

    [Header("Rendering (*)")]
    [Tooltip("Dimensions of texture used to cache glyphs")]
    public TextureSize glyphTextureSize = TextureSize._1024x1024;

    [Tooltip("Multisampling of offscreen textures")]
    public OffscreenSampleCount offscreenSampleCount = OffscreenSampleCount._1x;

    [Tooltip("Number of offscreen textures created at startup")]
    public uint offscreenInitSurfaces = 0;

    [Tooltip("Max number of offscreen textures (0 = unlimited)")]
    public uint offscreenMaxSurfaces = 0;

    [Tooltip("Enables linear color space")]
    public LinearRendering linearRendering = LinearRendering._SamesAsUnity;

    [Header("Editor Settings")]
    [Tooltip("Enables generation of thumbnails and previews")]
    public bool previewEnabled = true;

    public enum LogLevel
    {
        Off,
        Error,
        Warning,
        Information,
        Debug
    }

    [Tooltip("Sets the logging level for general messages")]
    public LogLevel generalLogLevel = LogLevel.Warning;

    [Tooltip("Sets the logging level for data binding")]
    public LogLevel bindingLogLevel = LogLevel.Warning;

    [System.Serializable]
    public struct Cursor
    {
        public Texture2D Texture;
        public Vector2 HotSpot;
    }

    [Tooltip("The cursor that appears when an application is starting")]
    public Cursor AppStarting;
    [Tooltip("The Arrow cursor")]
    public Cursor Arrow;
    [Tooltip("The arrow with a compact disk cursor")]
    public Cursor ArrowCD;
    [Tooltip("The crosshair cursor")]
    public Cursor Cross;
    [Tooltip("A hand cursor")]
    public Cursor Hand;
    [Tooltip("A help cursor which is a combination of an arrow and a question mark")]
    public Cursor Help;
    [Tooltip("An I-beam cursor, which is used to show where the text cursor appears when the mouse is clicked")]
    public Cursor IBeam;
    [Tooltip("A cursor with which indicates that a particular region is invalid for a given operation")]
    public Cursor No;
    [Tooltip("A special cursor that is invisible")]
    public Cursor None;
    [Tooltip("A pen cursor")]
    public Cursor Pen;
    [Tooltip("The scroll all cursor")]
    public Cursor ScrollAll;
    [Tooltip("The scroll east cursor")]
    public Cursor ScrollE;
    [Tooltip("The scroll north cursor")]
    public Cursor ScrollN;
    [Tooltip("The scroll northeast cursor")]
    public Cursor ScrollNE;
    [Tooltip("The scroll north/south cursor")]
    public Cursor ScrollNS;
    [Tooltip("A scroll northwest cursor")]
    public Cursor ScrollNW;
    [Tooltip("The scroll south cursor")]
    public Cursor ScrollS;
    [Tooltip("A south/east scrolling cursor")]
    public Cursor ScrollSE;
    [Tooltip("The scroll southwest cursor")]
    public Cursor ScrollSW;
    [Tooltip("The scroll west cursor")]
    public Cursor ScrollW;
    [Tooltip("A west/east scrolling cursor")]
    public Cursor ScrollWE;
    [Tooltip("A four-headed sizing cursor, which consists of four joined arrows that point north, south, east, and west")]
    public Cursor SizeAll;
    [Tooltip("A two-headed northeast/southwest sizing cursor")]
    public Cursor SizeNESW;
    [Tooltip("A two-headed north/south sizing cursor")]
    public Cursor SizeNS;
    [Tooltip("A two-headed northwest/southeast sizing cursor")]
    public Cursor SizeNWSE;
    [Tooltip("A two-headed west/east sizing cursor")]
    public Cursor SizeWE;
    [Tooltip("An up arrow cursor, which is typically used to identify an insertion point")]
    public Cursor UpArrow;
    [Tooltip("Specifies a wait (or hourglass) cursor")]
    public Cursor Wait;

    [SerializeField]
    private string version = "";

    public string Version
    {
        get { return version; }
        set
        { 
            if (version != value)
            {
                version = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
        }
    }
}
