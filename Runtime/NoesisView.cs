using UnityEngine;
using Noesis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

#if ENABLE_URP_PACKAGE
using UnityEngine.Rendering.Universal;
#endif

#if ENABLE_HDRP_PACKAGE
using UnityEngine.Rendering.HighDefinition;
#endif

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

//[ExecuteInEditMode]
[AddComponentMenu("NoesisGUI/Noesis View")]
[HelpURL("https://www.noesisengine.com/docs")]
[DisallowMultipleComponent]
public class NoesisView: MonoBehaviour, ISerializationCallbackReceiver
{
    #region Public properties

    /// <summary>
    /// User interface definition XAML
    /// </summary>
    public NoesisXaml Xaml
    {
        set { this._xaml = value; }
        get { return this._xaml; }
    }

    /// <summary>
    /// The texture to render this View into
    /// </summary>
    public RenderTexture Texture
    {
        set { this._texture = value; }
        get { return this._texture; }
    }

    /// <summary>
    /// Tessellation curve tolerance in screen space. 'Medium Quality' is usually fine for PPAA (non-multisampled) 
    /// while 'High Quality' is the recommended pixel error if you are rendering to a 8x multisampled surface
    /// </summary>
    public float TessellationMaxPixelError
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetTessellationMaxPixelError(value);
            }

            this._tessellationMaxPixelError = value;
        }

        get
        {
            if (_uiView != null)
            {
                return _uiView.GetTessellationMaxPixelError().Error;
            }

            return this._tessellationMaxPixelError; 
        }
    }

    /// <summary>
    /// Bit flags used for debug rendering purposes.
    /// </summary>
    public RenderFlags RenderFlags
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetFlags(value);
            }

            this._renderFlags = value;
        }
        get
        {
            if (_uiView != null)
            {
                return _uiView.GetFlags();
            }

            return this._renderFlags;
        }
    }

    /// <summary>
    /// When enabled, the view is scaled by the actual DPI of the screen or physical device running the application
    /// </summary>
    public bool DPIScale
    {
        set { this._dpiScale = value; }
        get { return this._dpiScale; }
    }

    /// <summary>
    /// When continuous rendering is disabled, rendering only happens when UI changes. For performance
    /// purposes and to save battery this is the default mode when rendering to texture. If not rendering
    /// to texture, this property is ignored. Use the property 'NeedsRendering' instead.
    /// </summary>
    public bool ContinuousRendering
    {
        set { this._continuousRendering = value; }
        get { return this._continuousRendering; }
    }

    /// <summary>
    /// When enabled, the view must be explicitly updated by calling 'ExternalUpdate()'.
    /// By default, the view is automatically updated during LateUpdate.
    /// </summary>
    public bool EnableExternalUpdate
    {
        set { this._enableExternalUpdate = value; }
        get { return this._enableExternalUpdate; }
    }

    /// <summary>
    /// After updating the view, this flag indicates if the GUI needs to be repainted.
    /// This flag can be used on manually painted cameras to optimize performance and save battery.
    /// </summary>
    public bool NeedsRendering
    {
        set { this._needsRendering = value; }
        get { return this._needsRendering; }
    }

    /// <summary>
    /// Enables keyboard input management.
    /// </summary>
    public bool EnableKeyboard
    {
        set { this._enableKeyboard = value; }
        get { return this._enableKeyboard; }
    }

    /// <summary>
    /// Enables mouse input management.
    /// </summary>
    public bool EnableMouse
    {
        set { this._enableMouse = value; }
        get { return this._enableMouse; }
    }

    /// <summary>
    /// Enables touch input management.
    /// </summary>
    public bool EnableTouch
    {
        set { this._enableTouch = value; }
        get { return this._enableTouch; }
    }

    /// <summary>
    /// Enables gamepad input management.
    /// </summary>
    public bool EnableGamepad
    {
        set { this._enableGamepad = value; }
        get { return this._enableGamepad; }
    }

    /// <summary>
    /// Actions for gamepad events.
    /// </summary>
    public UnityEngine.InputSystem.InputActionAsset GamepadActions
    {
        set { this._actions = value; }
        get { return this._actions; }
    }

    /// <summary>
    /// The initial delay (in seconds) between an initial gamepad action and a repeated gamepad action.
    /// </summary>
    public float GamepadRepeatDelay
    {
        set { this._gamepadRepeatDelay = value; }
        get { return this._gamepadRepeatDelay; }
    }

    /// <summary>
    /// The speed (in seconds) that the gamepad action repeats itself once repeating
    /// </summary>
    public float GamepadRepeatRate
    {
        set { this._gamepadRepeatRate = value; }
        get { return this._gamepadRepeatRate; }
    }

  #if ENABLE_URP_PACKAGE
    /// <summary>
    /// Controls when the UI render pass executes
    /// </summary>
    public RenderPassEvent RenderPassEvent
    {
        set { this._renderPassEvent = value; }
        get { return this._renderPassEvent; }
    }
  #endif

    /// <summary>
    /// Emulate touch input with mouse.
    /// </summary>
    public bool EmulateTouch
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetEmulateTouch(value);
            }

            this._emulateTouch = value;
        }
        get { return this._emulateTouch; }
    }

    /// <summary>
    /// When enabled, UI is updated using Time.realtimeSinceStartup.
    /// </summary>
    public bool UseRealTimeClock
    {
        set { this._useRealTimeClock = value; }
        get { return this._useRealTimeClock; }
    }

    /// <summary>
    /// Gets the root of the loaded Xaml.
    /// </summary>
    /// <returns>Root element.</returns>
    public FrameworkElement Content
    {
        get { return _uiView != null ? _uiView.Content : null; }
    }

    /// <summary>
    /// Indicates if this component is rendering UI to a RenderTexture.
    /// </summary>
    /// <returns></returns>
    public bool IsRenderToTexture()
    {
        return !gameObject.TryGetComponent(out Camera _);
    }

    #endregion

    #region Public events

    #region Render
    public event RenderingEventHandler Rendering
    {
        add
        {
            if (_uiView != null)
            {
                _uiView.Rendering += value;
            }
        }
        remove
        {
            if (_uiView != null)
            {
                _uiView.Rendering -= value;
            }
        }
    }

    public ViewStats GetStats()
    {
        if (_uiView != null)
        {
            return _uiView.GetStats();
        }

        return new ViewStats();
    }
    #endregion

    #region Keyboard input events
    /// <summary>
    /// Notifies Renderer that a key was pressed.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyDown(Noesis.Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyDown(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was released.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyUp(Noesis.Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyUp(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was translated to the corresponding character.
    /// </summary>
    /// <param name="ch">Unicode character value.</param>
    public bool Char(uint ch)
    {
        if (_uiView != null)
        {
            return _uiView.Char(ch);
        }

        return false;
    }
    #endregion

    #region Mouse input events
    /// <summary>
    /// Notifies Renderer that mouse was moved. The mouse position is specified in renderer
    /// surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    public bool MouseMove(int x, int y)
    {
        if (_uiView != null)
        {
            return _uiView.MouseMove(x, y);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a mouse button was pressed. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseButtonDown(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonDown(x, y, button);
        }

        return false;
    }

    /// Notifies Renderer that a mouse button was released. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was released.</param>
    public bool MouseButtonUp(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonUp(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer of a mouse button double click. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseDoubleClick(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseDoubleClick(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that mouse wheel was rotated. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="wheelRotation">Indicates the amount mouse wheel has changed.</param>
    public bool MouseWheel(int x, int y, int wheelRotation)
    {
        if (_uiView != null)
        {
            return _uiView.MouseWheel(x, y, wheelRotation);
        }

        return false;
    }
    #endregion

    #region Touch input events
    /// <summary>
    /// Notifies Renderer that a finger is moving on the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchMove(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchMove(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger touches the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchDown(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchDown(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger is raised off the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchUp(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchUp(x, y, touchId);
        }

        return false;
    }
    #endregion

    #endregion

    #region Public methods

    /// <summary>
    /// Loads the user interface specified in the XAML property
    /// </summary>
    public void LoadXaml(bool force)
    {
        if (force)
        {
            DestroyView();
        }

        if (_xaml != null && _uiView == null)
        {
            FrameworkElement content = _xaml.Load() as FrameworkElement;

            if (content == null)
            {
                throw new System.Exception("XAML root is not FrameworkElement");
            }

            CreateView(content);
        }
    }

    #endregion

    #region Private members

    #region MonoBehavior component messages

    /// <summary>
    /// Called once when component is attached to GameObject for the first time
    /// </summary>
    void Reset()
    {
        _isPPAAEnabled = true;
        _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
        _renderFlags = 0;
        _dpiScale = true;
        _continuousRendering = gameObject.TryGetComponent(out Camera _);
        _enableExternalUpdate = false;
        _enableKeyboard = true;
        _enableMouse = true;
        _enableTouch = true;
        _enableGamepad = false;
        _emulateTouch = false;
        _useRealTimeClock = false;
        _gamepadRepeatDelay = 0.5f;
        _gamepadRepeatRate = 0.1f;
      #if ENABLE_URP_PACKAGE
        _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
      #endif
    }

    void Start()
    {
        // https://forum.unity.com/threads/gc-collect-in-guiutility-begingui.642229/
        // Avoid OnGUI GC Allocations
        useGUILayout = false;

      #if !ENABLE_LEGACY_INPUT_MANAGER
        if (_enableMouse)
        {
            Debug.LogWarning("Mouse enabled requires 'Active Input Handling' set to 'Old' or 'Both' in Player Settings");
        }
      #endif

      #if !ENABLE_LEGACY_INPUT_MANAGER
        if (_enableKeyboard)
        {
            Debug.LogWarning("Keyboard enabled requires 'Active Input Handling' set to 'Old' or 'Both' in Player Settings");
        }
      #endif

      #if !ENABLE_LEGACY_INPUT_MANAGER
        if (_enableTouch)
        {
            Debug.LogWarning("Touch enabled requires 'Active Input Handling' set to 'Old' or 'Both' in Player Settings");
        }
      #endif

      #if !ENABLE_INPUT_SYSTEM
        if (_enableGamepad)
        {
            Debug.LogWarning("Gamepad enabled requires 'Active Input Handling' set to 'New' or 'Both' in Player Settings");
        }
      #endif
    }

    private void EnableActions()
    {
        if (_actions)
        {
            _actions.Enable();

            _upAction = _actions.FindAction("Gamepad/Up");
            _downAction = _actions.FindAction("Gamepad/Down");
            _leftAction = _actions.FindAction("Gamepad/Left");
            _rightAction = _actions.FindAction("Gamepad/Right");

            _acceptAction = _actions.FindAction("Gamepad/Accept");
            _cancelAction = _actions.FindAction("Gamepad/Cancel");

            _menuAction = _actions.FindAction("Gamepad/Menu");
            _viewAction = _actions.FindAction("Gamepad/View");

            _pageLeftAction = _actions.FindAction("Gamepad/PageLeft");
            _pageRightAction = _actions.FindAction("Gamepad/PageRight");
            _pageUpAction = _actions.FindAction("Gamepad/PageUp");
            _pageDownAction = _actions.FindAction("Gamepad/PageDown");

            _vScrollAction = _actions.FindAction("Gamepad/Scroll");
            _hScrollAction = _actions.FindAction("Gamepad/HScroll");
        }
    }

    private void DisableActions()
    {
        if (_actions)
        {
            _actions.Disable();
        }
    }

    private CommandBuffer _commands;

    private void EnsureCommandBuffer()
    {
        if (_commands == null)
        {
            _commands = new CommandBuffer();
        }
    }

    void Awake()
    {
        EnsureCommandBuffer();
    }

    private Camera _camera;

    void OnEnable()
    {
        EnsureCommandBuffer();
        TryGetComponent<Camera>(out _camera);

        EnableActions();
        LoadXaml(false);

        Camera.onPreRender += PreRender;

      #if ENABLE_URP_PACKAGE || ENABLE_HDRP_PACKAGE
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;

        #if ENABLE_URP_PACKAGE
          _scriptableRenderPass = new NoesisScriptableRenderPass(this);
        #endif
      #endif
    }

    void OnDisable()
    {
        DisableActions();

        Camera.onPreRender -= PreRender;

      #if ENABLE_URP_PACKAGE || ENABLE_HDRP_PACKAGE
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
      #endif
    }

    private static class Profiling
    {
        public static readonly CustomSampler UpdateSampler = CustomSampler.Create("Noesis.Update");
        public static readonly CustomSampler RenderOnScreenSampler = CustomSampler.Create("Noesis.RenderOnscreen");
        public static readonly string RegisterView = "Noesis.RegisterView";
        public static readonly string UnregisterView = "Noesis.UnregisterView";
        public static readonly string UpdateRenderTree = "Noesis.UpdateRenderTree";
        public static readonly string RenderOffScreen = "Noesis.RenderOffscreen";
        public static readonly string RenderOnScreen = "Noesis.RenderOnscreen";
        public static readonly string RenderTexture = "Noesis.RenderTexture";
    }

#region Universal Render Pipeline
#if ENABLE_URP_PACKAGE
    private class NoesisScriptableRenderPass: ScriptableRenderPass
    {
        public NoesisScriptableRenderPass(NoesisView view)
        {
            _view = view;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            bool flipY = !IsGL() && !IsBackbuffer(_view._camera);

            _view._commands.name = Profiling.RenderOnScreen;
            NoesisRenderer.RenderOnscreen(_view._uiView, flipY, _view._commands);
            NoesisRenderer.InvalidateState(_view._commands);

            context.ExecuteCommandBuffer(_view._commands);
            _view._commands.Clear();
        }

        private bool IsBackbuffer(Camera camera)
        {
            var scriptableRenderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;

            #if UNITY_2022_1_OR_NEWER
                return camera.targetTexture == null && scriptableRenderer.cameraColorTargetHandle.rt == null;
            #else
                return camera.targetTexture == null && scriptableRenderer.cameraColorTarget == BuiltinRenderTextureType.CameraTarget;
            #endif
        }

        NoesisView _view;
    }

    private NoesisScriptableRenderPass _scriptableRenderPass;

    private void RenderOffscreenUniversal(ScriptableRenderContext context)
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOffScreen;
            NoesisRenderer.RenderOffscreen(_uiView, _commands);
            NoesisRenderer.InvalidateState(_commands);

            context.ExecuteCommandBuffer(_commands);
            _commands.Clear();
        }
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        var cameraData = camera.GetUniversalAdditionalCameraData();

        // To avoid inefficient changes of render target, stacked cameras must render
        // their offscreen phase before the base camera is started
        if (cameraData.renderType == CameraRenderType.Base)
        {
            if (_camera == camera)
            {
                RenderOffscreenUniversal(context);
            }
            else
            {
                foreach (var stackedCamera in cameraData.cameraStack)
                {
                    if (_camera == stackedCamera)
                    {
                        RenderOffscreenUniversal(context);
                        break;
                    }
                }
            }
        }

        if (_camera == camera)
        {
            _scriptableRenderPass.renderPassEvent = _renderPassEvent;
            cameraData.scriptableRenderer.EnqueuePass(_scriptableRenderPass);
        }
    }

    private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
    }
