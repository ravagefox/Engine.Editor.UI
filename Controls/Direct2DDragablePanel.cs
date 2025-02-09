// Source: Direct2DDragablePanel
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
using System;
using System.Drawing;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DPanel : WindowContext
    {
        public bool IsDraggable { get; set; } = true;

        private bool _isDragging;
        private int _lastX;
        private int _lastY;

        protected override void OnMouseButtonDown(object sender, SdlMouseEventArgs e)
        {
            if (!this.IsDraggable)
            {
                return;
            }

            if (this.VerticalScroll.Enabled && this.VerticalScroll.Visible)
            {
                if (this.VerticalScroll.IsMouseOver) { return; }
            }

            var pt = this.PointToClient(e.X, e.Y);

            // Start dragging only if mouse is inside the panel
            if (this.ClientRectangle.Contains(pt.X, pt.Y))
            {
                this._isDragging = true;
                this._lastX = e.X;
                this._lastY = e.Y;
            }

            base.OnMouseButtonDown(sender, e);
        }

        protected override void OnMouseMove(object sender, SdlMouseEventArgs e)
        {
            var pt = this.PointToClient(e.X, e.Y);
            if (!this.ClientRectangle.Contains(pt.X, pt.Y) && this._isDragging)
            {
                this._isDragging = false;
                return;
            }

            if (!this.IsDraggable || !this._isDragging)
            {
                return;
            }

            // Corrected offset calculation
            var offsetX = e.X - this._lastX;
            var offsetY = e.Y - this._lastY;

            if (Mathf.Abs(offsetX) > float.Epsilon || Mathf.Abs(offsetY) > float.Epsilon)
            {
                this.Location = new PointF(this.Left + offsetX, this.Top + offsetY);

                // Move all child controls along with the panel
                foreach (var ctrl in this.Controls)
                {
                    ctrl.Location = new PointF(ctrl.Left + offsetX, ctrl.Top + offsetY);
                }

                // Update last position
                this._lastX = e.X;
                this._lastY = e.Y;
            }

            base.OnMouseMove(sender, e);
        }

        protected override void OnMouseButtonUp(object sender, SdlMouseEventArgs e)
        {
            this._isDragging = false;
            base.OnMouseButtonUp(sender, e);
        }
        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            this.VerticalScroll.Enabled = false;
            this.VerticalScroll.Visible = false;

            base.OnPaintBackground(sender, e);
        }
    }
}
