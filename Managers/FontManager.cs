// Source: FontManager
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

using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;

namespace Engine.Editor.UI.Managers
{
    public class FontManager : IDisposable
    {
        public FontWeight FontWeight { get; set; } = FontWeight.Normal;
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;
        public float FontSize { get; set; } = 14.0f;


        private Dictionary<string, TextFormat> _fonts;
        private Factory _factory;


        public FontManager()
        {
            this._factory = new Factory();
            this._fonts = new Dictionary<string, TextFormat>(StringComparer.OrdinalIgnoreCase);
        }


        public TextFormat GetFont(
            string fontName,
            float fontSize,
            FontWeight weight = FontWeight.Regular,
            FontStyle style = FontStyle.Normal)
        {
            var key = $"{fontName}-{fontSize}-{weight}-{style}";

            if (!this._fonts.TryGetValue(key, out var font))
            {
                font = new TextFormat(this._factory, fontName, weight, style, fontSize);
                this._fonts[key] = font;
            }

            return font;
        }

        public void Dispose()
        {
            foreach (var font in this._fonts.Values)
            {
                font?.Dispose();
            }

            this._fonts.Clear();
            this._fonts = null;

            this._factory?.Dispose();
            this._factory = null;
        }
    }
}
