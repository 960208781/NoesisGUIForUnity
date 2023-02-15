using System.Runtime.InteropServices;
using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

/// <summary>
/// In Unity, the render thread is only accesible in C++ using IssuePluginEvent(). This is a helper
/// class to communicate a C# view with its C++ renderer.
/// </summary>
public class NoesisRenderer
{
    /// <summary>
    /// Registers a view in the render thread
    /// </summary>
    public static void RegisterView(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        commands.IssuePluginEventAndData(_renderRegisterCallback, 0, view.CPtr.Handle);
    }

    /// <summary>
    /// Sends render tree update commands to native code
    /// </summary>
    public static void UpdateRenderTree(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        // Send information about requested textures to C++ texture provider
        NoesisTextureProvider.instance.UpdateTextures();

        commands.IssuePluginEventAndData(_updateRenderTreeCallback, (int)EventId.UpdateRenderTree, view.CPtr.Handle);
    }

    /// <summary>
    /// Sends offscreen render commands to native code
    /// </summary>
    public static void RenderOffscreen(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        // This a way to force Unity to close the current MTL command encoder
        // We need to activate a new encoder in the current command buffer for our Offscreen phase
        if (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            UnityEngine.RenderTexture surface = UnityEngine.RenderTexture.GetTemporary(1,1);
            commands.SetRenderTarget(surface, LoadAction.DontCare, StoreAction.DontCare, LoadAction.DontCare, StoreAction.DontCare);
            commands.ClearRenderTarget(false, false, UnityEngine.Color.clear);
            UnityEngine.RenderTexture.ReleaseTemporary(surface);
        }

        commands.IssuePluginEventAndData(_renderOffscreenCallback, (int)EventId.RenderOffscreen, view.CPtr.Handle);
    }

