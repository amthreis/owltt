using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Task
{
    public partial class Main : Node
    {
        GlobalInputCSharp globalInput;

        public override void _Ready()
        {
            Settings = new MainSettings(this);
            Data = new MainData();

            globalInput = GetNode<GlobalInputCSharp>("/root/GlobalInput/GlobalInputCSharp");

        }

        public MainSettings Settings { get; private set; }
        public MainData Data { get; private set; }
        //float _saveEveryMinutes;

        //int _targetHours;

        //[Export(PropertyHint.Range, "1, 12")]
        //public int TargetHours
        //{
        //    get => _targetHours;
        //    set
        //    {
        //        _targetHours = value;

        //        UpdateHoursAndLines();
        //    }
        //}

        Vector2 lastMousePos;

        public override void _PhysicsProcess(double dT)
        {
            var mousePos = globalInput.GetMousePosition();

            if (mousePos != lastMousePos)
            {
                Data.InactivityTimer = 0f;
            }

            lastMousePos = mousePos;

            Data.InactivityTimer += (float)dT;

            GD.Print(Mathf.Floor(Data.InactivityTimer));
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
}
