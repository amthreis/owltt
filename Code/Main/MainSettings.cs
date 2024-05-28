using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    public class MainSettings
    {
        Main m;

        public MainSettings(Main m)
        {
            this.m = m;
        }

        public float AutoSaveEveryMinutes { get; private set; } = 1f;
        public int TargetHours { get; private set; } = 6;
        public int TargetDays { get; private set; } = 30;
        public bool WorkingDaysOnly { get; private set; } = true;

        public void SetAutoSaveEveryMinutes(float min)
        {
            AutoSaveEveryMinutes = min;
            m.UpdateHoursAndLines();
        }

        public void SetTargetHours(int tH)
        {
            TargetHours = tH;

            m.UIScore.UpdateControls();
        }
    }
}