#endif
#endregion

#region High Definition Render Pipeline
#if ENABLE_HDRP_PACKAGE
    // With a CustomPass we can control when the UI is rendered (for example before or after post process)
    // For using this class:
    //  1. Attach a 'Custom Pass Volume' to the camera
    //  2. Set 'Mode' to 'Camera'
    //  3. Set 'Target Camera' to the same camera
    //  4. Select 'Injection Point' (only 'Before Post Process' or 'After Post Process' makes sense)
    //  5. Add a Custom Pass of type 'NoesisCustomPass'
    //  6. Set 'Clear Flags' to 'Stencil'
    private class NoesisCustomPass: CustomPass
    {
        protected override bool executeInSceneView { get { return false; } }

        protected override void Execute(CustomPassContext ctx)
        {
            if (ctx.hdCamera.camera.TryGetComponent(out NoesisView view))
            {
                view.OnExecuteCustomPass(ctx.cmd);
            }
        }
    }

    private bool _usingCustomPass;

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (_camera == camera)
        {
            if (_uiView != null && _visible)
            {
                _commands.name = Profiling.RenderOffScreen;
                NoesisRenderer.RenderOffscreen(_uiView, _commands);
                NoesisRenderer.InvalidateState(_commands);

                context.ExecuteCommandBuffer(_commands);
                _commands.Clear();

                _usingCustomPass = false;
            }
        }
    }

    private void OnExecuteCustomPass(CommandBuffer commands)
    {
        if (!_usingCustomPass)
        {
            if (_uiView != null && _visible)
            {
                // This is always rendering to an intermediate texture
                bool flipY = !IsGL();

                commands.BeginSample(Profiling.RenderOnScreenSampler);
                NoesisRenderer.RenderOnscreen(_uiView, flipY, commands);
                NoesisRenderer.InvalidateState(commands);
                commands.EndSample(Profiling.RenderOnScreenSampler);
            }

            // EndCameraRendering is skipped
            _usingCustomPass = true;
        }
    }

    private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (_camera == camera && !_usingCustomPass)
        {
            if (_uiView != null && _visible)
            {
                _commands.name = Profiling.RenderOnScreen;
                NoesisRenderer.RenderOnscreen(_uiView, FlipRender(), _commands);
                NoesisRenderer.InvalidateState(_commands);

                context.ExecuteCommandBuffer(_commands);
                _commands.Clear();
            }
        }
    }
