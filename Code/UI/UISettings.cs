using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Task;

public partial class UISettings : Panel
{
    [Export] ColorRect overlay;
    Main m;

    [Export] HSlider TargetHoursHS;
    [Export] SpinBox TargetDaysSB;
    [Export] CheckBox WorkingDaysOnlyCB;
    [Export] Button closeBtn;

    public override void _Ready()
    {
        closeBtn.Pressed += OnClose;

        TargetHoursHS.ValueChanged += OnTHChanged;
        TargetDaysSB.ValueChanged += OnTDChanged;
        //TargetDaysSB.Changed += OnTDChanged2;
    }

    void OnTDChanged(double value)
    {
        GD.Print("OnTDChanged");
    }

    void OnTHChanged(double value)
    {
        //GD.Print("OnTHChanged");
        TargetHoursHS.GetNode<Label>("../../Label").Text = $"{(int)value}h";
    }

    public void Start(Main m)
    {
        this.m = m;

        overlay.Show();
        Show();


        TargetHoursHS.Value = m.Settings.TargetHours;
        TargetHoursHS.GetNode<Label>("../../Label").Text = $"{m.Settings.TargetHours}h";

        TargetDaysSB.Value = m.Settings.TargetDays;
        WorkingDaysOnlyCB.ButtonPressed = m.Settings.WorkingDaysOnly;
    }

    void PreClose()
    {
        GD.Print("PreClose");
        CallDeferred(nameof(Close));
    }

    void Close()
    {
        GD.Print("close");
        overlay.Hide();
        Hide();


        m.Settings.TargetHours = (int)TargetHoursHS.Value;
        m.Settings.TargetDays = (int)TargetDaysSB.Value;
        m.Settings.WorkingDaysOnly = WorkingDaysOnlyCB.ButtonPressed;

        m.UpdateAll();
    }

    async void OnClose()
    {
        Hide();

        await System.Threading.Tasks.Task.Delay(20);


        m.Settings.TargetHours = (int)TargetHoursHS.Value;
        m.Settings.TargetDays = (int)TargetDaysSB.Value;
        m.Settings.WorkingDaysOnly = WorkingDaysOnlyCB.ButtonPressed;

        m.UpdateAll();
        overlay.Hide();
    }
}
