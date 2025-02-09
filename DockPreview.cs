// Source: DockPreview
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

namespace Engine.Editor.UI
{
    public class DockPreview
    {
        private WindowRenderTarget _target;
        private RawColor4 _color = new RawColor4(0, 1, 0, 0.5f);
        private bool _visible = false;
        private RawRectangleF _previewArea;

        public DockPreview(WindowRenderTarget target)
        {
            _target = target;
        }

        public void Show(float x, float y, float width, float height)
        {
            _previewArea = new RawRectangleF(x, y, width, height);
            _visible = true;
        }

        public void Hide()
        {
            _visible = false;
        }

        public void Draw()
        {
            if (_visible)
            {
                var brush = new SolidColorBrush(_target, _color);
                _target.FillRectangle(_previewArea, brush);
            }
        }
    }

}
