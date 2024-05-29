using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    //[Tool]
    public partial class UIGraphItem : VBoxContainer
    {
        Main m;
        float _value;

        [Export(PropertyHint.Range, "0, 1")]
        public float Value 
        { 
            get { return _value; }
            set {  
                _value = value;

                GetNode<Control>("Remainder").SizeFlagsStretchRatio = 1f - _value;
                GetNode<Panel>("Panel").SizeFlagsStretchRatio = _value;
            }
        }

        public void UpdateBar(TimeSpan time)
        {
            var timeRatio = (float)(time / TimeSpan.FromHours(m.Settings.TargetHours));

            GetNode<Control>("Panel").SizeFlagsStretchRatio = timeRatio;
            GetNode<Control>("Remainder").SizeFlagsStretchRatio = 1f - timeRatio;
        }

        public void SetDTS(Main m, DateTime day, TimeSpan time)
        {
            this.m = m;

            var date = day.ToString("dd/MM/yyyy").Split("/");// $"{}\n({day.DayOfWeek.ToString()[0..3]})".Split();

            GetNode<Label>("Panel/Info/Weekday").Text = day.DayOfWeek.ToString()[0..3];

            var dateCt = GetNode("Panel/Info/Date");

            dateCt.GetNode<Label>("Day").Text = date[0];
            dateCt.GetNode<Label>("Month").Text = date[1];
            dateCt.GetNode<Label>("Year").Text = date[2];

            GetNode<Label>("Panel/Hours").Text = $"{time.ToString(@"h\hmm")}";

            //GetNode<Label>("Box/Content/Bar/Bar/Value").Text = $"{time.ToString(@"h\hmm")}";

            //MaxTime.ra

            //var isWeekend = day.DayOfWeek == DayOfWeek.Sunday || day.DayOfWeek == DayOfWeek.Saturday;
            UpdateBar(time);

            //GetNode<Control>("Box/Content/Bar/Bar").SelfModulate = new Color(isWeekend ? "837f89" : "856aad");

            // GetNode<Control>("Box/Content/Bar/Bar").SelfModulate = tasker.GetScoreColor(tasker.GetDayScore(day));
        }
    }
}
