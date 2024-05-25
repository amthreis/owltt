using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    [Tool]
    public partial class UIGraphHour : Control
    {
        int _hour;

        [Export] public int Hour
        {
            get { return _hour; }
            set {

                GetNode<Label>("Hour").Text = $"{ value }h";
                
                _hour = value;
            }
        }
    }
}
