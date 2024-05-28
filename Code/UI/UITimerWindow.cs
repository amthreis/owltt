using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task;

public partial class UITimerWindow : Window
{
    [Export] Button _toggle;
    [Export] Panel _togglePn;
    Main m;

    public void Start(Main m)
    {
        this.m = m;

        _toggle.Pressed += OnToggled;
    }

    void OnToggled()
    {
        m.IsActive = !m.IsActive;

        UpdateColor();
    }

    void UpdateColor()
    {
        _togglePn.SelfModulate = Main.TimerColors[m.IsActive ? 2 : 0];
    }
}
