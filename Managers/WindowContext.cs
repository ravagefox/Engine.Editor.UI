// Source: WindowContext
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using Engine.Core;
using Engine.Editor.UI.Controls;
using Engine.Editor.UI.Events;
using Engine.Editor.UI.Managers;
using SDL2;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Engine.Editor.UI
{
    public class WindowContext : IMsgTranslator, IDisposable
    {
        #region Properties

        public float Width { get; set; }

        public float Height { get; set; }

        public BorderStyle BorderStyle
        {
            get => this._borderStyle;
            set
            {
                if (this._borderStyle != value)
                {
                    this._borderStyle = value;
                    this.OnBorderStyleChanged(this, EventArgs.Empty);
                }
            }
        }

        public IntPtr Handle
        {
            get => this._handle;
            internal set
            {
                if (this._firstShown && this._handle != value)
                {
                    this._handle = value;
                    this.OnHandleCreated(this, EventArgs.Empty);
                }
                else
                {
                    if (this._handle != IntPtr.Zero
                        && value == IntPtr.Zero)
                    {
                        this._handle = IntPtr.Zero;
                        this.OnHandleDestroyed(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool Visible
        {
            get => this._visible;
            set
            {
                if (this._visible != value)
                {
                    this._visible = value;
                    this.OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public PointF Location
        {
            get => this._location;
            set
            {
                if (this._location != value)
                {
                    this._location = value;
                    this.OnLocationChanged(this, EventArgs.Empty);
                }
            }
        }

        public PointF Size
        {
            get => new PointF(this.Width, this.Height);
            set
            {
                if (this.Size.X != value.X || this.Size.Y != value.Y)
                {
                    this.Width = value.X;
                    this.Height = value.Y;

                    this.OnSizeChanged(this, EventArgs.Empty);
                }
            }
        }

        public RectangleF ClientRectangle
        {
            get => new RectangleF(this.Location.X, this.Location.Y, this.Size.X, this.Size.Y);
            set
            {
                var cliRect = this.ClientRectangle;
                if (cliRect.X != value.X ||
                    cliRect.Y != value.Y ||
                    cliRect.Width != value.Width ||
                    cliRect.Height != value.Height)
                {
                    this.Location = new PointF(value.X, value.Y);
                    this.Size = new PointF(value.Width, value.Height);
                }
            }
        }

        public bool Enabled
        {
            get => this._enabled;
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;
                    this.OnEnabledChanged(this, EventArgs.Empty);
                }
            }
        }

        protected Direct2DScrollbar VerticalScroll { get; private set; }

        public float Left => (float)this.Location.X;
        public float Top => (float)this.Location.Y;
        public float Right => (float)this.Left + this.Width;
        public float Bottom => (float)this.Top + this.Height;

        protected RawRectangleF Bounds => new RawRectangleF(this.Left, this.Top, this.Right, this.Bottom);

        public FontManager Font { get; private set; } = new FontManager() { FontSize = 16.0f, FontStyle = SharpDX.DirectWrite.FontStyle.Normal, FontWeight = FontWeight.Normal };

        public string FontName { get; set; } = "Segoe UI";

        public int TabOrder
        {
            get => this._tabOrder;
            set
            {
                if (this._tabOrder != value)
                {
                    this._tabOrder = value;
                    this.OnTabOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        public string Text
        {
            get => this._text;
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.OnTextChanged(this, EventArgs.Empty);
                }
            }
        }

        public BrushManager BrushControl { get; internal set; }
        public BrushManager BrushBorder { get; internal set; }
        public BrushManager BrushAccent { get; internal set; }
        public BrushManager BrushShadow { get; internal set; }
        public BrushManager BrushFont { get; internal set; }


        public RawColor4 BackgroundColor { get; set; }
        public RawColor4 BorderColor { get; set; }
        public RawColor4 FontColor { get; set; }
        public RawColor4 AccentColor { get; set; }


        public RawRectangleF Padding { get; set; } = new RawRectangleF(5, 5, 5, 5);

        public string Name
        {
            get => this._name;
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    this.OnNameChanged(this, EventArgs.Empty);
                }
            }
        }

        public List<WindowContext> Controls { get; private set; } = new List<WindowContext>();
        public WindowContext Parent { get; private set; }

        protected WindowRenderTarget RenderTarget { get; private set; }

        public static WindowContext ActiveControl { get; private set; }
        #endregion

        #region Fields
        public event EventHandler Shown;
        public event EventHandler VisibleChanged;
        public event EventHandler HandleCreated;
        public event EventHandler HandleDestroyed;
        public event EventHandler LocationChanged;
        public event EventHandler SizeChanged;
        public event EventHandler Disposing;
        public event EventHandler WindowSizeChanged;
        public event EventHandler EnabledChanged;
        public event EventHandler TabOrderChanged;
        public event EventHandler BorderStyleChanged;
        public event EventHandler Initialized;
        public event EventHandler ActiveControlChanged;
        public event EventHandler NameChanged;
        public event EventHandler TextChanged;
        public event EventHandler Update;

        public event EventHandler<SdlMouseEventArgs> MouseButtonDown;
        public event EventHandler<SdlMouseEventArgs> MouseButtonUp;
        public event EventHandler<SdlMouseEventArgs> MouseMove;
        public event EventHandler<SdlMouseEventArgs> MouseWheel;
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;

        public event EventHandler<SdlKeyEventArgs> KeyDown;
        public event EventHandler<SdlKeyEventArgs> KeyUp;


        private IntPtr _handle;
        private bool _visible = false;
        private bool _enabled = true;
        private bool _firstShown = true;
        private PointF _location;
        private int _tabOrder = -1;
        private BorderStyle _borderStyle = BorderStyle.Rounded;
        private bool disposedValue;

        private string _text = string.Empty;
        private uint _windowId = unchecked((uint)-1);
        private float _dpiScale = 1.0f;
        private bool _hasEntered;
        private string _name = string.Empty;

        #endregion

        #region Constructors

        #endregion

        #region Protected Methods
        protected virtual void OnUpdate(object sender, EventArgs e) { this.Update?.Invoke(sender, e); }

        protected virtual void OnTextChanged(object sender, EventArgs e)
        {
            if (WindowContextManager.Instance.IsWindow(this))
            {
                SDL.SDL_SetWindowTitle(this.Handle, this.Text);
            }

            this.TextChanged?.Invoke(sender, e);
        }
        protected virtual void OnNameChanged(object sender, EventArgs e) { this.NameChanged?.Invoke(sender, e); }
        protected virtual void OnActiveControlChanged(object sender, EventArgs e) { this.ActiveControlChanged?.Invoke(sender, e); }
        protected virtual void OnBorderStyleChanged(object sender, EventArgs e) { this.BorderStyleChanged?.Invoke(sender, e); }
        protected virtual void OnInitialized(object sender, EventArgs e) { this.Initialized?.Invoke(sender, e); }
        protected virtual void OnTabOrderChanged(object sender, EventArgs e) { this.TabOrderChanged?.Invoke(sender, e); }
        protected virtual void OnEnabledChanged(object sender, EventArgs e) { this.EnabledChanged?.Invoke(sender, e); }
        protected virtual void OnSizeChanged(object sender, EventArgs e) { this.SizeChanged?.Invoke(sender, e); }
        protected virtual void OnLocationChanged(object sender, EventArgs e) { this.LocationChanged?.Invoke(sender, e); }
        protected virtual void OnHandleCreated(object sender, EventArgs e) { this.HandleCreated?.Invoke(sender, e); }
        protected virtual void OnHandleDestroyed(object sender, EventArgs e) { this.HandleDestroyed?.Invoke(sender, e); }
        protected virtual void OnVisibleChanged(object sender, EventArgs e) { this.VisibleChanged?.Invoke(this, e); }
        protected virtual void OnShown(object sender, EventArgs e) { this.Shown?.Invoke(sender, e); }
        protected virtual void OnWindowSizeChanged(object sender, EventArgs e) { this.WindowSizeChanged?.Invoke(sender, e); }
        protected virtual void OnDispose(object sender, EventArgs e) { this.Disposing?.Invoke(sender, e); }

        protected virtual void OnMouseButtonDown(object sender, SdlMouseEventArgs e) { this.MouseButtonDown?.Invoke(sender, e); }
        protected virtual void OnMouseButtonUp(object sender, SdlMouseEventArgs e) { this.MouseButtonUp?.Invoke(sender, e); }
        protected virtual void OnMouseMove(object sender, SdlMouseEventArgs e) { this.MouseMove?.Invoke(sender, e); }
        protected virtual void OnMouseWheel(object sender, SdlMouseEventArgs e) { this.MouseWheel?.Invoke(sender, e); }
        protected virtual void OnMouseEnter(object sender, EventArgs e) { this.MouseEnter?.Invoke(sender, e); }
        protected virtual void OnMouseLeave(object sender, EventArgs e) { this.MouseLeave?.Invoke(sender, e); }

        protected virtual void OnKeyDown(object sender, SdlKeyEventArgs e) { this.KeyDown?.Invoke(sender, e); }
        protected virtual void OnKeyUp(object sender, SdlKeyEventArgs e) { this.KeyUp?.Invoke(sender, e); }

        protected virtual void OnTranslateMsg(SDL.SDL_Event e) { }
        protected virtual void OnPaintBackground(object sender, EventArgs e)
        {
            var startColor = this.BackgroundColor;
            var endColor = new RawColor4(0, 0, 0, 1.0f);

            if (!this.Enabled)
            {
                startColor.R *= 0.3f;
                startColor.G *= 0.3f;
                startColor.B *= 0.3f;
            }

            var startPoint = new RawVector2(this.Location.X, this.Location.Y);
            var endPoint = new RawVector2(this.Bounds.Right, this.Bounds.Bottom);

            var color = this.BrushControl.GetLinearGradientBrush(
                startColor,
                endColor,
                startPoint, // Offset by location
                endPoint); // Extend based on size

            color.StartPoint = startPoint;
            color.EndPoint = endPoint;

            this.FillStyle(
                this.Bounds,
                this.BrushControl,
                BackgroundColor,
                this.BorderStyle);

            this.DrawBorder(
                this.Bounds,
                this.BrushBorder,
                this.BorderColor,
                this.BorderStyle);

        }


        protected virtual void OnPaint(object sender, EventArgs e) { }


        protected SharpDX.Size2F MeasureString(string text)
        {
            var metrics = new SharpDX.Size2F();

            if (string.IsNullOrEmpty(text)) { return metrics; }
            if (this.Font.FontSize < float.Epsilon) { return metrics; }

            var font = this.Font.GetFont(this.FontName, this.Font.FontSize, this.Font.FontWeight, this.Font.FontStyle);
            var factory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated);

            using (var layout = new TextLayout(factory, text, font, this.Bounds.Right - this.Bounds.Left, this.Bounds.Bottom - this.Bounds.Top))
            {
                metrics = new SharpDX.Size2F(layout.Metrics.Width, layout.Metrics.Height);
                layout.Dispose();
            }

            factory?.Dispose();
            return metrics;
        }

        protected internal void DrawBorder(
            RawRectangleF bounds,
            BrushManager brush,
            RawColor4 color,
            BorderStyle style)
        {
            // Dynamically calculate a proportionate radius
            var baseRadius = 6.0f;  // Minimum radius for small controls
            var scaleFactor = 0.15f; // Percentage of the smallest dimension (Width/Height)
            var maxRadius = 24.0f; // Maximum radius to prevent excessive rounding

            var minDimension = Mathf.Min(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            var scaledRadius = Mathf.Clamp(Mathf.Log(minDimension * scaleFactor), baseRadius, maxRadius);

            // Draw Border
            var solidBrush = brush.GetSolidColorBrush(color);
            switch (style)
            {
                case BorderStyle.Rounded:
                    this.RenderTarget.DrawRoundedRectangle(
                        new RoundedRectangle()
                        {
                            RadiusX = scaledRadius,
                            RadiusY = scaledRadius,
                            Rect = bounds // Relative bounds
                        },
                        solidBrush,
                        brush.Thickness);
                    break;
                case BorderStyle.Single:
                    this.RenderTarget.DrawRectangle(
                        bounds, // Relative bounds
                        solidBrush,
                        brush.Thickness);
                    break;
            }
        }

        protected internal void FillStyle(
            RawRectangleF bounds,
            BrushManager brush,
            RawColor4 brushColor,
            BorderStyle style)
        {
            // Dynamically calculate a proportionate radius
            var baseRadius = 6.0f;  // Minimum radius for small controls
            var scaleFactor = 0.15f; // Percentage of the smallest dimension (Width/Height)
            var maxRadius = 24.0f; // Maximum radius to prevent excessive rounding

            var minDimension = Mathf.Min(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            var scaledRadius = Mathf.Clamp(Mathf.Log(minDimension * scaleFactor), baseRadius, maxRadius);

            // Draw Border
            var solidBrush = brush.GetSolidColorBrush(brushColor);
            switch (style)
            {
                case BorderStyle.Rounded:
                    this.RenderTarget.FillRoundedRectangle(
                        new RoundedRectangle()
                        {
                            RadiusX = scaledRadius,
                            RadiusY = scaledRadius,
                            Rect = bounds // Relative bounds
                        },
                        solidBrush);
                    break;
                case BorderStyle.Single:
                    this.RenderTarget.DrawRectangle(
                        bounds, // Relative bounds
                        solidBrush,
                        this.BrushBorder.Thickness);
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void AddChild(WindowContext context)
        {
            this.Controls.Add(context);

            context.Parent = this;
        }

        public void RemoveChild(WindowContext context)
        {
            if (this.Controls.Remove(context))
            {
                context.Parent = null;
            }
        }

        public void Paint()
        {
            if (this._firstShown) { return; }
            if (this._visible && this.RenderTarget != null)
            {
                this.OnUpdate(this, EventArgs.Empty);

                this.OnPaintBackground(this, EventArgs.Empty);
                this.OnPaint(this, EventArgs.Empty);

                foreach (var child in this.Controls)
                {
                    child.Paint();
                }

                if (this.VerticalScroll.Visible)
                {
                    this.VerticalScroll.Width = 15;
                    this.VerticalScroll.Height = this.Height;
                    this.VerticalScroll.Location = new PointF(this.Right - this.VerticalScroll.Width, this.Top);

                    this.VerticalScroll.Paint();
                }
            }
        }

        public void Translate(SDL.SDL_Event e)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN)
                        {
                            var isMe = SDL.SDL_GetWindowID(this._handle);
                            if (this._firstShown && isMe == e.window.windowID)
                            {
                                this.Initialize();

                                this._windowId = isMe;

                                this.ApplyDPIScaling(); // Reapply scaling when window resizes
                                this._firstShown = false;
                            }
                        }
                        else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                        {
                            if (e.window.windowID == this._windowId)
                            {
                                WindowContextManager.Instance.SetActiveWindow(this);
                            }
                        }
                        else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN)
                        {
                            if (e.window.windowID == this._windowId)
                            {
                                this.Visible = false;
                            }
                        }
                        else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {

                            if (e.window.windowID == this._windowId)
                            {
                                var newWidth = e.window.data1;
                                var newHeight = e.window.data2;

                                this.Width = newWidth;
                                this.Height = newHeight;

                                this.RenderTarget.Resize(
                                    new SharpDX.Size2(newWidth, newHeight));

                                this.OnWindowSizeChanged(this, EventArgs.Empty);
                                this.Initialize();
                            }

                            this.ApplyDPIScaling(); // Reapply scaling when window resizes
                        }
                        else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                        {
                            var isMe = SDL.SDL_GetWindowID(this._handle);
                            if (e.window.windowID == isMe)
                            {
                                this.Close();
                                this.Dispose();
                                return;
                            }
                        }
                        else
                        {
                            //Console.WriteLine(e.window.windowEvent);
                        }
                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        var x = e.button.x;
                        var y = e.button.y;
                        var mouseCoord = this.PointToClient(x, y);

                        if (this.ClientRectangle.Contains(mouseCoord.X, mouseCoord.Y) && this.IsTopSelected())
                        {
                            var mse = new SdlMouseEventArgs(e.button);
                            this.OnMouseButtonDown(this, mse);
                        }
                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        var x = e.button.x;
                        var y = e.button.y;
                        var mouseCoord = this.PointToClient(x, y);

                        if (this.IsTopSelected() && this.ClientRectangle.Contains(mouseCoord.X, mouseCoord.Y))
                        {
                            var mse = new SdlMouseEventArgs(e.button);
                            this.OnMouseButtonUp(this, mse);

                            if (!WindowContextManager.Instance.IsWindow(this))
                            {
                                if (ActiveControl != this)
                                {
                                    ActiveControl = this;
                                    this.OnActiveControlChanged(this, EventArgs.Empty);
                                }

                            }

                        }
                        else if (ActiveControl == this && !WindowContextManager.Instance.IsWindow(this))
                        {
                            ActiveControl = null;
                            this.OnActiveControlChanged(this, EventArgs.Empty);
                        }

                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        var x = e.motion.x;
                        var y = e.motion.y;

                        var mouseCoord = this.PointToClient(x, y);

                        if (this.ClientRectangle.Contains(mouseCoord.X, mouseCoord.Y))
                        {
                            if (!this._hasEntered) { this.OnMouseEnter(this, EventArgs.Empty); this._hasEntered = true; }

                            var mse = new SdlMouseEventArgs(e.motion);
                            this.OnMouseMove(this, mse);
                        }
                        else if (this._hasEntered) { this.OnMouseLeave(this, EventArgs.Empty); this._hasEntered = false; }
                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    {
                        var x = e.wheel.x;
                        var y = e.wheel.y;

                        var mouseCoord = this.PointToClient(x, y);
                        if (this.ClientRectangle.Contains(mouseCoord.X, mouseCoord.Y))
                        {
                            var mse = new SdlMouseEventArgs(e.wheel);
                            this.OnMouseWheel(this, mse);
                        }
                    }
                    break;

                case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        if (ActiveControl == this)
                        {
                            var kse = new SdlKeyEventArgs(e.key);
                            this.OnKeyDown(this, kse);
                        }
                    }
                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    {
                        if (ActiveControl == this)
                        {
                            var kse = new SdlKeyEventArgs(e.key);
                            this.OnKeyUp(this, kse);
                        }
                    }
                    break;
            }

            foreach (var child in this.Controls)
            {
                child.Translate(e);
            }

            this.OnTranslateMsg(e);
        }

        private bool IsTopSelected()
        {
            if (this.Parent == null) { return false; }

            var mouseMask = SDL.SDL_GetMouseState(out var mouseX, out var mouseY);
            var window = WindowContextManager.Instance.GetActiveWindow();
            if (window == null) { return false; }

            if ((SDL.SDL_GetWindowFlags(window.Handle) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS) == 0)
            {
                mouseX = mouseY = -1;
                return false;
            }

            // Convert mouse coordinates to local window space
            var pointToClient = this.PointToClient(mouseX, mouseY);

            // Get all controls that contain the mouse
            var allControls = this.GetControls(window.Controls)
                .Where(c => c.ClientRectangle.Contains(pointToClient.X, pointToClient.Y))
                .Reverse()
                .OrderByDescending(c => c.TabOrder) // Sort by Z-index or TabOrder
                .ToList();

            // If no controls match, this one must be top
            if (!allControls.Any())
            {
                return true;
            }

            // If this is the top-most control, return true
            var ctrl = allControls.First();
            return ctrl == this;
        }

        public void Close()
        {
            if (this._handle != IntPtr.Zero
                && WindowContextManager.Instance.IsWindow(this))
            {
                this._visible = false;

                WindowContextManager.Instance.Destroy(ref this._handle);

                if (this._handle == IntPtr.Zero)
                {
                    this.OnHandleDestroyed(this, EventArgs.Empty);
                }

            }
        }


        public void BringToFront()
        {
            if (this._windowId == uint.MaxValue)
            {
                if (this.Parent != null && this.Parent.Controls.Remove(this))
                {
                    this.Parent.Controls.Insert(0, this);
                }
            }
        }

        public void SendToBack()
        {
            if (this._windowId == uint.MaxValue)
            {
                if (this.Parent != null && this.Parent.Controls.Remove(this))
                {
                    this.Parent.Controls.Insert(
                        this.Parent.Controls.Count - 1, this);
                }
            }
        }

        public RawVector2 PointToClient(float x, float y)
        {
            var activeWindow = WindowContextManager.Instance.GetActiveWindow();
            if (activeWindow == null)
            {
                return new RawVector2(float.NaN, float.NaN);
            }

            var xscaled = x * (float)(this.RenderTarget.Size.Width / activeWindow.Width) * this._dpiScale;
            var yscaled = y * (float)(this.RenderTarget.Size.Height / activeWindow.Height) * this._dpiScale;

            return new RawVector2(xscaled, yscaled);
        }

        #endregion

        #region Private Methods

        private IEnumerable<WindowContext> GetControls(IEnumerable<WindowContext> controls)
        {
            if (controls == null || !controls.Any()) { yield break; }
            foreach (var control in controls)
            {
                yield return control;

                var children = new List<WindowContext>();
                if (control.Controls.Count > 0)
                {
                    children.AddRange(this.GetControls(control.Controls));
                    foreach (var child in children)
                    {
                        yield return child;
                    }
                }
            }
        }

        private void ApplyDPIScaling()
        {
            this._dpiScale = DPIManager.GetDPIScaleFactor(this.Handle);

            // Apply DPI scaling to window size
            if (this._windowId != uint.MaxValue)
            {
                this.Width *= this._dpiScale;
                this.Height *= this._dpiScale;
            }

            // Apply DPI scaling to controls inside the window
            foreach (var control in this.Controls)
            {
                control.Width = (int)(control.Width * this._dpiScale);
                control.Height = (int)(control.Height * this._dpiScale);
                control.Location = new PointF(control.Location.X * this._dpiScale, control.Location.Y * this._dpiScale);

                // Apply scaling to fonts
                control.Font.FontSize *= this._dpiScale;
            }

            if (this.VerticalScroll == null) { this.VerticalScroll = new Direct2DScrollbar(); }

            this.VerticalScroll.Width = (int)(this.VerticalScroll.Width * this._dpiScale);
            this.VerticalScroll.Height = this.Height;
            this.VerticalScroll.Location = new PointF(this.Right, this.Top);
        }

        private void Initialize()
        {
            this._visible = true;

            if (this.RenderTarget == null && WindowContextManager.Instance.IsWindow(this))
            {
                this.RenderTarget = WindowContextManager.Instance.CreateGraphics(this.Handle);
            }

            if (!(this is Direct2DScrollbar))
            {
                this.VerticalScroll = new Direct2DScrollbar();
                this.VerticalScroll.RenderTarget = this.RenderTarget;
                this.VerticalScroll.Initialize();
            }

            if (string.IsNullOrEmpty(this._name))
            {
                this._name = this.GetType().Name;
            }

            this.BrushAccent = new BrushManager(this.RenderTarget);
            this.BrushBorder = new BrushManager(this.RenderTarget);
            this.BrushControl = new BrushManager(this.RenderTarget);
            this.BrushFont = new BrushManager(this.RenderTarget);
            this.BrushShadow = new BrushManager(this.RenderTarget);

            foreach (var child in this.Controls)
            {
                child.RenderTarget = this.RenderTarget;
                child.Initialize();

                child._firstShown = false;
            }

            this.OnInitialized(this, EventArgs.Empty);
        }



        #endregion

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.OnDispose(this, EventArgs.Empty);

                    foreach (var child in this.Controls)
                    {
                        child?.Dispose();
                    }

                    this.Controls.Clear();
                    this.Controls = null;

                    this.RenderTarget?.Dispose();
                    this.RenderTarget = null;

                    this.BrushAccent?.Dispose();
                    this.BrushBorder?.Dispose();
                    this.BrushControl?.Dispose();
                    this.BrushFont?.Dispose();
                    this.BrushShadow?.Dispose();

                    this.BrushAccent = null;
                    this.BrushBorder = null;
                    this.BrushControl = null;
                    this.BrushFont = null;
                    this.BrushShadow = null;

                    this.Font?.Dispose();
                    this.Font = null;
                }

                this.disposedValue = true;
                this._visible = false;
                this._enabled = false;
                this.Handle = IntPtr.Zero;
            }
        }

        ~WindowContext()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.Collect();
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
