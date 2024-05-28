using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    //[Tool]
    public partial class UIScore : TextureProgressBar
    {
        float _score;

        [Export(PropertyHint.Range, "0, 1")] public float Score
        {
            get => _score;
            set {
                _score = value;

                UpdateControls();
            }
        }

        [Export] Gradient colorGrad;

        public override void _Ready()
        {
            Score = 0f;
        }

        public void SetValue(float score)
        {
            _score = score;

            UpdateControls();
        }

        public void UpdateControls()
        {
            if (GetNodeOrNull<Label>("Value") == null)
                return;

            Value = _score;
            TintProgress = colorGrad.Sample(_score);

            GetNode<Label>("Value").Text = $"{Mathf.Round(_score * 100) }%";
            GetNode<Label>("Value").SelfModulate = colorGrad.Sample(_score);
        }
    }
}
