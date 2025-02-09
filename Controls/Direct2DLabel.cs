// Source: Direct2DLabel
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
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Windows.Forms.VisualStyles;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DLabel : WindowContext
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

        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            if (this._metrics.Width > float.Epsilon || this._metrics.Height > float.Epsilon)
            {
                this.Width = this._metrics.Width;
                this.Height = this._metrics.Height;
            }

            this._textRect = this.CreateRenderBounds();
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
                    BrushFont.GetSolidColorBrush(this.FontColor));
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
