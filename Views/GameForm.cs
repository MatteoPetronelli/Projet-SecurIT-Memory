using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SecurIT_Memory.Core;
using SecurIT_Memory.Models;

namespace SecurIT_Memory.Views
{
    public class GameForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Timer _gameTimer;
        private readonly Timer _revealTimer;
        private TableLayoutPanel _gridPanel;
        private Label _attemptsLabel;
        private Label _timerLabel;
        private Button _restartButton;
        private PictureBox _firstBox;
        private PictureBox _secondBox;
        private bool _isBusy;
        private int _elapsedSeconds;
        private int _gridSize;
        private int _pairCount;
        private Image _cardBackImage;
        private readonly Dictionary<int, Image> _faceImages;
        private List<string> _selectedImagePaths;

        public GameForm()
        {
            Text = "Jeu - SecurIT Memory";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 740);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(20, 20, 20);

            _gameManager = new GameManager();
            _gameTimer = new Timer { Interval = 1000 };
            _revealTimer = new Timer();
            _faceImages = new Dictionary<int, Image>();
            _cardBackImage = CreateCardBackImage();

            _gameTimer.Tick += GameTimer_Tick;
            _revealTimer.Tick += RevealTimer_Tick;

            InitializeComponents();
            StartNewGame();
        }

        private void InitializeComponents()
        {
            _attemptsLabel = new Label
            {
                Text = "Essais : 0",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 14)
            };

            _timerLabel = new Label
            {
                Text = "Temps : 00:00",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(180, 14)
            };

            _restartButton = new Button
            {
                Text = "Nouvelle partie",
                Size = new Size(140, 34),
                Location = new Point(560, 10),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _restartButton.Click += RestartButton_Click;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            topPanel.Controls.Add(_attemptsLabel);
            topPanel.Controls.Add(_timerLabel);
            topPanel.Controls.Add(_restartButton);

            _gridPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(20, 20, 20)
            };

            Controls.Add(_gridPanel);
            Controls.Add(topPanel);
        }

        private void StartNewGame()
        {
            _gridSize = GameSettings.GridSize;
            _pairCount = _gridSize * _gridSize / 2;
            _elapsedSeconds = 0;
            _isBusy = false;
            _firstBox = null;
            _secondBox = null;
            _faceImages.Clear();
            _cardBackImage = CreateCardBackImage();

            LoadThemeImages();
            _gameManager.InitializeGame(_selectedImagePaths);
            BuildGrid();
            _gameTimer.Stop();
            _gameTimer.Start();
            UpdateLabels();
        }

        private void LoadThemeImages()
        {
            var imagePaths = ThemeManager.GetImagePaths(GameSettings.ThemeName);
            if (imagePaths.Count == 0)
            {
                MessageBox.Show("Aucune image thématique trouvée. \nAjoutez des fichiers PNG/JPG dans le dossier Ressources/Images/Crypto.", "Images manquantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _selectedImagePaths = imagePaths.Take(_pairCount).ToList();
            while (_selectedImagePaths.Count < _pairCount)
            {
                if (imagePaths.Count > 0)
                {
                    _selectedImagePaths.Add(imagePaths[_selectedImagePaths.Count % imagePaths.Count]);
                }
                else
                {
                    _selectedImagePaths.Add(string.Empty);
                }
            }
        }

        private void BuildGrid()
        {
            _gridPanel.Controls.Clear();
            _gridPanel.ColumnStyles.Clear();
            _gridPanel.RowStyles.Clear();
            _gridPanel.ColumnCount = _gridSize;
            _gridPanel.RowCount = _gridSize;
            _gridPanel.Dock = DockStyle.Fill;

            for (int i = 0; i < _gridSize; i++)
            {
                _gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / _gridSize));
                _gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / _gridSize));
            }

            var cards = _gameManager.Cards;
            for (int index = 0; index < cards.Count; index++)
            {
                var card = cards[index];
                var pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Black,
                    Cursor = Cursors.Hand,
                    Tag = card,
                    Image = _cardBackImage
                };

                pictureBox.Click += CardBox_Click;
                _gridPanel.Controls.Add(pictureBox, index % _gridSize, index / _gridSize);
            }
        }

        private void CardBox_Click(object sender, EventArgs e)
        {
            if (_isBusy)
            {
                return;
            }

            if (sender is not PictureBox box || box.Tag is not Card card)
            {
                return;
            }

            if (card.CurrentState != CardState.Hidden)
            {
                return;
            }

            if (_firstBox == box)
            {
                return;
            }

            RevealCard(box, card);

            if (_firstBox == null)
            {
                _firstBox = box;
                return;
            }

            _secondBox = box;
            _isBusy = true;
            bool match = _gameManager.CheckMatch((Card)_firstBox.Tag, (Card)_secondBox.Tag);
            UpdateLabels();

            if (match)
            {
                _firstBox = null;
                _secondBox = null;
                _isBusy = false;
                CheckVictory();
                return;
            }

            _revealTimer.Interval = GameSettings.RevealDelayMilliseconds;
            _revealTimer.Start();
        }

        private void RevealCard(PictureBox box, Card card)
        {
            box.Image = GetFaceImage(card);
            card.CurrentState = CardState.Revealed;
        }

        private void HideCard(PictureBox box, Card card)
        {
            if (card.CurrentState == CardState.Matched)
            {
                return;
            }

            card.CurrentState = CardState.Hidden;
            box.Image = _cardBackImage;
        }

        private void RevealTimer_Tick(object sender, EventArgs e)
        {
            _revealTimer.Stop();

            if (_firstBox != null && _secondBox != null)
            {
                HideCard(_firstBox, (Card)_firstBox.Tag);
                HideCard(_secondBox, (Card)_secondBox.Tag);
            }

            _firstBox = null;
            _secondBox = null;
            _isBusy = false;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            _timerLabel.Text = $"Temps : {TimeSpan.FromSeconds(_elapsedSeconds):mm\\:ss}";
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            _gameTimer.Stop();
            StartNewGame();
        }

        private void UpdateLabels()
        {
            _attemptsLabel.Text = $"Essais : {_gameManager.Attempts}";
            _timerLabel.Text = $"Temps : {TimeSpan.FromSeconds(_elapsedSeconds):mm\\:ss}";
        }

        private void CheckVictory()
        {
            if (_gameManager.CheckVictory(_pairCount))
            {
                _gameTimer.Stop();
                MessageBox.Show($"Victoire !\nTemps total : {TimeSpan.FromSeconds(_elapsedSeconds):mm\\:ss}\nNombre d'essais : {_gameManager.Attempts}", "Bravo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Image GetFaceImage(Card card)
        {
            if (!_faceImages.TryGetValue(card.Id, out Image image))
            {
                image = LoadFaceImage(card);
                _faceImages[card.Id] = image;
            }

            return image;
        }

        private Image LoadFaceImage(Card card)
        {
            if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
            {
                return Image.FromFile(card.ImagePath);
            }

            return CreatePlaceholderFace(card.Id);
        }

        private Image CreateCardBackImage()
        {
            var bitmap = new Bitmap(220, 220);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(55, 55, 60));
                using var font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
                using var brush = new SolidBrush(Color.White);
                g.DrawString("SECURIT", font, brush, new PointF(16, 90));
            }

            return bitmap;
        }

        private Image CreatePlaceholderFace(int id)
        {
            var bitmap = new Bitmap(220, 220);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(20, 100, 180));
                using var font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
                using var brush = new SolidBrush(Color.White);
                g.DrawString($"Carte {id}", font, brush, new PointF(22, 90));
            }

            return bitmap;
        }
    }
}
