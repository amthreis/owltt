﻿using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Task;


public partial class Main : Node
{
    public static readonly Color[] TimerColors = new Color[] { new Color("595a58"), new Color("9a8247"), new Color("376845") };

    GlobalInputCSharp globalInput;

    Timer secondTimer, minuteTimer;

    public bool IsActive = false;

    public override void _Ready()
    {
        Settings = new MainSettings(this);
        Data = new MainData();

        globalInput = GetNode<GlobalInputCSharp>("/root/GlobalInput/GlobalInputCSharp");

        secondTimer = new Timer();
        AddChild(secondTimer);
        secondTimer.OneShot = false;
        secondTimer.Start(1f);
        secondTimer.Timeout += OnSecond;

        minuteTimer = new Timer();
        AddChild(minuteTimer);
        minuteTimer.OneShot = false;
        minuteTimer.Start(60f);
        minuteTimer.Timeout += OnMinute;



        //SpawnGraphItem(DateTime.Now);

        UpdateHoursAndLines();
    }

    public MainSettings Settings { get; private set; }
    public MainData Data { get; private set; }

    [Export] PackedScene uiGraphItemPS;
    [Export] HBoxContainer uiGraphItemsCtr;
    [Export] UITime uiTime;

    Vector2 lastMousePos;

    public event Action<TimeSpan> SecondPassed;

    void OnSecond()
    {
        GD.Print("sec");
        var second = TimeSpan.FromSeconds(1f);

        elapsed += second;
        ElapsedTotal += second;

        //var time = TimeSpan.FromSeconds(elapsed);
        var str = elapsed.ToString(@"hh\:mm\:ss");

        if (!ElapsedPerDay.ContainsKey(DateTime.Today))
        {
            ElapsedPerDay.Add(DateTime.Today, TimeSpan.Zero);
        }

        if (!_dayGraphItem.ContainsKey(DateTime.Today))
        {
            SpawnGraphItem(DateTime.Today);
        }

        ElapsedPerDay[DateTime.Today] = ElapsedPerDay[DateTime.Today].Add(second);

        _dayGraphItem[DateTime.Today].SetDTS(this, DateTime.Today, ElapsedPerDay[DateTime.Today]);

        //time.Add(TimeSpan.FromSeconds(dT));

        //GD.Print(str);

        //toggleRunningButton.GetNode<Label>("../Margin/Date").Text = str;

        uiTime.TotalHours = ElapsedTotal;
        uiTime.TotalHoursThisWeek = GetElapsedThisWeek();
        uiTime.TotalHoursToday = GetElapsedToday();

        SecondPassed?.Invoke(elapsed);
        //GetNode<Label>("HUD/Actions/Total/Value").Text = FormattedTotalHours(ElapsedTotal);//.ToString(@"h\hmm");
        //GetNode<Label>("HUD/Actions/Today/Value").Text = GetElapsedToday().ToString(@"h\hmm");
    }

    void OnMinute()
    {
        UIScore.SetValue(GetDevScore());
    }

    public override void _PhysicsProcess(double dT)
    {
        var mousePos = globalInput.GetMousePosition();

        if (mousePos != lastMousePos)
        {
            Data.InactivityTimer = 0f;
        }

        lastMousePos = mousePos;

        Data.InactivityTimer += (float)dT;

        //GD.Print(Mathf.Floor(Data.InactivityTimer));
        //GD.Print(anything);
    }