#endif
#endregion

    void OnDestroy()
    {
        DestroyView();
    }

#if ENABLE_UGUI_PACKAGE
    UnityEngine.EventSystems.PointerEventData _pointerData;
#endif

    private UnityEngine.Vector2 ProjectPointer(float x, float y)
    {
        if (_camera != null)
        {
            return new UnityEngine.Vector2(x, UnityEngine.Screen.height - y);
        }
        else if (_texture != null)
        {
            // Project using texture coordinates

#if ENABLE_UGUI_PACKAGE
            // First try with Unity UI RawImage objects
            UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;

            if (eventSystem != null && eventSystem.IsPointerOverGameObject())
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2(x, y);

                if (_pointerData == null)
                {
                    _pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem)
                    {
                        pointerId = 0,
                        position = pos
                    };
                }
                else
                {
                    _pointerData.Reset();
                }

                _pointerData.delta = pos - _pointerData.position;
                _pointerData.position = pos;

                if (TryGetComponent(out RectTransform rect))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                        _pointerData.position, _pointerData.pressEventCamera, out pos))
                    {
                        UnityEngine.Vector2 pivot = new UnityEngine.Vector2(
                            rect.pivot.x * rect.rect.width,
                            rect.pivot.y * rect.rect.height);

                        float texCoordX = (pos.x + pivot.x) / rect.rect.width;
                        float texCoordY = (pos.y + pivot.y) / rect.rect.height;

                        float localX = _texture.width * texCoordX;
                        float localY = _texture.height * (1.0f - texCoordY);
                        return new UnityEngine.Vector2(localX, localY);
                    }
                }
            }
