// Source: BrushManager
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

using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Editor.UI.Managers
{
    public sealed class BrushManager : IDisposable
    {
        public WindowRenderTarget RenderTarget { get; private set; }
        public float Radius { get; set; } = 4;
        public float Thickness { get; set; } = 1;


        private Dictionary<string, SolidColorBrush> _solidBrushes;
        private Dictionary<string, LinearGradientBrush> _linearGradientBrushes;


        public BrushManager(WindowRenderTarget renderTarget)
        {
            this.RenderTarget = renderTarget;
            this._solidBrushes = new Dictionary<string, SolidColorBrush>(StringComparer.OrdinalIgnoreCase);
            this._linearGradientBrushes = new Dictionary<string, LinearGradientBrush>(StringComparer.OrdinalIgnoreCase);
        }

        public SolidColorBrush GetSolidColorBrush(RawColor4 color)
        {
            var key = $"{color.R}-{color.G}-{color.B}-{color.A}";
            if (!this._solidBrushes.TryGetValue(key, out var brush))
            {
                brush = new SolidColorBrush(this.RenderTarget, color);
                this._solidBrushes[key] = brush;
            }

            return brush;
        }

        public LinearGradientBrush GetLinearGradientBrush(
            RawColor4 startColor,
            RawColor4 endColor,
            RawVector2 startPoint,
            RawVector2 endPoint)
        {
            var key = $"{startColor.R}-{startColor.G}-{startColor.B}-{startColor.A}|{endColor.R}-{endColor.G}-{endColor.B}-{endColor.A}";

            if (!this._linearGradientBrushes.TryGetValue(key, out var brush))
            {
                var gradientStops = new[]
                {
                    new GradientStop { Position = 0.0f, Color = startColor },
                    new GradientStop() { Position = 1.0f, Color = endColor },
                };

                brush = new LinearGradientBrush(this.RenderTarget, new LinearGradientBrushProperties()
                {
                    EndPoint = endPoint,
                    StartPoint = startPoint,
                }, new GradientStopCollection(this.RenderTarget, gradientStops));

                this._linearGradientBrushes[key] = brush;
            }

            return brush;
        }

        public void Dispose()
        {
            this._solidBrushes.Values.ToList().ForEach(s => s.Dispose());
            this._linearGradientBrushes.Values.ToList().ForEach(s => s.Dispose());

            this._solidBrushes.Clear();
            this._linearGradientBrushes.Clear();

            this._solidBrushes = null;
            this._linearGradientBrushes = null;

            GC.Collect();
        }
    }
}
