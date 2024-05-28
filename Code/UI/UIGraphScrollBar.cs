using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    public partial class UIGraphScrollBar : HScrollBar
    {
        public override void _Ready()
        {
            Scrolling += OnScrolling;
            ValueChanged += OnValueChanged;
            Changed += OnChanged;

        }


        void OnScrolling()
        {
            GD.Print("OnScrolling");
        }

        void OnValueChanged(double value)
        {
            GD.Print("---OnValueChanged");
        }

        void OnChanged()
        {
            GD.Print("---OnChanged");
        }
    }
}
