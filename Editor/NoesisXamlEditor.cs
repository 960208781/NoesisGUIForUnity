//#define DEBUG_IMPORTER

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Noesis;
using UnityEngine.Video;

[CustomEditor(typeof(NoesisXaml))]
public class NoesisXamlEditor: Editor
{
    private Noesis.View _viewPreview;
    private Noesis.View _viewPreviewGUI;
    private UnityEngine.Rendering.CommandBuffer _commands;

    private bool IsGL()
    {
        return
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
    }

    private void CreatePreviewView()
    {
        try
        {
            NoesisXaml xaml = (NoesisXaml)target;
            FrameworkElement root = xaml.Load() as FrameworkElement;
            _viewPreview = Noesis.GUI.CreateView(root);
            _viewPreview.SetFlags(IsGL() ? 0 : RenderFlags.FlipY);
            _viewPreview.SetTessellationMaxPixelError(Noesis.TessellationMaxPixelError.HighQuality.Error);

            NoesisRenderer.RegisterView(_viewPreview, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    private void CreatePreviewGUIView()
    {
        try
        {
            // Mute messages when generating thumbnails to avoid duplicated messages
            NoesisUnity.MuteLog();

            NoesisXaml xaml = (NoesisXaml)target;
            FrameworkElement root = xaml.Load() as FrameworkElement;
            _viewPreviewGUI = Noesis.GUI.CreateView(root);
            _viewPreviewGUI.SetFlags(IsGL() ? 0 : RenderFlags.FlipY);
            _viewPreviewGUI.SetTessellationMaxPixelError(Noesis.TessellationMaxPixelError.HighQuality.Error);

            NoesisRenderer.RegisterView(_viewPreviewGUI, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            NoesisUnity.UnmuteLog();
        }
    }

    public void OnEnable()
    {
        if (_commands == null)
        {
            _commands = new UnityEngine.Rendering.CommandBuffer();
        }
    }

    public void OnDisable()
    {
        if (_viewPreview != null)
        {
            NoesisRenderer.UnregisterView(_viewPreview, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();
            _viewPreview = null;
        }

        if (_viewPreviewGUI != null)
        {
            NoesisRenderer.UnregisterView(_viewPreviewGUI, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();
            _viewPreviewGUI = null;
        }
    }

    private bool _showTextures = true;
    private bool _showSprites = true;
    private bool _showFonts = true;
    private bool _showAudios = true;
    private bool _showVideos = true;
    private bool _showXAMLs = true;
    private bool _showShaders = true;

    public override void OnInspectorGUI()
    {
        NoesisXaml xaml = (NoesisXaml)target;

        bool enabled = UnityEngine.GUI.enabled;
        UnityEngine.GUI.enabled = true;
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "XAML Dependencies", EditorStyles.label); 

        _showTextures = EditorGUILayout.Foldout(_showTextures, $"Textures ({xaml.textures.Length})", false);
        if (_showTextures && xaml.textures != null)
        {
            foreach (var texture in xaml.textures)
            {
                EditorGUILayout.ObjectField(texture.texture, typeof(Texture2D), false);
            }
        }

        _showSprites = EditorGUILayout.Foldout(_showSprites, $"Sprites ({xaml.sprites.Length})", false);
        if (_showSprites && xaml.sprites != null)
        {
            foreach (var sprite in xaml.sprites)
            {
                EditorGUILayout.ObjectField(sprite.sprite, typeof(Sprite), false);
            }
        }

        _showFonts = EditorGUILayout.Foldout(_showFonts, $"Fonts ({xaml.fonts.Length})", false);
        if (_showFonts && xaml.fonts != null)
        {
            foreach (var font_ in xaml.fonts)
            {
                EditorGUILayout.ObjectField(font_.font, typeof(NoesisFont), false);
            }
        }

        _showAudios = EditorGUILayout.Foldout(_showAudios, $"Audios ({xaml.audios.Length})", false);
        if (_showAudios && xaml.audios != null)
        {
            foreach (var audio_ in xaml.audios)
            {
                EditorGUILayout.ObjectField(audio_.audio, typeof(AudioClip), false);
            }
        }

        _showVideos = EditorGUILayout.Foldout(_showVideos, $"Videos ({xaml.videos.Length})", false);
        if (_showVideos && xaml.videos != null)
        {
            foreach (var video_ in xaml.videos)
            {
                EditorGUILayout.ObjectField(video_.video, typeof(VideoClip), false);
            }
        }

        _showXAMLs = EditorGUILayout.Foldout(_showXAMLs, $"XAMLs ({xaml.xamls.Length})", false);
        if (_showXAMLs && xaml.xamls != null)
        {
            foreach (var xaml_ in xaml.xamls)
            {
                EditorGUILayout.ObjectField(xaml_.xaml, typeof(NoesisXaml), false);
            }
        }

        _showShaders = EditorGUILayout.Foldout(_showShaders, $"Shaders ({xaml.shaders.Length})", false);
        if (_showShaders && xaml.shaders != null)
        {
            foreach (var shader_ in xaml.shaders)
            {
                EditorGUILayout.ObjectField(shader_.shader, typeof(NoesisShader), false);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        UnityEngine.GUI.enabled = enabled;
    }

    private bool CanRender()
    {
        NoesisXaml xaml = (NoesisXaml)target;
        return xaml != null && NoesisSettings.Get().previewEnabled;
    }

    public override bool HasPreviewGUI()
    {
        // Wait for script compilation before showing preview as XAML could have code-behind dependencies
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return false;
        }

        if (!CanRender())
        {
            return false;
        }

        if (_viewPreview == null)
        {
            CreatePreviewView();
        }

        if (_viewPreview == null || _viewPreview.Content == null)
        {
            return false;
        }

        return true;
    }

    public override void OnPreviewGUI(UnityEngine.Rect r, GUIStyle background)
    {
        if (Event.current.type == EventType.Repaint)
        {
            if (CanRender())
            {
                if (r.width > 4 && r.height > 4)
                {
                    if (_viewPreview != null && _viewPreview.Content != null)
                    {
                        UnityEngine.GUI.DrawTexture(r, RenderPreview(_viewPreview, (int)r.width, (int)r.height));
                    }
                }
            }
        }
    }

    private enum RenderMode
    {
        Normal,
        Wireframe,
        Batches,
        Overdraw
    }

    private RenderMode _renderMode = RenderMode.Normal;

    public override void OnPreviewSettings()
    {
        _renderMode = (RenderMode)EditorGUILayout.EnumPopup(_renderMode);

        if (_viewPreview != null)
        {
            RenderFlags flags = IsGL() ? 0 : RenderFlags.FlipY;

            if (_renderMode == RenderMode.Normal)
            {
                _viewPreview.SetFlags(flags);
            }
            else if (_renderMode == RenderMode.Wireframe)
            {
                _viewPreview.SetFlags(flags | RenderFlags.Wireframe);
            }
            else if (_renderMode == RenderMode.Batches)
            {
                _viewPreview.SetFlags(flags | RenderFlags.ColorBatches);
            }
            else if (_renderMode == RenderMode.Overdraw)
            {
                _viewPreview.SetFlags(flags | RenderFlags.Overdraw);
            }
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        if (CanRender())
        {
            if (_viewPreviewGUI == null)
            {
                CreatePreviewGUIView();
            }

            if (_viewPreviewGUI != null && _viewPreviewGUI.Content != null)
            {
                #if DEBUG_IMPORTER
                    Debug.Log($"=> RenderStaticPreview {assetPath}");
                #endif

                RenderTexture rt = RenderPreview(_viewPreviewGUI, width, height);

                if (rt != null)
                {
                    RenderTexture prev = RenderTexture.active;
                    RenderTexture.active = rt;

                    Texture2D tex = new Texture2D(width, height);
                    tex.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);
                    tex.Apply(true);

                    RenderTexture.active = prev;
                    return tex;
                }
            }
        }

        return null;
    }

    private RenderTexture RenderPreview(Noesis.View view, int width, int height)
    {
        try
        {
            if (CanRender() && view != null && view.Content != null)
            {
                NoesisRenderer.SetRenderSettings();

                view.SetSize(width, height);
                view.Update(0.0f);

                NoesisRenderer.UpdateRenderTree(view, _commands);
                NoesisRenderer.RenderOffscreen(view, _commands);

                RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
                _commands.SetRenderTarget(rt);
                _commands.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
                NoesisRenderer.RenderOnscreen(view, false, _commands);

                Graphics.ExecuteCommandBuffer(_commands);
                _commands.Clear();

                // D3D12 gets corrupted if we invalidate
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12)
                {
                    GL.InvalidateState();
                }

                RenderTexture.ReleaseTemporary(rt);
                return rt;
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        return null;
    }
}
