// Source: Direct2DTextbox
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
using SharpDX.Mathematics.Interop;
using System;

namespace Engine.Editor.UI.Controls
{
    public class TextBoxControl : WindowContext
    {
        public TextBoxControl()
        {
            this.BackgroundColor = new RawColor4(0.1f, 0.1f, 0.1f, 1.0f);
            this.BorderColor = new RawColor4(1, 1, 1, 1);
            this.Width = 200;
            this.Height = 30;
        }

        protected override void OnPaint(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text)
                && !string.IsNullOrEmpty(this.FontName)
                && this.Font.FontSize > 0)
            {
                var font = this.Font.GetFont(this.FontName, this.Font.FontSize, this.Font.FontWeight, this.Font.FontStyle);

                this.RenderTarget.DrawText(
                    this.Text,
                    font,
                    this.Bounds,
                    BrushFont.GetSolidColorBrush(this.FontColor));
            }

            base.OnPaint(sender, e);
        }

        protected override void OnKeyDown(object sender, SdlKeyEventArgs e)
        {
            if (e.Keycode == SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE && this.Text.Length > 0)
            {
                this.Text = this.Text.Substring(0, this.Text.Length - 1);
            }
            else if (e.Keycode >= SDL2.SDL.SDL_Keycode.SDLK_a && e.Keycode <= SDL2.SDL.SDL_Keycode.SDLK_z)
            {
                this.Text += (char)e.Keycode;
            }

            base.OnKeyDown(sender, e);
        }
    }
}
