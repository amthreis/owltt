using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    //[Tool]
    public partial class UIGraphLines : VBoxContainer
    {
        int _lineWidth;
        Color _lineColor;

        [Export] public int LineWidth 
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                UpdateLines();
            }
        }
        [Export] public Color LineColor 
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                UpdateLines();
            }
        }

        void UpdateLines()
        {
            foreach(var ch in GetChildren())
            {
                if (ch.Name == "Margin")
                    continue;

                var ln = ch.GetNode<ColorRect>("Line");
                
                ln.Size = new Vector2(ln.Size.X, _lineWidth);
                ln.Color = _lineColor;
            }
        }
    }
}
