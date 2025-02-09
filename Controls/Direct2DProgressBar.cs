// Source: Direct2DProgressBar
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
using Engine.Editor.UI.Managers;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;

namespace Engine.Editor.UI.Controls
{
    public enum ProgressBarStyle
    {
        Horizontal,
        Vertical,
        Arc,
    }


    public class Direct2DProgressBar : WindowContext
    {

        public float Min
        {
            get => this._min;
            set
            {
                if (this._min != value)
                {
                    this._min = value;
                    this.OnMinChanged(this, EventArgs.Empty);
                }
            }
        }

        public float Max
        {
            get => this._max;
            set
            {
                if (this._max != value)
                {
                    this._max = value;
                    this.OnMaxChanged(this, EventArgs.Empty);
                }
            }
        }

        public float Value
        {
            get => this._value;
            set
            {
                var v = Mathf.Clamp(value, this._min, this._max);
                if (this._value != v)
                {
                    this._value = v;
                    this.OnValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public ProgressBarStyle BarStyle
        {
            get => this._barStyle;
            set
            {
                if (this._barStyle != value)
                {
                    this._barStyle = value;
                    this.OnBarStyleChanged(this, EventArgs.Empty);
                }
            }
        }

        public BrushManager BrushFill { get; private set; }

        public RawColor4 FillColor { get; set; } = SystemColors.HotTrack.ToRawColor4();

        public bool IsSlider { get; set; }
        public float FillThickness { get; set; }


        public event EventHandler MinChanged;
        public event EventHandler MaxChanged;
        public event EventHandler ValueChanged;
        public event EventHandler BarStyleChanged;

        private float _min;
        private float _max;
        private float _value;
        private ProgressBarStyle _barStyle;
        private bool _isDragging;
        private int _lastX;
        private int _lastY;

        protected virtual void OnBarStyleChanged(object sender, EventArgs e) { this.BarStyleChanged?.Invoke(sender, e); }
        protected virtual void OnMinChanged(object sender, EventArgs e) { this.MinChanged?.Invoke(sender, e); }
        protected virtual void OnMaxChanged(object sender, EventArgs e) { this.MaxChanged?.Invoke(sender, e); }
        protected virtual void OnValueChanged(object sender, EventArgs e) { this.ValueChanged?.Invoke(sender, e); }
        protected override void OnInitialized(object sender, EventArgs e)
        {
            this.BrushFill = new BrushManager(this.RenderTarget)
            {
                Thickness = this.FillThickness,
            };

            base.OnInitialized(sender, e);
        }


        protected override void OnMouseButtonDown(object sender, SdlMouseEventArgs e)
        {
            if (!this.IsSlider) { return; }

            var pt = this.PointToClient(e.X, e.Y);
            if (this.ClientRectangle.Contains(pt.X, pt.Y))
            {
                this._isDragging = true;
                this._lastY = e.Y;
                this._lastX = e.X;
            }

            base.OnMouseButtonDown(sender, e);
        }

        protected override void OnMouseButtonUp(object sender, SdlMouseEventArgs e)
        {
            this._isDragging = false;
            base.OnMouseButtonUp(sender, e);
        }

        protected override void OnMouseMove(object sender, SdlMouseEventArgs e)
        {
            var pt = this.PointToClient(e.X, e.Y);
            if (!this.ClientRectangle.Contains(pt.X, pt.Y) && this._isDragging)
            {
                this._isDragging = false;
                return;
            }

            if (!this.IsSlider || !this._isDragging) { return; }

            // Corrected offset calculation
            var offsetY = e.Y - this._lastY;
            var offsetX = e.X - this._lastX;

            if (Mathf.Abs(offsetY) > float.Epsilon || Mathf.Abs(offsetX) > float.Epsilon)
            {
                this.Value += this.BarStyle == ProgressBarStyle.Horizontal ? offsetX : offsetY;

                // Update last position
                this._lastY = e.Y;
                this._lastX = e.X;
            }

            base.OnMouseMove(sender, e);
        }

        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            if (this.BrushFill != null) { this.BrushFill.Thickness = this.FillThickness; }

            switch (this.BarStyle)
            {
                case ProgressBarStyle.Horizontal:
                    {
                        this.PainHorizontal();
                    }
                    break;
                case ProgressBarStyle.Vertical:
                    {
                        this.PaintVertical();
                    }
                    break;
                case ProgressBarStyle.Arc:
                    {
                        this.PaintArc();
                    }
                    break;
            }
        }

        private void PaintArc()
        {
            var center = new RawVector2(this.Left + (this.Width / 2), this.Top + (this.Height / 2));
            var radius = (Mathf.Min(this.Width, this.Height) / 2) - this.BrushFill.Thickness;
            var strokeWidth = this.BrushFill.Thickness;

            // Normalize progress to degrees (0 - 360°)
            var progressAngle = this.Value / this.Max * 360.0f;

            if (progressAngle <= 0.0f)
            {
                return; // Avoid unnecessary rendering
            }

            using (var pathGeometry = new PathGeometry(this.RenderTarget.Factory))
            {
                var startRad = Mathf.ToRadians(-270.0f); // Start from top (6 o'clock)
                var endRad = Mathf.ToRadians(-270.0f + progressAngle);

                // Compute start and end points of the arc
                var startPoint = new RawVector2(
                    center.X + (float)(radius * Mathf.Cos(startRad)),
                    center.Y + (float)(radius * Mathf.Sin(startRad))
                );

                var endPoint = new RawVector2(
                    center.X + (float)(radius * Mathf.Cos(endRad)),
                    center.Y + (float)(radius * Mathf.Sin(endRad))
                );

                // Handle full circle case (100% progress)
                if (progressAngle >= 359.9f)
                {
                    this.PaintCircular();
                }
                else
                {
                    using (var sink = pathGeometry.Open())
                    {
                        // Begin drawing the arc
                        sink.BeginFigure(startPoint, FigureBegin.Hollow);
                        sink.AddArc(new ArcSegment
                        {
                            Point = endPoint,
                            Size = new Size2F(radius, radius),
                            RotationAngle = 0,
                            SweepDirection = SweepDirection.Clockwise,
                            ArcSize = progressAngle > 180.0f ? ArcSize.Large : ArcSize.Small
                        });
                        sink.EndFigure(FigureEnd.Open);
                        sink.Close();

                        // Draw the progress arc
                        this.RenderTarget.DrawGeometry(pathGeometry, this.BrushFill.GetSolidColorBrush(this.FillColor), strokeWidth);

                    }
                }

            }
        }


        private void PaintCircular()
        {
            var strokeWidth = this.BrushFill.Thickness;
            var center = new RawVector2(this.Left + (this.Width / 2), this.Top + (this.Height / 2));
            var radius = (Mathf.Min(this.Width, this.Height) / 2) - strokeWidth;

            // Draw the full circular border (if needed)
            this.RenderTarget.DrawEllipse(
                new Ellipse(center, radius, radius),
                this.BrushFill.GetSolidColorBrush(this.FillColor),
                strokeWidth);
        }


        private void PaintVertical()
        {
            var progress = this.Value / this.Max;

            var fillHeight = this.Height * progress;
            var fillRect = new RawRectangleF(
                this.Left,                     // X position (remains unchanged)
                this.Bottom - fillHeight,       // Start from the bottom and move up
                this.Right,                     // Full width of the control
                this.Bottom);

            this.FillStyle(
                fillRect,
                this.BrushFill,
                this.FillColor,
                this.BorderStyle);

            this.DrawBorder(
                this.Bounds,
                this.BrushBorder,
                this.BorderColor,
                this.BorderStyle);
        }

        private void PainHorizontal()
        {
            var progress = this.Value / this.Max;

            var minWidth = this.Width * progress;
            var fillRect = new RawRectangleF(
                this.Left,
                this.Top,
                this.Left + minWidth,
                this.Bottom);

            this.FillStyle(
                fillRect,
                this.BrushFill,
                this.FillColor,
                this.BorderStyle);

            this.DrawBorder(
                this.Bounds,
                this.BrushBorder,
                this.BorderColor,
                this.BorderStyle);
        }

    }
}
