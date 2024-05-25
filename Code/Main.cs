using Godot;

namespace Task
{
    [Tool]
    public partial class Main : Node
    {
        int _targetHours;

        [Export(PropertyHint.Range, "1, 12")]
        public int TargetHours
        {
            get => _targetHours;
            set
            {
                _targetHours = value;

                UpdateHoursAndLines();
            }
        }

        [Export] VBoxContainer linesCtr;
        [Export] VBoxContainer hoursCtr;

        void UpdateHoursAndLines()
        {
            if (hoursCtr == null || linesCtr == null)
                return;

            //GD.Print(chd.Name);

            for (var i = 0; i < 12; i++)
            {
                var hChd = hoursCtr.GetChild<Control>(hoursCtr.GetChildCount() - 2 - i);
                var lChd = linesCtr.GetChild<Control>(linesCtr.GetChildCount() - 1 - i);

                hChd.Visible = i < _targetHours;
                lChd.Visible = i < _targetHours;
            }
        }
    }
}
