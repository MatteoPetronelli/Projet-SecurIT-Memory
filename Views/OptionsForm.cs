using System;
using System.Drawing;
using System.Windows.Forms;
using SecurIT_Memory.Core;

namespace SecurIT_Memory.Views
{
    public class OptionsForm : Form
    {
        private RadioButton _grid4x4Radio;
        private RadioButton _grid6x6Radio;
        private RadioButton _easyRadio;
        private RadioButton _normalRadio;
        private RadioButton _hardRadio;
        private Button _saveButton;

        public OptionsForm()
        {
            Text = "Options";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 340);
            Padding = new Padding(16);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;

            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            var sizeGroup = new GroupBox
            {
                Text = "Taille de la grille",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            _grid4x4Radio = new RadioButton { Text = "4 × 4", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.LightGray };
            _grid6x6Radio = new RadioButton { Text = "6 × 6", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.LightGray };
            sizeGroup.Controls.Add(_grid6x6Radio);
            sizeGroup.Controls.Add(_grid4x4Radio);

            var difficultyGroup = new GroupBox
            {
                Text = "Niveau de difficulté",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 140,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            _easyRadio = new RadioButton { Text = "Facile", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.LightGray };
            _normalRadio = new RadioButton { Text = "Normal", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.LightGray };
            _hardRadio = new RadioButton { Text = "Difficile", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.LightGray };
            difficultyGroup.Controls.Add(_hardRadio);
            difficultyGroup.Controls.Add(_normalRadio);
            difficultyGroup.Controls.Add(_easyRadio);

            _saveButton = new Button
            {
                Text = "Enregistrer",
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10)
            };
            _saveButton.Click += SaveButton_Click;

            Controls.Add(_saveButton);
            Controls.Add(difficultyGroup);
            Controls.Add(sizeGroup);
        }

        private void LoadSettings()
        {
            _grid4x4Radio.Checked = GameSettings.GridSize == 4;
            _grid6x6Radio.Checked = GameSettings.GridSize == 6;
            _easyRadio.Checked = GameSettings.Difficulty == "Facile";
            _normalRadio.Checked = GameSettings.Difficulty == "Normal";
            _hardRadio.Checked = GameSettings.Difficulty == "Difficile";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            GameSettings.GridSize = _grid6x6Radio.Checked ? 6 : 4;
            GameSettings.Difficulty = _easyRadio.Checked ? "Facile" : _hardRadio.Checked ? "Difficile" : "Normal";
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
