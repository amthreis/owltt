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
        float _totalHours;
        float _totalHoursThisWeek;
        float _totalHoursToday;

        public float TotalHours
        {
            get => _totalHours;
            set
            {
                _totalHours = value;
                GetNode<Label>("Total/Value").Text = "";
            }
        }

        public float TotalHoursThisWeek
        {
            get => _totalHoursThisWeek;
            set
            {
                _totalHoursThisWeek = value;
                GetNode<Label>("ThisWeek/Value").Text = "";
            }
        }

        public float TotalHoursToday
        {
            get => _totalHoursToday;
            set
            {
                _totalHoursToday = value;
                GetNode<Label>("Today/Value").Text = "";
            }
        }
    }
}
