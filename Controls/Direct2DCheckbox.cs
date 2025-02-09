// Source: Direct2DCheckbox
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

using Engine.Editor.UI.Events;
using SharpDX;
using SharpDX.Mathematics.Interop;
using System;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DCheckbox : WindowContext
    {
        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;
                    this.OnCheckedChanged(this, EventArgs.Empty);
                }
            }
        }


        public event EventHandler CheckedChanged;


        private bool _isChecked;
        private Size2F _metrics;
        private RawRectangleF _labelRect;

        protected override void OnTextChanged(object sender, EventArgs e)
        {
            this._metrics = this.MeasureString(this.Text);

            base.OnTextChanged(sender, e);
        }

        protected virtual void OnCheckedChanged(object sender, EventArgs e) { this.CheckedChanged?.Invoke(sender, e); }


        protected override void OnMouseButtonUp(object sender, SdlMouseEventArgs e)
        {
            var pt = this.PointToClient(e.X, e.Y);
            if (this.ClientRectangle.Contains(pt.X, pt.Y) ||
                (pt.X > this._labelRect.Left && pt.Y > this._labelRect.Top && pt.X < this._labelRect.Right && pt.Y < this._labelRect.Bottom))
            {
                this.IsChecked = !this.IsChecked;
            }

            base.OnMouseButtonUp(sender, e);
        }


        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            var padding = 0;
            var checkRect = new RawRectangleF(
                this.Left + padding,
                this.Top + padding,
                this.Left + this.Height - padding,
                this.Top + this.Height - padding);

            var isCheckRect = new RawRectangleF(
                checkRect.Left + 2,
                checkRect.Top + 2,
                checkRect.Right - 2,
                checkRect.Bottom - 2);

            var oldRoundValue = this.BrushAccent.Radius;
            this.BrushAccent.Radius = 1;
            this.FillStyle(
                checkRect,
                this.BrushBorder,
                this.BorderColor,
                BorderStyle.Rounded);

            if (this._isChecked)
            {
                this.FillStyle(
                    isCheckRect,
                    this.BrushAccent,
                    this.AccentColor,
                    BorderStyle.Rounded);
            }

            if (!string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(this.FontName))
            {
                this._labelRect = new RawRectangleF(
                    checkRect.Right + 4,
                    this.Top,
                    checkRect.Right + (this._metrics.Width + this._metrics.Width),
                    checkRect.Bottom);

                this.RenderTarget.DrawText(
                    this.Text,
                    this.Font.GetFont(this.FontName, this.Font.FontSize, this.Font.FontWeight, this.Font.FontStyle),
                    this._labelRect,
                    this.BrushFont.GetSolidColorBrush(this.FontColor));
            }

            this.BrushAccent.Radius = oldRoundValue;
        }

    }
}
