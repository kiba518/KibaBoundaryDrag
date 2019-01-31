using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KibaBoundaryDrag
{
    public class KibaBoundaryDragEventArgs : EventArgs
    {
        public double HorizontalChange { get; set; }
        public double VerticalChange { get; set; }
        public bool? LeftDirection { get; set; }
        public bool? TopDirection { get; set; }

        public override string ToString()
        {
            return String.Format("{0}({1}) {2}({3})", LeftDirection, HorizontalChange, TopDirection, VerticalChange);
        }

        public KibaBoundaryDragEventArgs(double hori, double verti, bool? lefdir, bool? topdir)
        {
            HorizontalChange = hori;
            VerticalChange = verti;
            LeftDirection = lefdir;
            TopDirection = topdir;
        }
    }
    
}
