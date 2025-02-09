// Source: DockRegionInfo
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

using Engine.Editor.UI.Controls;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Engine.Editor.UI
{
    public class DockRegionInfo
    {
        public DockRegion Region { get; }
        public Stopwatch HoverTimer { get; private set; }
        public bool IsHovering => this._hoverTime >= 150; // 500ms delay


        private volatile bool _runHoverTimer;
        private volatile int _hoverTime;

        private static readonly object obj = new object();


        public DockRegionInfo(DockRegion region)
        {
            this.Region = region;
            this.HoverTimer = new Stopwatch();

            _ = Task.Run(async () =>
            {
                while (!Environment.HasShutdownStarted)
                {
                    this._hoverTime = this.HoverTimer.Elapsed.Milliseconds;

                    if (this._hoverTime > 100)
                    {
                        ;
                    }

                    if (this._runHoverTimer)
                    {
                        if (!this.HoverTimer.IsRunning)
                        {
                            this.HoverTimer.Restart();
                        }
                    }
                    else
                    {
                        if (this.HoverTimer.IsRunning)
                        {
                            this.HoverTimer.Reset();
                        }
                    }

                    await Task.Delay(100);
                }

            });

        }

        public void StartHover()
        {
            lock (obj)
            {
                if (!this._runHoverTimer)
                {
                    this._runHoverTimer = true;
                }
            }
        }

        public void StopHover()
        {
            lock (obj)
            {
                if (this._runHoverTimer)
                {
                    this._runHoverTimer = false;
                }
            }
        }

        public void ShowDockPreview(
            Direct2DDockPanel panel,
            DockPreview preview)
        {
            if (preview == null) { return; }

            var regionRect = this.GetRegionRect(panel);
            preview.Show(
                regionRect.Left,
                regionRect.Top,
                regionRect.Right,
                regionRect.Bottom);
        }

        public RawRectangleF GetRegionRect(Direct2DDockPanel panel)
        {
            int x = 0, y = 0, width = 0, height = 0;

            switch (this.Region)
            {
                case DockRegion.Fill:
                    {
                        x = panel.X;
                        y = panel.Y;
                        width = panel.Width;
                        height = panel.Height;
                    }
                    break;
                case DockRegion.Left:
                    {
                        x = panel.X;
                        y = panel.Y;
                        width = panel.Width / 4;
                        height = panel.Height;
                    }
                    break;

                case DockRegion.Right:
                    {
                        x = (int)panel.Right - (panel.Width / 4);
                        y = panel.Y;
                        width = (int)(panel.Width / 4);
                        height = panel.Height;
                    }
                    break;

                case DockRegion.Top:
                    {
                        x = panel.X;
                        y = panel.Y;
                        width = panel.Width;
                        height = panel.Height / 4;
                    }
                    break;

                case DockRegion.Bottom:
                    {
                        x = panel.X;
                        y = (int)panel.Bottom - (panel.Height / 4);
                        width = panel.Width;
                        height = panel.Height / 4;
                    }
                    break;

                case DockRegion.TopLeft:
                    {
                        x = panel.X;
                        y = panel.Y;
                        width = panel.Width / 4;
                        height = panel.Height / 4;
                    }
                    break;
                case DockRegion.TopRight:
                    {
                        x = (int)panel.Right - (panel.Width / 4);
                        y = panel.Y;
                        width = panel.Width / 4;
                        height = panel.Height / 4;
                    }
                    break;
                case DockRegion.BottomLeft:
                    {
                        x = panel.X;
                        y = panel.Y + (panel.Height / 4);
                        height = panel.Height / 4;
                        width = panel.Width / 4;
                    }
                    break;
                case DockRegion.BottomRight:
                    {
                        x = (int)panel.Right - (width / 4);
                        y = (int)panel.Bottom - (height / 4);
                        width = panel.Width / 4;
                        height = panel.Height / 4;
                    }
                    break;
            }

            return new RawRectangleF(x, y, x + width, y + height);
        }
    }

}
