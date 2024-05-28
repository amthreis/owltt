using Godot;

namespace Task
{
    [Tool]
    public partial class Main : Node
    {
        public override void _Ready()
        {
            Settings = new MainSettings(this);
            Data = new MainData();
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

        public override void _Process(double dT)
        {
            Data.InactivityTimer += (float)dT;

            GD.Print(Mathf.Floor(Data.InactivityTimer));
        }

        public override void _Input(InputEvent ev)
        {
            Data.InactivityTimer = 0f;
        }

        [Export] VBoxContainer linesCtr;
        [Export] VBoxContainer hoursCtr;
        [Export] public UIScore UIScore;

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
    }
}