    [Export] VBoxContainer linesCtr;
    [Export] VBoxContainer hoursCtr;
    [Export] public UIScore UIScore;

    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_up"))
        {
            GetNode("FileDialog").Set("file_mode", 0);
            GetNode("FileDialog").Call("set_filters", new string[] { "*.tsk ; Taskeru Files" });
            GetNode("FileDialog").Call("show");
        }
    }

    public void UpdateHoursAndLines()
    {
        if (hoursCtr == null || linesCtr == null)
            return;

        //GD.Print(chd.Name);

        for (var i = 0; i < 12; i++)
        {
            var hChd = hoursCtr.GetChild<Control>(hoursCtr.GetChildCount() - 2 - i);
            var lChd = linesCtr.GetChild<Control>(linesCtr.GetChildCount() - 1 - i);

            hChd.Visible = i < Settings.TargetHours;
            lChd.Visible = i <= Settings.TargetHours;
        }

        UIScore.UpdateControls();
    }


    void OnLoad()
    {
        //Load("user://Tasks");

        saveMode = false;
        GetNode<Control>("Overlay").Show();

        GetNode("FileDialog").Set("file_mode", 0);
        GetNode("FileDialog").Call("set_filters", new string[] { "*.tsk ; Taskeru Files" });
        GetNode("FileDialog").Call("show");
    }

    bool saveMode;
    string saveToPath;

    TimeSpan elapsed;
    public TimeSpan ElapsedTotal { get; private set; }
    public Dictionary<DateTime, TimeSpan> ElapsedPerDay { get; private set; } = new Dictionary<DateTime, TimeSpan>();

    TimeSpan RefTime => TimeSpan.FromHours(Settings.TargetHours);

    public float GetDevScore()
    {
        var score = 0f;
        var weight = 0f;

        var i = 0;
        var days = 0;

        var TODAY = DateTime.Today;
        var currDay = TODAY;
        //return 0f;
        //GD.Print("td = ", Settings.TargetDays);
        while (days < Settings.TargetDays)
        {
            //GD.Print("--");
            var isWeekend = currDay.DayOfWeek == DayOfWeek.Sunday || currDay.DayOfWeek == DayOfWeek.Saturday;

            if (!isWeekend || !Settings.WorkingDaysOnly)
            {
                //var dayW = i;

                var dayScore = GetDayScore(currDay);
                var dayWeight = 1f - Mathf.Pow(days / (float)Settings.TargetDays, 1f);

                //if (dayScore > 0f)
                {
                    GD.Print($"Score of {currDay.ToString("dd/MM/yyyy")} ({currDay.DayOfWeek}): {dayScore}, w: {dayWeight}");
                }

                score += dayScore * dayWeight;
                weight += dayWeight;
                days++;
            }

            //GD.Print("--2");
            //GD.Print()
            currDay = currDay.Add(-TimeSpan.FromDays(1));
            //GD.Print("--3");

            //i++;
        }
        

        GD.Print($"-------- total score:  { score / weight }");

        return (float)(score /weight);
    }
    public float GetDayScore(DateTime day)
    {
        var time = GetElapsedAtDay(day);

        return Mathf.Clamp(Mathf.InverseLerp(0, Settings.TargetHours, (float)time.TotalHours), 0, 1);
    }


    Dictionary<DateTime, UIGraphItem> _dayGraphItem = new Dictionary<DateTime, UIGraphItem>();

    UIGraphItem SpawnGraphItem(DateTime dT)
    {
        var mth = uiGraphItemPS.Instantiate<UIGraphItem>();
        mth.SetDTS(this, dT, GetElapsedAtDay(dT));

        uiGraphItemsCtr.AddChild(mth);
        _dayGraphItem[dT] = mth;

        return mth;
    }

    TimeSpan GetElapsedToday()
    {
        if (!ElapsedPerDay.ContainsKey(DateTime.Today))
            return TimeSpan.Zero;

        return ElapsedPerDay[DateTime.Today];
    }

    TimeSpan GetElapsedThisWeek()
    {
        TimeSpan weekSpan = TimeSpan.Zero;
        DateTime t = DateTime.Today;

        while(true)
        {
            if (ElapsedPerDay.ContainsKey(t))
            {
                weekSpan += ElapsedPerDay[t];
            }


            if (t.DayOfWeek == DayOfWeek.Sunday)
            {
                break;
            }

            t = t.AddDays(-1);
        }

        return weekSpan;
    }

    TimeSpan GetElapsedAtDay(DateTime day)
    {
        if (!ElapsedPerDay.ContainsKey(day))
            return TimeSpan.Zero;

        return ElapsedPerDay[day];
    }

    void OnSave(bool saveAs = false)
    {
        saveMode = true;
        //Serialize(new TTaskerSD(this), "user://Tasks");
        GD.Print("Saved");

        if (!saveAs && saveToPath != null)
        {
            //saveToPath = "";
            //Serialize(new TTaskerSD(this), saveToPath);

            //NoAsterisk();
        }
        else
        {
            GetNode<Control>("Overlay").Show();

            GetNode("FileDialog").Set("file_mode", 3);
            GetNode("FileDialog").Call("set_filters", new string[] { "*.tsk ; Taskeru Files" });
            GetNode("FileDialog").Call("show");
            GD.Print("Done");
        }
    }

    public void SetScore(float score)
    {
        UIScore.SetValue(score);
    }

    public static void Serialize(object o, string path, Action callback = null)
    {
        var lastSlash = path.LastIndexOf('/');
        var substr = path.Substring(0, lastSlash);
        var _f = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        var data = JsonConvert.SerializeObject(o, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Converters = { new StringEnumConverter() },
                        TypeNameHandling = TypeNameHandling.Auto,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });

        _f.StoreString(data);

        _f.Close();
        callback?.Invoke();
    }

    public static T Deserialize<T>(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            throw new Exception($"The file to deserialize couldn't be found. Path: {path}");
        }

        var _f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var data = FileAccess.GetFileAsString(path);

        _f.Close();

        return JsonConvert.DeserializeObject<T>(data,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Converters = { new StringEnumConverter() },
                        TypeNameHandling = TypeNameHandling.Auto,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                    });
    }
}
