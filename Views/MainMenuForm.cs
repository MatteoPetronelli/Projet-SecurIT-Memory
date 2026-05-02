using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecurIT_Memory.Views
{
    public class MainMenuForm : Form
    {
        private Button _playButton;
        private Button _optionsButton;
        private Button _quitButton;
        private Label _titleLabel;
        private Label _settingsLabel;

        public MainMenuForm()
        {
            Text = "SecurIT Memory";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(440, 480);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _titleLabel = new Label
            {
                Text = "SecurIT Memory",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(100, 200, 240)
            };

            _settingsLabel = new Label
            {
                Text = "Taille de la grille : 4×4 | Difficulté : Normal",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray
            };

            _playButton = CreateMenuButton("Jouer", PlayButton_Click);
            _optionsButton = CreateMenuButton("Options", OptionsButton_Click);
            _quitButton = CreateMenuButton("Quitter", QuitButton_Click);
            _quitButton.Margin = new Padding(0, 10, 0, 0);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 20, 0, 20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Anchor = AnchorStyles.None
            };

            buttonPanel.Controls.Add(_playButton);
            buttonPanel.Controls.Add(_optionsButton);
            buttonPanel.Controls.Add(_quitButton);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.Transparent
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(_titleLabel, 1, 0);
            layout.Controls.Add(_settingsLabel, 1, 1);
            layout.Controls.Add(buttonPanel, 1, 2);
            layout.SetCellPosition(buttonPanel, new TableLayoutPanelCellPosition(1, 2));

            Controls.Add(layout);
        }

        private Button CreateMenuButton(string text, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Height = 50,
                Width = 320,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point)
            };

            button.Click += clickHandler;
            return button;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            using (var gameForm = new GameForm())
            {
                gameForm.ShowDialog(this);
                RefreshSettingsLabel();
            }
        }

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            using (var optionsForm = new OptionsForm())
            {
                optionsForm.ShowDialog(this);
            }

            RefreshSettingsLabel();
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RefreshSettingsLabel()
        {
            _settingsLabel.Text = $"Taille de la grille : {Core.GameSettings.GridSize}×{Core.GameSettings.GridSize} | Difficulté : {Core.GameSettings.Difficulty}";
        }
    }
}
