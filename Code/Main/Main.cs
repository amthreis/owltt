using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task;

public partial class Main : Node
{
    public static readonly Color[] TimerColors = new Color[] { new Color("595a58"), new Color("9a8247"), new Color("376845") };

    [Export] UITimerWindow uiTimerWindow;

    GlobalInputCSharp globalInput;

    Timer secondTimer, minuteTimer;

    public bool IsActive { get; private set; } = false;

    public void Activate(bool enable)
    {
        //GD.Print("ACTIVATE ", enable);
        IsActive = enable;

        secondTimer.ProcessMode = enable ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        minuteTimer.ProcessMode = enable ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    static Action[] _fileMBActions;

    public void UpdateAll()
    {
        UpdateHoursAndLines();
        UIScore.SetValue(GetDevScore());


        foreach(var gI in _dayGraphItem)
        {
            if (ElapsedPerDay.ContainsKey(gI.Key))
                gI.Value.UpdateBar(ElapsedPerDay[gI.Key]);
        }
    }

    public override void _Ready()
    {
        try
        {
            recentSaveToPath = Deserialize<List<string>>("user://Recent");
        }
        catch (Exception e)
        {
            recentSaveToPath = new List<string>();
        }

        GetNode("FileDialog").Connect("canceled", Callable.From(OnCancelFD));
        GetNode("FileDialog").Connect("file_selected", Callable.From<string>(OnConfirmFD));

        _fileMBActions = new Action[]
        {
            OnNew, OnLoad, () => OnSave(false), () => OnSave(true), OnOpenSettings
        };

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

        Activate(false);

        //fileMB = GetNode<MenuButton>("HUD/TopBar/File");
        fileMBCount = fileMB.GetPopup().ItemCount;


        fileMB.AboutToPopup += OnFileMBOpened;
        fileMB.GetPopup().IndexPressed += OnFileMBClicked;

        uiTimerWindow.Start(this, true);
        //SpawnGraphItem(DateTime.Now);

        UpdateHoursAndLines();
    }

    public MainSettings Settings { get; private set; }
    public MainData Data { get; private set; }

    [Export] PackedScene uiGraphItemPS;
    [Export] HBoxContainer uiGraphItemsCtr;
    [Export] UITime uiTime;
    [Export] UISettings uiSettings;

    Vector2 lastMousePos;

    public event Action<TimeSpan> SecondPassed;

    [Export] MenuButton fileMB;
    int fileMBCount;

    void OnOpenSettings()
    {
        uiSettings.Start(this);
        //GetNode<Control>("Overlay").Show();
        //GetNode<Control>("Overlay/Panel").Show();
    }

    void OnFileMBClicked(long i)
    {
        if (i < fileMBCount)
        {
            _fileMBActions[i].Invoke();
        }
        else
        {
            var rcID = i - fileMBCount;

            Load(recentSaveToPath[(int)rcID]);
        }
    }

    void OnSecond()
    {
        //GD.Print("sec");
        var second = TimeSpan.FromSeconds(1f);

        elapsed += second;
        ElapsedTotal += second;

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
        uiTimerWindow.UpdateTime(elapsed);

        //SecondPassed?.Invoke(elapsed);
        //GetNode<Label>("HUD/Actions/Total/Value").Text = FormattedTotalHours(ElapsedTotal);//.ToString(@"h\hmm");
        //GetNode<Label>("HUD/Actions/Today/Value").Text = GetElapsedToday().ToString(@"h\hmm");
    }

    public DateTime CreationDate = DateTime.Now;

    void OnMinute()
    {
        //GD.Print("Try autosave. STP = ", saveToPath);
        UIScore.SetValue(GetDevScore());

        if (saveToPath != null)
        {
            OnSave();
        }
    }

    float lastAutoSavedAtMinute;

    void Load(string path)
    {
        //var path = "C:/Users/Matheus/Documents/dev/Taskerus/Programmer.tsk";

        GD.Print("--------------LOAD");
        //var box = GetNode("HUD/Tabs/Panel/Margin/Scroll/Box");

        //foreach (var it in box.GetChildren())
        //{
        //    it.QueueFree();
        //}

        foreach (var it in _dayGraphItem)
        {
            it.Value.QueueFree();
        }

        _dayGraphItem.Clear();

        uiTimerWindow.Hide();
        // while(box.GetChildCount() == 0)
        // {
        //    box.GetChild(box.GetChildCount() - 1).QueueFree();
        // }


        path = path.Replace("\\", "/");

        var tSD = Deserialize<MainSD>(path);

        //GD.Print("tsd", tSD);
        //GD.Print("tsd.settings", tSD.Settings);


        Settings = tSD.Settings ?? new MainSettings();

        lastAutoSavedAtMinute = 0;
        CreationDate = tSD.CreationDate;
        //tasks = tSD.Tasks.OrderByDescending(t => t.State).ThenBy(t => t.Date[t.State]).ToList();
        ElapsedTotal = tSD.ElapsedTotal;
        ElapsedPerDay = tSD.ElapsedPerDay ?? new Dictionary<DateTime, TimeSpan>();

        Activate(false);
        var currDate = new DateTime(1, 1, 1);
        //TTaskState? lastState = null;

        if (ElapsedPerDay != null && ElapsedPerDay.Count > 0)
        for (var d = ElapsedPerDay.ElementAt(0).Key; d <= DateTime.Today; d = d.AddDays(1))
        {
            SpawnGraphItem(d);
        }


        saveToPath = path;

        //GetNode<Label>("HUD/Actions/Total/Value").Text = FormattedTotalHours(ElapsedTotal);
        //GetNode<Label>("HUD/Actions/Today/Value").Text = GetElapsedToday().ToString(@"h\hmm");

        uiTime.TotalHours = ElapsedTotal;
        uiTime.TotalHoursToday = GetElapsedToday();
        uiTime.TotalHoursThisWeek = GetElapsedThisWeek();

        AddSaveToPath(path);

        GetWindow().Title = $"{path} - owl.tt";

        UIScore.SetValue(GetDevScore());


        uiTimerWindow.Start(this, false);
        //SpawnGraphItem(DateTime.Now);

        UpdateAll();
        //UpdateDevScore();
    }
    List<string> recentSaveToPath = new List<string>();

    void AddSaveToPath(string path)
    {
        saveToPath = path;

        recentSaveToPath.Remove(saveToPath);
        recentSaveToPath.Insert(0, saveToPath);
    }

    public override void _ExitTree()
    {
        Serialize(recentSaveToPath, "user://Recent");

        //GD.Print("exitTree");
        if (saveToPath != null )
        {
            OnSave();
        }
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

    public void UpdateHoursAndLines()
    {
        if (hoursCtr == null || linesCtr == null)
            return;

        //GD.Print(chd.Name);

        for (var i = 0; i <= 12; i++)
        {
            var hChd = hoursCtr.GetChild<Control>(hoursCtr.GetChildCount() - 1 - i);
            var lChd = linesCtr.GetChild<Control>(linesCtr.GetChildCount() - 1 - i);

            hChd.Visible = i <= Settings.TargetHours;
            lChd.Visible = i <= Settings.TargetHours;
        }

        UIScore.UpdateControls();
    }

    void OnCancelFD()
    {
        GetNode<Control>("Overlay").Hide();

    }
    void OnConfirmFD(string path)
    {
        GetNode<Control>("Overlay").Hide();

        if (saveMode)
        {
            GD.Print("p");
            var sd = new MainSD(this);
            path = path.Replace("\\", "/");
            GD.Print(path);

            Serialize(sd, path);

            GD.Print(path);
            //saveToPath = path;
            AddSaveToPath(path);
            //GetWindow().Title = $"{path} - Taskeru";

            GD.Print("Saved to ", path);

            //NoAsterisk();
        }
        else
        {
            Load(path);
            AddSaveToPath(path);

            //saveToPath = path;

            GD.Print("Loaded from ", path);
        }

    }

    void OnLoad()
    {
        //Load("user://Tasks");

        saveMode = false;
        GetNode<Control>("Overlay").Show();

        var fd = GetNode<FileDialog>("FileDialog");

        fd.FileMode = FileDialog.FileModeEnum.OpenFile;
        fd.Filters = new string[] { "*.owltt ; owl.tt Files" };
        fd.Show();

        //recentSaveToPath = 
        //GetNode("FileDialog").Set("file_mode", 0);
        //GetNode("FileDialog").Call("set_filters", new string[] { "*.owltt ; owl.tt Files" });
        //GetNode("FileDialog").Call("show");
    }

    bool saveMode;
    string saveToPath;

    public TimeSpan elapsed;
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
                    //GD.Print($"Score of {currDay.ToString("dd/MM/yyyy")} ({currDay.DayOfWeek}): {dayScore}, w: {dayWeight}");
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
        

        //GD.Print($"-------- total score:  { score / weight }");

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

    void OnFileMBOpened()
    {
        var i = fileMBCount;

        while (fileMB.GetPopup().ItemCount > fileMBCount)
        {
            fileMB.GetPopup().RemoveItem(fileMB.GetPopup().ItemCount - 1);
        }

        foreach (var rc in recentSaveToPath)
        {
            fileMB.GetPopup().AddItem(rc);
        }

        //fileMB.GetPopup().SetItemShortcut
        //fileMB.GetPopup().RemoveItem();
        //fileMB.GetPopup().AddItem();
    }

    void OnNew()
    {
        GD.Print("--------------NEW");
        

        foreach (var it in _dayGraphItem)
        {
            it.Value.QueueFree();
        }
        _dayGraphItem.Clear();

        uiTimerWindow.Hide();
        lastAutoSavedAtMinute = 0;

        Activate(false);

        CreationDate = DateTime.Now;
        //tasks = new List<TTask>();
        ElapsedTotal = TimeSpan.Zero;
        ElapsedPerDay = new Dictionary<DateTime, TimeSpan>();

        uiTime.Reset();


        uiTimerWindow.Start(this, false);
        //SpawnGraphItem(DateTime.Now);

        UpdateHoursAndLines();
    }

    void OnSave(bool saveAs = false)
    {
        saveMode = true;
        //Serialize(new TTaskerSD(this), "user://Tasks");

        if (!saveAs && saveToPath != null)
        {
            //saveToPath = "";
            GD.Print("Saved to ", saveToPath);
            Serialize(new MainSD(this), saveToPath);

            //NoAsterisk();
        }
        else
        {
            GetNode<Control>("Overlay").Show();

            var fd = GetNode<FileDialog>("FileDialog");

            //fd.FileMode = FileDialog.FileModeEnum.SaveFile;
            //fd.Filters = new string[] { "*.owltt ; owl.tt Files" };
            //fd.Show();

            GetNode("FileDialog").Set("file_mode", 4);
            GetNode("FileDialog").Call("set_filters", new string[] { "*.owltt ; owl.tt Files" });
            GetNode("FileDialog").Call("show");
            //GD.Print("Done");
        }
    }

    public void SetScore(float score)
    {
        UIScore.SetValue(score);
    }

    public static void Serialize(object o, string path, Action callback = null)
    {
        //GD.Print("serialize");
        var lastSlash = path.LastIndexOf('/');
        //GD.Print("lastSlash", lastSlash);

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
