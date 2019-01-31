using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KibaBoundaryDrag
{ 
    public class KibaBoundaryDrag
    {
        FrameworkElement control;
        volatile bool pressed;//是否按下
        Point prePoint;//上一个鼠标位置
        public Thickness thickness = new Thickness(4);
        public double radius = 15;
        public Cursor DefaultCursor { get; private set; }

        public bool? leftBound;//鼠标是否在左右边框里
        public bool? topBound;//鼠标是否在上下边框里
        public bool isCustomCursor = false;//是否已经设置了自定义鼠标样式
        public double minWidth = 80;//最小拉到80宽
        public double minHeight = 80;//最小拉到80高
        public double maxHeight = 80;//最大拉到距窗体80
        public double maxWidth = 80;//最大拉到距窗体80

        public KibaBoundaryDrag(FrameworkElement _control, FrameworkElement _parentControl)
        {  
            if (_control == null)
            {
                throw new ArgumentNullException("_control");
            }
            this.Resize += (s, e) =>
            {
                var rect = s as FrameworkElement;
                //左右拉伸
                if (e.LeftDirection.HasValue)
                { 
                    var value = rect.Width + e.HorizontalChange;
                    if (value > rect.MinWidth)
                    {
                        rect.Width = value;
                        if (rect.Width < minWidth)
                        {
                            rect.Width = minWidth;
                        }
                        double maxWidth = _parentControl.ActualWidth - this.maxWidth;
                        if (rect.Width > maxWidth)
                        {
                            rect.Width = maxWidth;
                        }
                    }
                }
                //上下拉伸
                if (e.TopDirection.HasValue)
                { 
                    var value = rect.Height + e.VerticalChange;
                    if (value > rect.MinHeight)
                    {
                        rect.Height = value;
                        if (rect.Height < minHeight)
                        {
                            rect.Height = minHeight;
                        }
                        double maxHeight = _parentControl.ActualHeight - this.maxHeight;
                        if (rect.Height > maxHeight)
                        {
                            rect.Height = maxHeight;
                        }
                    }
                }
            }; 
            DefaultCursor = null;
            this.control = _control;
            this.control.MouseEnter += _control_MouseEnter;
            this.control.MouseMove += _control_MouseMove;
            this.control.MouseDown += _control_MouseDown;
            this.control.MouseUp += _control_MouseUp;
        }

        public event EventHandler<KibaBoundaryDragEventArgs> Resize;

        protected virtual void OnResize(KibaBoundaryDragEventArgs e)
        {
            if (this.Resize != null)
                this.Resize(control, e);
        }
         
        void _control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isCustomCursor)
            {
                control.ReleaseMouseCapture();
                pressed = false;
            }

        }

        void _control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCustomCursor)
            {
                var point = e.GetPosition(control);
                point = control.PointToScreen(point);
                control.CaptureMouse();
                prePoint = point;
                pressed = true; 
            }

        }

        void _control_MouseMove(object sender, MouseEventArgs e)
        {

            var org_point = e.GetPosition(control);
            var point = control.PointToScreen(org_point); 
            if (!pressed)
            {
                _SetCursor(org_point);
            }
            else
            {
                if ((point.X == prePoint.X || point.X == prePoint.X * -1)
                    && (point.Y == prePoint.Y || point.Y == prePoint.Y * -1))
                {
                    return;
                }

                if (isCustomCursor & (topBound.HasValue || leftBound.HasValue))
                {
                    double vertiChange, horiChange;
                    vertiChange = horiChange = 0;
                    var pointScr = point;
                    if (leftBound.HasValue)
                    {
                        horiChange = pointScr.X - prePoint.X;
                        if (leftBound.Value)
                            horiChange *= -1;
                    }
                    if (topBound.HasValue)
                    {
                        vertiChange = pointScr.Y - prePoint.Y;
                        if (topBound.Value)
                        {
                            vertiChange *= -1;
                        }
                        else
                        {

                        }
                    } 
                    OnResize(new KibaBoundaryDragEventArgs(horiChange, vertiChange, leftBound, topBound));
                    prePoint = pointScr;
                    
                }
            }
        }

        void _control_MouseEnter(object sender, MouseEventArgs e)
        {
            var org_point = e.GetPosition(control);
            var point = control.PointToScreen(org_point);
            if (!pressed)
            {
                _SetCursor(point);
            }
        }

        void _SetCursor(Point point)
        {
            var left = point.X;
            var top = point.Y;
            var right = control.ActualWidth - left;
            var bottom = control.ActualHeight - top;

            leftBound = topBound = null;
            if (left < thickness.Left)
            {
                leftBound = true;
            }
            else if (right < thickness.Right)
            {
                leftBound = false;
            }

            if (top <= thickness.Top)
            {
                topBound = true;

            }
            else if (bottom <= thickness.Bottom)
            {
                topBound = false;
            }
            Cursor cur = null;
            if (leftBound.HasValue && !double.IsNaN(control.Width))
            {
                //如果上下没有进入区域，尝试按照Radius属性再次计算
                if (!topBound.HasValue)
                {
                    if (top < radius)
                        topBound = true;
                    else if (bottom < radius)
                        topBound = false;
                }

                if (topBound.HasValue)
                {
                    if (leftBound.Value == topBound.Value)
                        cur = Cursors.SizeNWSE;
                    else
                        cur = Cursors.SizeNESW;
                }
                else
                    cur = Cursors.SizeWE;
            }
            else if (topBound.HasValue && !double.IsNaN(control.Height))
            {
                //这里leftDirection.HasValue必定是false，所以直接计算
                if (left < radius)
                    leftBound = true;
                else if (right < radius)
                    leftBound = false;

                if (leftBound.HasValue)
                {
                    if (leftBound.Value == topBound.Value)
                        cur = Cursors.SizeNWSE;
                    else
                        cur = Cursors.SizeNESW;
                }
                else
                    cur = Cursors.SizeNS;
            }
            if (cur != null)
            {
                control.Cursor = cur;
                isCustomCursor = true;
            }
            else
            {
                control.Cursor = DefaultCursor;
                isCustomCursor = false;

            }
        }
    }
}
