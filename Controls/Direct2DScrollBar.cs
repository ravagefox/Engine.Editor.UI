// Source: Direct2DScrollBar
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
using Engine.Editor.UI.Events;
using Engine.Editor.UI.Extensions;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DScrollbar : WindowContext
    {
        public float Min { get; set; } = 0;
        public float Max { get; set; } = 100;
        public float Value { get; set; } = 0;
        public bool IsMouseOver { get; private set; }

        public event EventHandler ScrollChanged;

        private bool _isDragging = false;
        private float _lastY;

        public Direct2DScrollbar()
        {
            this.Width = 10;
            this.Height = 100;
            this.BackgroundColor = new RawColor4(0.3f, 0.3f, 0.3f, 1f);
            this.BorderColor = SystemColors.ActiveCaption.ToRawColor4();
        }

        protected override void OnMouseButtonDown(object sender, SdlMouseEventArgs e)
        {
            this._isDragging = true;
            this._lastY = e.Y;
        }

        protected override void OnMouseButtonUp(object sender, SdlMouseEventArgs e)
        {
            this._isDragging = false;
        }

        protected override void OnMouseMove(object sender, SdlMouseEventArgs e)
        {
            var pt = this.PointToClient(e.X, e.Y);
            this.IsMouseOver = this.ClientRectangle.Contains(pt.X, pt.Y);

            if (this._isDragging)
            {
                var deltaY = e.Y - this._lastY;
                this.Value = Mathf.Clamp(this.Value + deltaY, this.Min, this.Max);
                ScrollChanged?.Invoke(this, EventArgs.Empty);
                this._lastY = e.Y;
            }
        }

        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            var solidBrush = this.BrushControl.GetSolidColorBrush(this.BackgroundColor);
            this.RenderTarget.FillRectangle(this.Bounds, solidBrush);

            var thumbHeight = Mathf.Max(20, this.Height * (this.Height / (this.Max - this.Min + this.Height)));
            var thumbTop = this.Top + (this.Value / this.Max * (this.Height - thumbHeight));

            this.RenderTarget.FillRectangle(
                new RawRectangleF(this.Left, thumbTop, this.Right, thumbTop + thumbHeight),
                this.BrushBorder.GetSolidColorBrush(this.BorderColor));
        }
    }

}
