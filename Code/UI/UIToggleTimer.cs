using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task;

public partial class UIToggleTimer : Button
{
    [Export] public Window TimerWindow;

    public override void _Pressed()
    {
        TimerWindow.Visible = !TimerWindow.Visible;

        if (!TimerWindow.Visible)
        {
            //stop timer
        }
    }
}
