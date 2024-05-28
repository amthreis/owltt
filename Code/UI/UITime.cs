using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    public partial class UITime : HBoxContainer
    {
        TimeSpan _totalHours;
        TimeSpan _totalHoursThisWeek;
        TimeSpan _totalHoursToday;

        public TimeSpan TotalHours
        {
            get => _totalHours;
            set
            {
                _totalHours = value;
                //GetNode<Label>("Total/Value").Text = "";

                GetNode<Label>("Total/Value").Text = _totalHours.ToString(@"h\hmm"); //FormattedTotalHours(_totalHours);//.ToString(@"h\hmm");
            }
        }

        public TimeSpan TotalHoursThisWeek
        {
            get => _totalHoursThisWeek;
            set
            {
                _totalHoursThisWeek = value;
                GetNode<Label>("ThisWeek/Value").Text = _totalHoursThisWeek.ToString(@"h\hmm");
            }
        }

        public TimeSpan TotalHoursToday
        {
            get => _totalHoursToday;
            set
            {
                _totalHoursToday = value;
                GetNode<Label>("Today/Value").Text = _totalHoursToday.ToString(@"h\hmm");
            }
        }

        string FormattedTotalHours(TimeSpan span)
        {
            return string.Format("{0}h{1}", (int)span.TotalHours, span.Minutes);
        }
    }
}