#endif

            // NOTE: A MeshCollider must be attached to the target to obtain valid
            // texture coordinates, otherwise Hit Testing won't work

            UnityEngine.Ray ray = UnityEngine.Camera.main.ScreenPointToRay(new UnityEngine.Vector3(x, y, 0));

            UnityEngine.RaycastHit hit;
            if (UnityEngine.Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    float localX = _texture.width * hit.textureCoord.x;
                    float localY = _texture.height * (1.0f - hit.textureCoord.y);
                    return new UnityEngine.Vector2(localX, localY);
                }
            }

            return new UnityEngine.Vector2(-1, -1);
        }

        return Vector2.zero;
    }

    private UnityEngine.Vector3 _mousePos;
    private int _activeDisplay = 0;

    private Vector3 MousePosition(Vector3 mousePosition)
    {
        Vector3 p = Display.RelativeMouseAt(mousePosition);

        if (p == Vector3.zero)
        {
            return mousePosition;
        }

        _activeDisplay = (int)p.z;
        return p;
    }

    private void UpdateMouse()
    {
        Vector3 mousePos = MousePosition(UnityEngine.Input.mousePosition);

        // mouse move
        if ((_camera == null || _activeDisplay == _camera.targetDisplay) && _mousePos != mousePos)
        {
            _mousePos = mousePos;

            UnityEngine.Vector2 mouse = ProjectPointer(_mousePos.x, _mousePos.y);

            _uiView.MouseMove((int)mouse.x, (int)mouse.y);
        }
    }

    private void UpdateTouch()
    {
        for (int i = 0; i < UnityEngine.Input.touchCount; i++) 
        {
            UnityEngine.Touch touch = UnityEngine.Input.GetTouch(i);
            uint id = (uint)touch.fingerId;
            UnityEngine.Vector2 pos = ProjectPointer(touch.position.x, touch.position.y);
            UnityEngine.TouchPhase phase = touch.phase;

            if (phase == UnityEngine.TouchPhase.Began)
            {
                _uiView.TouchDown((int)pos.x, (int)pos.y, id);
            }
            else if (phase == UnityEngine.TouchPhase.Moved || phase == UnityEngine.TouchPhase.Stationary)
            {
                _uiView.TouchMove((int)pos.x, (int)pos.y, id);
            }
            else
            {
                _uiView.TouchUp((int)pos.x, (int)pos.y, id);
            }
        }
    }

    [FlagsAttribute] 
    enum GamepadButtons
    {
         Up = 1,
         Down = 2,
         Left = 4,
         Right = 8,
         Accept = 16,
         Cancel = 32,
         Menu = 64,
         View = 128,
         PageUp = 256,
         PageDown = 512,
         PageLeft = 1024,
         PageRight = 2048
    }

    private struct ButtonState
    {
        public GamepadButtons button;
        public Noesis.Key key;
        public float t;
    }

    private ButtonState[] _buttonStates = new ButtonState[]
    {
        new ButtonState { button = GamepadButtons.Up, key = Key.GamepadUp },
        new ButtonState { button = GamepadButtons.Down, key = Key.GamepadDown },
        new ButtonState { button = GamepadButtons.Left, key = Key.GamepadLeft },
        new ButtonState { button = GamepadButtons.Right, key = Key.GamepadRight },
        new ButtonState { button = GamepadButtons.Accept, key = Key.GamepadAccept },
        new ButtonState { button = GamepadButtons.Cancel, key = Key.GamepadCancel },
        new ButtonState { button = GamepadButtons.Menu, key = Key.GamepadMenu},
        new ButtonState { button = GamepadButtons.View, key = Key.GamepadView },
        new ButtonState { button = GamepadButtons.PageUp, key = Key.GamepadPageUp },
        new ButtonState { button = GamepadButtons.PageDown, key = Key.GamepadPageDown },
        new ButtonState { button = GamepadButtons.PageLeft, key = Key.GamepadPageLeft },
        new ButtonState { button = GamepadButtons.PageRight, key = Key.GamepadPageRight },
    };

    private GamepadButtons _gamepadButtons = 0;

    private void UpdateGamepad(float t)
    {
        GamepadButtons gamepadButtons = 0;

        if (_upAction != null && _upAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Up;
        if (_downAction != null && _downAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Down;
        if (_leftAction != null && _leftAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Left;
        if (_rightAction != null && _rightAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Right;

        if (_acceptAction != null && _acceptAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Accept;
        if (_cancelAction != null && _cancelAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Cancel;

        if (_menuAction != null && _menuAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.Menu;
        if (_viewAction != null && _viewAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.View;

        if (_pageUpAction != null && _pageUpAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.PageUp;
        if (_pageDownAction != null && _pageDownAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.PageDown;
        if (_pageLeftAction != null && _pageLeftAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.PageLeft;
        if (_pageRightAction != null && _pageRightAction.ReadValue<float>() > 0.0f) gamepadButtons |= GamepadButtons.PageRight;

        if (_vScrollAction != null)
        {
            float v = _vScrollAction.ReadValue<float>();
            _uiView.Scroll(v);
        }

        if (_hScrollAction != null)
        {
            float v = _hScrollAction.ReadValue<float>();
            _uiView.HScroll(v);
        }

        GamepadButtons delta = gamepadButtons ^ _gamepadButtons;
        if (delta != 0 || gamepadButtons != 0)
        {
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                if ((delta & _buttonStates[i].button) > 0)
                {
                    if ((gamepadButtons & _buttonStates[i].button) > 0)
                    {
                        _uiView.KeyDown(_buttonStates[i].key);
                        _buttonStates[i].t = t + _gamepadRepeatDelay;
                    }
                    else
                    {
                        _uiView.KeyUp(_buttonStates[i].key);
                    }
                }
                else if ((gamepadButtons & _buttonStates[i].button) > 0)
                {
                    if (t >= _buttonStates[i].t)
                    {
                        _uiView.KeyDown(_buttonStates[i].key);
                        _buttonStates[i].t = t + _gamepadRepeatRate;
                    }
                }
            }
        }

         _gamepadButtons = gamepadButtons;
    }

    private void UpdateInputs(float t)
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (_enableMouse)
        {
            UpdateMouse();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (_enableTouch)
        {
            UpdateTouch();
        }
#endif

#if ENABLE_INPUT_SYSTEM
        if (_enableGamepad)
        {
            UpdateGamepad(t);
        }
#endif
    }

    private int _viewSizeX;
    private int _viewSizeY;
    private float _viewScale;

    private void UpdateSize()
    {
        int sizeX = 0;
        int sizeY = 0;

        if (_camera != null)
        {
            sizeX = _camera.pixelWidth;
            sizeY = _camera.pixelHeight;
        }
        else if (_texture != null)
        {
            sizeX = _texture.width;
            sizeY = _texture.height;
        }

        if (sizeX != _viewSizeX || sizeY != _viewSizeY)
        {
            _uiView.SetSize(sizeX, sizeY);
            _viewSizeX = sizeX;
            _viewSizeY = sizeY;
        }

        float scale = (_dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 96.0f : 1.0f;

        if (scale != _viewScale)
        {
            _uiView.SetScale(scale);
            _viewScale = scale;
        }
    }

    private bool _visible = true;

    void LateUpdate()
    {
        if (!_enableExternalUpdate)
        {
            UpdateInternal();
        }
    }

    public void ExternalUpdate()
    {
        Debug.Assert(_enableExternalUpdate, "Calling ExternalUpdate() with EnableExternalUpdate disabled", this);
        UpdateInternal();
    }

    private void UpdateInternal()
    {
        if (_uiView != null && _visible)
        {
            Profiling.UpdateSampler.Begin();

            float t = Time.realtimeSinceStartup;

            UpdateSize();
            UpdateInputs(t);

            NoesisUnity.IME.Update(_uiView);
            NoesisUnity.TouchKeyboard.Update();

            Noesis_UnityUpdate();
            _needsRendering = _uiView.Update(_useRealTimeClock ? t : Time.time);

            Profiling.UpdateSampler.End();

            if (_needsRendering)
            {
                _commands.name = Profiling.UpdateRenderTree;
                NoesisRenderer.UpdateRenderTree(_uiView, _commands);

                Graphics.ExecuteCommandBuffer(_commands);
                _commands.Clear();
            }

            if (_camera == null && _texture != null)
            {
                if (_continuousRendering || _needsRendering)
                {
                    _commands.name = Profiling.RenderTexture;
                    NoesisRenderer.RenderOffscreen(_uiView, _commands);
                    _commands.SetRenderTarget(_texture, LoadAction.DontCare, StoreAction.Store, LoadAction.DontCare, StoreAction.DontCare);
                    _commands.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
                    NoesisRenderer.RenderOnscreen(_uiView, !IsGL(), _commands);

                    Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

                    GL.InvalidateState();
                    _texture.DiscardContents(false, true);
                }
            }
        }
    }

    void OnBecameInvisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = false;
        }
    }

    void OnBecameVisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = true;
        }
    }

    private bool _updatePending = true;

    private void PreRender(Camera cam)
    {
        if (_camera != null)
        {
            // In case there are several cameras rendering to the same texture (Camera Stacking),
            // the camera rendered first (less depth) is the one that must apply our offscreen phase
            // to avoid inefficient Load/Store in Tiled architectures
            if (_updatePending && cam.targetTexture == _camera.targetTexture && cam.depth <= _camera.depth)
            {
                RenderOffscreen();
                _updatePending = false;
            }
        }
    }

    private void ForceRestoreCameraRenderTarget()
    {
        // Unity should automatically restore the render target but sometimes (for example a scene without lights)
        // it doesn't. We use this hack to flush the active render target and force unity to set the camera RT afterward
        RenderTexture surface = RenderTexture.GetTemporary(1,1);
        Graphics.SetRenderTarget(surface);
        RenderTexture.ReleaseTemporary(surface);
    }

    private void RenderOffscreen()
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOffScreen;
            NoesisRenderer.RenderOffscreen(_uiView, _commands);

            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            GL.InvalidateState();
            ForceRestoreCameraRenderTarget();
        }
    }

    private static bool IsGL()
    {
        var type = SystemInfo.graphicsDeviceType;
        return type == GraphicsDeviceType.OpenGLES2 || type == GraphicsDeviceType.OpenGLES3
            || type == GraphicsDeviceType.OpenGLCore;
    }

    private bool FlipRender()
    {
        // In D3D when Unity is rendering to an intermediate texture instead of the back buffer, we need to vertically flip the output
        // Note that camera.activeTexture should only be checked from OnPostRender
        if (!IsGL())
        {
            return _camera.activeTexture != null;
        }

        return false;
    }

    private void RenderOnscreen()
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOnScreen;
            NoesisRenderer.RenderOnscreen(_uiView, FlipRender(), _commands);

            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            GL.InvalidateState();
            _updatePending = true;
        }
    }

    private void OnPostRender()
    {
        RenderOnscreen();
    }

    private UnityEngine.EventModifiers _modifiers = 0;

    private void ProcessModifierKey(EventModifiers modifiers, EventModifiers delta, EventModifiers flag, Noesis.Key key)
    {
        if ((delta & flag) > 0)
        {
            if ((modifiers & flag) > 0)
            {
                _uiView.KeyDown(key);
            }
            else
            {
                _uiView.KeyUp(key);
            }
        }
    }

    private bool HitTest(float x, float y)
    {
        Visual root = (Visual)VisualTreeHelper.GetRoot(_uiView.Content);
        Point p = root.PointFromScreen(new Point(x, y));
        return VisualTreeHelper.HitTest(root, p).VisualHit != null;
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
    private static int lastFrame;
    private static Noesis.Key lastKeyDown;
#endif

    private void ProcessEvent(UnityEngine.Event ev, bool enableKeyboard, bool enableMouse)
    {
        // Process keyboard modifiers
        if (enableKeyboard)
        {
            EventModifiers delta = ev.modifiers ^ _modifiers;
            if (delta > 0)
            {
                _modifiers = ev.modifiers;

                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Shift, Key.LeftShift);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Control, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Command, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Alt, Key.LeftAlt);
            }
        }

        switch (ev.type)
        {
            case UnityEngine.EventType.MouseDown:
            {
                if (enableMouse)
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    // Ignore events generated by Unity to simulate a mouse down when a touch event occurs
                    bool mouseEmulated = Input.simulateMouseWithTouches && Input.touchCount > 0;
                    if (!mouseEmulated)
                    {
                        if (ev.clickCount == 1)
                        {
                            _uiView.MouseButtonDown((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                        }
                        else
                        {
                            _uiView.MouseDoubleClick((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                        }
                    }
                }
                break;
            }
            case UnityEngine.EventType.MouseUp:
            {
                if (enableMouse)
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    // Ignore events generated by Unity to simulate a mouse up when a touch event occurs
                    bool mouseEmulated = Input.simulateMouseWithTouches && Input.touchCount > 0;
                    if (!mouseEmulated)
                    {
                        _uiView.MouseButtonUp((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                    }
                }
                break;
            }
            case UnityEngine.EventType.ScrollWheel:
            {
                if (enableMouse)
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (ev.delta.y != 0.0f)
                    {
                        _uiView.MouseWheel((int)mouse.x, (int)mouse.y, -(int)(ev.delta.y * 40.0f));
                    }

                    if (ev.delta.x != 0.0f)
                    {
                        _uiView.MouseHWheel((int)mouse.x, (int)mouse.y, (int)(ev.delta.x * 40.0f));
                    }
                }
                break;
            }
            case UnityEngine.EventType.KeyDown:
            {
                if (enableKeyboard)
                {
                    // Don't process key when IME composition is being used
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Noesis.Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Noesis.Key.None)
                        {
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            // In OSX Standalone, CMD + key always sends two KeyDown events for the key.
                            // This seems to be a bug in Unity. 
                            if (!ev.command || lastFrame != Time.frameCount || lastKeyDown != noesisKeyCode)
                            {
                                lastFrame = Time.frameCount;
                                lastKeyDown = noesisKeyCode;
#endif
                                _uiView.KeyDown(noesisKeyCode);
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            }
#endif
                        }
                    }

                    if (ev.character != 0)
                    {
                        // Filter out character events when CTRL is down
                        bool isControl = (_modifiers & EventModifiers.Control) != 0 || (_modifiers & EventModifiers.Command) != 0;
                        bool isAlt = (_modifiers & EventModifiers.Alt) != 0;
                        bool filter = isControl && !isAlt;

                        if (!filter)
                        {
#if !UNITY_EDITOR && UNITY_STANDALONE_LINUX
                            // It seems that linux is sending KeySyms instead of Unicode points
                            // https://github.com/substack/node-keysym/blob/master/data/keysyms.txt
                            ev.character = NoesisKeyCodes.KeySymToUnicode(ev.character);
#endif
                            _uiView.Char((uint)ev.character);
                        }
                    }

                }
                break;
            }
            case UnityEngine.EventType.KeyUp:
            {
                // Don't process key when IME composition is being used
                if (enableKeyboard)
                {
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Noesis.Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Noesis.Key.None)
                        {
                            _uiView.KeyUp(noesisKeyCode);
                        }
                    }
                }
                break;
            }
        }
    }

    void OnGUI()
    {
        if (_uiView != null && (_camera == null || _activeDisplay == _camera.targetDisplay))
        {
            if (_camera)
            {
                UnityEngine.GUI.depth = -(int)_camera.depth;
            }

#if ENABLE_LEGACY_INPUT_MANAGER
            ProcessEvent(UnityEngine.Event.current, _enableKeyboard, _enableMouse);
#endif
        }
    }

    void OnApplicationFocus(bool focused)
    {
        if (_uiView != null)
        {
            if (NoesisUnity.TouchKeyboard.keyboard == null)
            {
                if (focused)
                {
                    _uiView.Activate();
                }
                else
                {
                    _uiView.Deactivate();
                }
            }
        }
    }
#endregion

    private void CreateView(FrameworkElement content)
    {
        if (_uiView == null)
        {
            // Send settings for the internal device, created by the first view
            NoesisRenderer.SetRenderSettings();

            _viewSizeX = 0;
            _viewSizeY = 0;
            _viewScale = 1.0f;

            _uiView = new Noesis.View(content);
            _uiView.SetTessellationMaxPixelError(_tessellationMaxPixelError);
            _uiView.SetEmulateTouch(_emulateTouch);
            _uiView.SetFlags(_renderFlags);

            _commands.name = Profiling.RegisterView;
            NoesisRenderer.RegisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += DestroyView;
#endif
        }
    }

    private void DestroyView()
    {
        if (_uiView != null)
        {
            _commands.name = Profiling.UnregisterView;
            NoesisRenderer.UnregisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            _uiView = null;
        }
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize()
    {
        // (3.0) PPAA flag is now in view render flags 
        if (_isPPAAEnabled)
        {
            _renderFlags |= RenderFlags.PPAA;
            _isPPAAEnabled = false;
        }
    }

    private Noesis.View _uiView;
    private bool _needsRendering = false;

#region Serialized properties
    [SerializeField] private NoesisXaml _xaml;
    [SerializeField] private RenderTexture _texture;

    [SerializeField] private bool _isPPAAEnabled = true;
    [SerializeField] private float _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
    [SerializeField] private RenderFlags _renderFlags = 0;
    [SerializeField] private bool _dpiScale = true;
    [SerializeField] private bool _continuousRendering = true;
    [SerializeField] private bool _enableExternalUpdate = false;
    [SerializeField] private bool _enableKeyboard = true;
    [SerializeField] private bool _enableMouse = true;
    [SerializeField] private bool _enableTouch = true;
    [SerializeField] private bool _enableGamepad = false;
    [SerializeField] private bool _emulateTouch = false;
    [SerializeField] private bool _useRealTimeClock = false;

    [SerializeField] private UnityEngine.InputSystem.InputActionAsset _actions;
    [SerializeField] private float _gamepadRepeatDelay = 0.5f;
    [SerializeField] private float _gamepadRepeatRate = 0.1f;

  #if ENABLE_URP_PACKAGE
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
  #endif

    private UnityEngine.InputSystem.InputAction _upAction;
    private UnityEngine.InputSystem.InputAction _downAction;
    private UnityEngine.InputSystem.InputAction _leftAction;
    private UnityEngine.InputSystem.InputAction _rightAction;

    private UnityEngine.InputSystem.InputAction _acceptAction;
    private UnityEngine.InputSystem.InputAction _cancelAction;

    private UnityEngine.InputSystem.InputAction _menuAction;
    private UnityEngine.InputSystem.InputAction _viewAction;

    private UnityEngine.InputSystem.InputAction _pageLeftAction;
    private UnityEngine.InputSystem.InputAction _pageRightAction;
    private UnityEngine.InputSystem.InputAction _pageUpAction;
    private UnityEngine.InputSystem.InputAction _pageDownAction;

    private UnityEngine.InputSystem.InputAction _vScrollAction;
    private UnityEngine.InputSystem.InputAction _hScrollAction;
#endregion

#region Imports
    [DllImport(Library.Name)]
    private static extern void Noesis_UnityUpdate();
#endregion

#endregion
}