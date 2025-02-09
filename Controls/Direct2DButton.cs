// Source: Direct2DButton
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

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Windows.Forms.VisualStyles;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DButton : WindowContext
    {
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

        public bool AutoSize { get; set; }

        private Size2F _metrics;
        private RawRectangleF _textRect;


        protected override void OnTextChanged(object sender, EventArgs e)
        {
            this._metrics = this.MeasureString(this.Text);

            base.OnTextChanged(sender, e);
        }

        protected override void OnInitialized(object sender, EventArgs e)
        {
            base.OnInitialized(sender, e);
        }

        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            if (this.AutoSize)
            {
                if (this._metrics.Width > float.Epsilon || this._metrics.Height > float.Epsilon)
                {
                    this.Width = this._metrics.Width;
                    this.Height = this._metrics.Height;
                }
            }

            this._textRect = this.CreateRenderBounds();
            var solidFill = this.BrushControl.GetSolidColorBrush(this.BackgroundColor);
            var border = this.BrushBorder.GetSolidColorBrush(this.BorderColor);

            if (this.BrushBorder.Radius > 0)
            {
                var rRect = new RoundedRectangle { RadiusX = this.BrushBorder.Radius, RadiusY = this.BrushBorder.Radius, Rect = this.Bounds };
                this.RenderTarget.FillRoundedRectangle(
                    rRect, solidFill);
                this.RenderTarget.DrawRoundedRectangle(
                    rRect,
                    border,
                    this.BrushBorder.Thickness);
            }
            else
            {
                this.RenderTarget.FillRectangle(
                    this.Bounds, solidFill);

                this.RenderTarget.FillRectangle(
                    new RawRectangleF(this.Left - this.BrushBorder.Thickness,
                                      this.Top - this.BrushBorder.Thickness,
                                      this.Right + this.BrushBorder.Thickness,
                                      this.Bottom + this.BrushBorder.Thickness),
                    border);
            }

        }

        protected override void OnPaint(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text)
                && !string.IsNullOrEmpty(this.FontName)
                && this.Font.FontSize > 0)
            {
                var font = this.Font.GetFont(
                    this.FontName,
                    this.Font.FontSize,
                    this.Font.FontWeight,
                    this.Font.FontStyle);

                this.RenderTarget.DrawText(
                    this.Text,
                    font,
                    this._textRect,
                    this.BrushFont.GetSolidColorBrush(this.FontColor));
            }

            base.OnPaint(sender, e);
        }

        private RawRectangleF CreateRenderBounds()
        {
            var rect = new RawRectangleF();
            var textWidth = this._metrics.Width;
            var textHeight = this._metrics.Height;

            // Handle Horizontal Alignment
            switch (this.TextAlignment)
            {
                case TextAlignment.Center:
                    rect.Left = this.Left + ((this.Width - textWidth) / 2.0f);
                    break;
                case TextAlignment.Trailing:
                    rect.Left = this.Right - textWidth;
                    break;
                case TextAlignment.Leading:
                    rect.Left = this.Left;
                    break;
            }

            // Handle Vertical Alignment
            switch (this.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    rect.Top = this.Top + ((this.Height - textHeight) / 2.0f);
                    break;
                case VerticalAlignment.Bottom:
                    rect.Top = this.Bottom - textHeight;
                    break;
                case VerticalAlignment.Top:
                    rect.Top = this.Top;
                    break;
            }

            // Adjust Right and Bottom
            rect.Right = rect.Left + textWidth;
            rect.Bottom = rect.Top + textHeight;

            return rect;
        }

    }
}
