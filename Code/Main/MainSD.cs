using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Task;

[Serializable]
public class MainSD
{
    public DateTime CreationDate { get; set; }
    public TimeSpan ElapsedTotal { get; set; }

    public Dictionary<DateTime, TimeSpan> ElapsedPerDay { get; set; }

    public MainSettings Settings { get; set; }

    public MainSD(Main m)
    {
        GD.Print("ss ");
        CreationDate  = m.CreationDate;
        ElapsedTotal  = m.ElapsedTotal;
        ElapsedPerDay = m.ElapsedPerDay;
        //Settings = m.Settings;

        GD.Print("store ", CreationDate);
        GD.Print("store ", ElapsedTotal);
        GD.Print("store ", ElapsedPerDay);
        GD.Print("store ", Settings);
    }
    public MainSD()
    {

    }
}