    /// <summary>
    /// Sends render commands to native code
    /// </summary>
    public static void RenderOnscreen(Noesis.View view, bool flipY, UnityEngine.Rendering.CommandBuffer commands)
    {
        // This is a workaround for a bug in Unity. When rendering nothing Unity sends us an empty MTLRenderCommandEncoder
        if (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            commands.DrawMesh(_dummyMesh, new UnityEngine.Matrix4x4(), _dummyMaterial);
        }

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenCallback, (int)eventId, view.CPtr.Handle);
    }

    /// <summary>
    /// CommandBuffer equivalent to GL.InvalidateState
    /// </summary>
    public static void InvalidateState(UnityEngine.Rendering.CommandBuffer commands)
    {
        // There is nothing equivalent to GL.InvalidateState() for the command buffer
        // But as IssuePluginEvent() invalidates the state, invoking an empty C++ callback gives the same effect
        // Note that IssuePluginEventAndData() does not invalidate the state
        commands.IssuePluginEvent(_invalidateStateCallback, 0);
    }

    /// <summary>
    /// Creates a custom pixel shader
    /// </summary>
    public static System.IntPtr CreatePixelShader(byte shader, int numTextures, byte[] bytes)
    {
        if (bytes != null)
        {
            if (_nextShaderId == 0)
            {
                // At domain reload _nextShaderId is reset, let's keep in sync with the render thread
                _commands.IssuePluginEventAndData(_renderClearShadersCallback, 0, System.IntPtr.Zero);
            }

            System.IntPtr ptr = Noesis_AllocateNative(bytes.Length + 12);

            int[] args = new int[] { shader, numTextures, bytes.Length };
            Marshal.Copy(args, 0, ptr, 3);
            Marshal.Copy(bytes, 0, ptr + 12, bytes.Length);
            _commands.IssuePluginEventAndData(_renderCreateShaderCallback, 0, ptr);

            UnityEngine.Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            return (System.IntPtr)(++_nextShaderId);
        }

        return System.IntPtr.Zero;
    }

    /// <summary>
    /// Unregisters given renderer
    /// </summary>
    public static void UnregisterView(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        commands.IssuePluginEventAndData(_renderUnregisterCallback, 0, view.CPtr.Handle);
    }

    /// <summary>
    /// Updates render settings
    /// </summary>
    public static void SetRenderSettings()
    {
        NoesisSettings settings = NoesisSettings.Get();

        bool linearRendering = false;

        switch (settings.linearRendering)
        {
            case NoesisSettings.LinearRendering._SamesAsUnity:
            {
                linearRendering = UnityEngine.QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear;
                break;
            }
            case NoesisSettings.LinearRendering._Enabled:
            {
                linearRendering = true;
                break;
            }
            case NoesisSettings.LinearRendering._Disabled:
            {
                linearRendering = false;
                break;
            }
        }

        int sampleCount = 1;

        switch (settings.offscreenSampleCount)
        {
            case NoesisSettings.OffscreenSampleCount._SameAsUnity:
            {
                sampleCount = UnityEngine.QualitySettings.antiAliasing;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._1x:
            {
                sampleCount = 1;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._2x:
            {
                sampleCount = 2;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._4x:
            {
                sampleCount = 4;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._8x:
            {
                sampleCount = 8;
                break;
            }
        }

        uint offscreenDefaultNumSurfaces = settings.offscreenInitSurfaces;
        uint offscreenMaxNumSurfaces = settings.offscreenMaxSurfaces;

        int glyphCacheTextureWidth = 1024;
        int glyphCacheTextureHeight = 1024;

        switch (settings.glyphTextureSize)
        {
            case NoesisSettings.TextureSize._256x256:
            {
                glyphCacheTextureWidth = 256;
                glyphCacheTextureHeight = 256;
                break;
            }
            case NoesisSettings.TextureSize._512x512:
            {
                glyphCacheTextureWidth = 512;
                glyphCacheTextureHeight = 512;
                break;
            }
            case NoesisSettings.TextureSize._1024x1024:
            {
                glyphCacheTextureWidth = 1024;
                glyphCacheTextureHeight = 1024;
                break;
            }
            case NoesisSettings.TextureSize._2048x2048:
            {
                glyphCacheTextureWidth = 2048;
                glyphCacheTextureHeight = 2048;
                break;
            }
            case NoesisSettings.TextureSize._4096x4096:
            {
                glyphCacheTextureWidth = 4096;
                glyphCacheTextureHeight = 4096;
                break;
            }
        }

        Noesis_RendererSettings(linearRendering, sampleCount, offscreenDefaultNumSurfaces,
            offscreenMaxNumSurfaces, glyphCacheTextureWidth, glyphCacheTextureHeight);
    }

    #region Private
    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderRegisterCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetUpdateRenderTreeCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOffscreenCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOnscreenCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetInvalidateStateCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_AllocateNative(int size);

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderClearShadersCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderCreateShaderCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderUnregisterCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern void Noesis_RendererSettings(bool linearSpaceRendering, int offscreenSampleCount,
        uint offscreenDefaultNumSurfaces, uint offscreenMaxNumSurfaces, int glyphCacheTextureWidth, int glyphCacheTextureHeight);

    // Keep this in sync with UnityDevice in C++
    private enum EventId
    {
        UpdateRenderTree = 0x6446, 
        RenderOffscreen = 0x6447,
        RenderOnscreen = 0x6448,
        RenderOnscreenFlipY = 0x6449
    }

    private static uint _nextShaderId;
    private static UnityEngine.Rendering.CommandBuffer _commands = new UnityEngine.Rendering.CommandBuffer();
    private static System.IntPtr _renderRegisterCallback = Noesis_GetRenderRegisterCallback();
    private static System.IntPtr _updateRenderTreeCallback = Noesis_GetUpdateRenderTreeCallback();
    private static System.IntPtr _renderOffscreenCallback = Noesis_GetRenderOffscreenCallback();
    private static System.IntPtr _renderOnscreenCallback = Noesis_GetRenderOnscreenCallback();
    private static System.IntPtr _invalidateStateCallback = Noesis_GetInvalidateStateCallback();
    private static System.IntPtr _renderClearShadersCallback = Noesis_GetRenderClearShadersCallback();
    private static System.IntPtr _renderCreateShaderCallback = Noesis_GetRenderCreateShaderCallback();
    private static System.IntPtr _renderUnregisterCallback = Noesis_GetRenderUnregisterCallback();

    private static UnityEngine.Material _dummyMaterial = new UnityEngine.Material(UnityEngine.Shader.Find("UI/Default"));
    private static UnityEngine.Mesh _dummyMesh = CreateDummyMesh();

    private static UnityEngine.Mesh CreateDummyMesh()
    {
        _dummyMesh  = new UnityEngine.Mesh();
        _dummyMesh.vertices = new UnityEngine.Vector3[3];
        _dummyMesh.vertices[0] = new UnityEngine.Vector3(0, 0, 0);
        _dummyMesh.vertices[1] = new UnityEngine.Vector3(0, 0, 0);
        _dummyMesh.vertices[2] = new UnityEngine.Vector3(0, 0, 0);
        _dummyMesh.triangles = new int[3] { 0, 2, 1 };
        return _dummyMesh;
    }

    #endregion
}