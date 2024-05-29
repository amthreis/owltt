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
    [Export] Label _timerLb;
    public Main m;

    public void Start(Main m, bool connect)
    {
        this.m = m;

        if (connect)
        {
            _toggle.Pressed += OnToggled;
        }


        UpdateTime(m.elapsed);
        UpdateColor();
    }


    void OnToggled()
    {
        m.Activate(!m.IsActive);

        UpdateColor();
    }

    public void UpdateTime(TimeSpan ts)
    {
        _timerLb.Text = ts.ToString(@"hh\:mm\:ss");
    }

    void UpdateColor()
    {
        _togglePn.SelfModulate = Main.TimerColors[m.IsActive ? 2 : 0];
    }
}
