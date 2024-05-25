using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    [Tool]
    public partial class UIGraphItem : VBoxContainer
    {
        float _value;

        [Export(PropertyHint.Range, "0, 1")]
        public float Value 
        { 
            get { return _value; }
            set {  
                _value = value;

                GetNode<Control>("Remainder").SizeFlagsStretchRatio = 1f - _value;
                GetNode<Panel>("Panel").SizeFlagsStretchRatio = _value;
            }
        }
    }
}
