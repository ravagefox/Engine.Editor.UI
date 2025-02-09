// Source: Direct2DOverlayGraph
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warranties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using Engine.Core;
using Engine.Editor.UI.Events;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DOverlayGraph : WindowContext
    {
        public bool IsResizable { get; set; } = true;


        private const int MAX_SAMPLES = 200; // Rolling buffer size
        private const float UPDATE_INTERVAL = 0.1f; // Seconds

        public Dictionary<string, List<float>> InputData { get; private set; } = new Dictionary<string, List<float>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, RawColor4> InputColors { get; private set; } = new Dictionary<string, RawColor4>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> InputVisibility { get; private set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // Toggle input visibility

        private float _timeElapsed = 0;
        private readonly Dictionary<string, float> _currentState = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, uint> _previousFlags = new Dictionary<string, uint>();
        private readonly float _lineThickness = 2f;

        private const float EDGE_THRESHOLD = 15.0f; // Resize area thickness
        private bool _isResizing = false;
        private ResizeDirection _resizeDir = ResizeDirection.None;
        private PointF _resizeStartPos;

        private enum ResizeDirection
        {
            None,
            Left, Right, Top, Bottom,
            TopLeft, TopRight, BottomLeft, BottomRight
        }


        public Direct2DOverlayGraph()
        {
            this.Width = 400;
            this.Height = 150;
            this.BackgroundColor = new RawColor4(0.1f, 0.1f, 0.1f, 0.8f);
            this.BorderColor = new RawColor4(1f, 1f, 1f, 1f);
        }

        protected override void OnMouseLeave(object sender, EventArgs e)
        {
            this._isResizing = false;

            this.SetCursorForResize(ResizeDirection.None);
            base.OnMouseLeave(sender, e);
        }

        protected override void OnMouseMove(object sender, SdlMouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (this.IsResizable)
            {
                var pt = this.PointToClient(e.X, e.Y);
                this._resizeDir = this.GetResizeDirection(pt.X, pt.Y);

                this.SetCursorForResize(this._resizeDir);
                if (!this._isResizing)
                {
                    // Change cursor based on region
                }
                else
                {
                    // Perform resize
                    this.ResizeControl(e.X, e.Y);
                }
            }
        }
        protected override void OnMouseButtonDown(object sender, SdlMouseEventArgs e)
        {
            base.OnMouseButtonDown(sender, e);

            if (this.IsResizable)
            {
                var pt = this.PointToClient(e.X, e.Y);
                this._resizeDir = this.GetResizeDirection(pt.X, pt.Y);

                if (this._resizeDir != ResizeDirection.None)
                {
                    this._isResizing = true;
                    this._resizeStartPos = new PointF(e.X, e.Y);
                }
            }
        }
        protected override void OnMouseButtonUp(object sender, SdlMouseEventArgs e)
        {
            base.OnMouseButtonUp(sender, e);
            this._isResizing = false;
            this._resizeDir = ResizeDirection.None;
        }

        private void ResizeControl(float mouseX, float mouseY)
        {
            var dx = mouseX - this._resizeStartPos.X;
            var dy = mouseY - this._resizeStartPos.Y;

            switch (this._resizeDir)
            {
                case ResizeDirection.Right:
                    this.Width = Mathf.Max(50, this.Width + dx);
                    break;
                case ResizeDirection.Bottom:
                    this.Height = Mathf.Max(50, this.Height + dy);
                    break;
                case ResizeDirection.BottomRight:
                    this.Width = Mathf.Max(50, this.Width + dx);
                    this.Height = Mathf.Max(50, this.Height + dy);
                    break;
                    // Handle other edges...
            }

            this._resizeStartPos = new PointF(mouseX, mouseY);
        }
        private ResizeDirection GetResizeDirection(float x, float y)
        {
            var left = x <= this.Left + EDGE_THRESHOLD;
            var right = x >= this.Right - EDGE_THRESHOLD;
            var top = y <= this.Top + EDGE_THRESHOLD;
            var bottom = y >= this.Bottom - EDGE_THRESHOLD;

            if (left && top)
            {
                return ResizeDirection.TopLeft;
            }

            if (right && top)
            {
                return ResizeDirection.TopRight;
            }

            if (left && bottom)
            {
                return ResizeDirection.BottomLeft;
            }

            if (right && bottom)
            {
                return ResizeDirection.BottomRight;
            }

            return left
                ? ResizeDirection.Left
                : right ? ResizeDirection.Right : top ? ResizeDirection.Top : bottom ? ResizeDirection.Bottom : ResizeDirection.None;
        }
        private void SetCursorForResize(ResizeDirection dir)
        {
            switch (dir)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    SDL2.SDL.SDL_SetCursor(SDL2.SDL.SDL_CreateSystemCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE));
                    break;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    SDL2.SDL.SDL_SetCursor(SDL2.SDL.SDL_CreateSystemCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS));
                    break;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    SDL2.SDL.SDL_SetCursor(SDL2.SDL.SDL_CreateSystemCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE));
                    break;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:
                    SDL2.SDL.SDL_SetCursor(SDL2.SDL.SDL_CreateSystemCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW));
                    break;

                default:
                    SDL2.SDL.SDL_SetCursor(SDL2.SDL.SDL_CreateSystemCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW));
                    break;
            }
        }



        protected override void OnInitialized(object sender, EventArgs e)
        {
            base.OnInitialized(sender, e);
        }

        public void RegisterInput(string inputName, RawColor4 color, SDL2.SDL.SDL_Keycode keycode)
        {
            if (this.InputData.ContainsKey(inputName)) { return; }

            this.InputData[inputName] = new List<float>(new float[MAX_SAMPLES]);
            this.InputColors[inputName] = color;
            this.InputVisibility[inputName] = true; // Default to visible
            this._currentState[inputName] = 0;

            this.KeyDown += new EventHandler<SdlKeyEventArgs>((s, e) =>
            {
                if (e.Keycode == keycode)
                {
                    this.UpdateInput(inputName, true);
                }
            });
            this.KeyUp += new EventHandler<SdlKeyEventArgs>((s, e) =>
            {
                if (e.Keycode == keycode && this.GetState(inputName))
                {
                    this.UpdateInput(inputName, false);
                }
            });
        }
        public void RegisterInput(string inputName, RawColor4 color, byte mouseButton)
        {
            if (this.InputData.ContainsKey(inputName)) { return; }

            this.InputData[inputName] = new List<float>(new float[MAX_SAMPLES]);
            this.InputColors[inputName] = color;
            this.InputVisibility[inputName] = true; // Default to visible
            this._currentState[inputName] = 0;

            this.MouseButtonDown += new EventHandler<SdlMouseEventArgs>((s, e) =>
            {
                if (e.ButtonEvent.button == mouseButton)
                {
                    this.UpdateInput(inputName, true);
                }
            });
            this.MouseButtonUp += new EventHandler<SdlMouseEventArgs>((s, e) =>
            {
                if (e.ButtonEvent.button == mouseButton && this.GetState(inputName))
                {
                    this.UpdateInput(inputName, false);
                }
            });
        }
        public void RegisterInput(string inputName, RawColor4 color, uint flagState, uint flags)
        {
            if (this.InputData.ContainsKey(inputName)) { return; }

            this.InputData[inputName] = new List<float>(new float[MAX_SAMPLES]);
            this.InputColors[inputName] = color;
            this.InputVisibility[inputName] = true; // Default to visible
            this._currentState[inputName] = 0;

            this.Update += new EventHandler((s, e) =>
            {
                uint previousState = _previousFlags.ContainsKey(inputName) ? _previousFlags[inputName] : 0;
                uint newState = flagState & flags;

                if (previousState != newState)
                {
                    this.UpdateInput(inputName, newState != 0);
                    _previousFlags[inputName] = newState;
                }
            });
        }

        public void ToggleInput(string inputName, bool isVisible)
        {
            if (this.InputVisibility.ContainsKey(inputName))
            {
                this.InputVisibility[inputName] = isVisible;
            }
        }

        public void UpdateInput(string inputName, bool isActive)
        {
            if (this._currentState.ContainsKey(inputName))
            {
                this._currentState[inputName] = isActive ? 1f : 0f;
            }
        }

        public bool GetState(string inputName)
        {
            return this._currentState.ContainsKey(inputName) && this._currentState[inputName] != 0;
        }

        protected override void OnPaint(object sender, EventArgs e)
        {
            if (this._isResizing) { return; }

            this._timeElapsed += UPDATE_INTERVAL;

            foreach (var key in this._currentState.Keys)
            {
                if (!this.InputVisibility[key])
                {
                    continue; // Skip hidden inputs
                }

                var data = this.InputData[key];

                // Shift the graph left
                for (var i = 1; i < MAX_SAMPLES; i++)
                {
                    data[i - 1] = data[i]; // Mathf.Lerp(data[i - 1], data[i], 0.5f); // Smooth interpolation
                }

                // Add new value
                data[MAX_SAMPLES - 1] = this._currentState[key];

                // Draw graph line
                this.DrawGraph(key, data);
            }
        }

        private void DrawGraph(string inputName, List<float> data)
        {
            var colorBrush = this.BrushBorder.GetSolidColorBrush(this.InputColors[inputName]);

            var step = this.Width / MAX_SAMPLES;
            var centerY = this.Top + (this.Height / 2);

            for (var i = 1; i < MAX_SAMPLES; i++)
            {
                var x1 = this.Left + ((i - 1) * step);
                var x2 = this.Left + (i * step);
                var y1 = centerY - (data[i - 1] * (this.Height / 2));
                var y2 = centerY - (data[i] * (this.Height / 2));

                this.RenderTarget.DrawLine(
                    new RawVector2(x1, y1),
                    new RawVector2(x2, y2),
                    colorBrush,
                    this._lineThickness);
            }
        }

    }
}
