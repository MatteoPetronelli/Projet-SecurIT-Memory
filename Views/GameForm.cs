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
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
            File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} [GameForm] Theme: {GameSettings.ThemeName}, Images found: {imagePaths.Count}\n");
            
            foreach (var p in imagePaths)
            {
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   - {p}\n");
            }

            if (imagePaths.Count == 0)
            {
                string rootPath = ThemeManager.GetImageRootPath();
                string expectedPath = Path.Combine(rootPath, GameSettings.ThemeName);
                string msg = $"Aucune image thématique trouvée pour le thème '{GameSettings.ThemeName}'.\nChemin attendu : {expectedPath}\nAjoutez des fichiers PNG/JPG/JPEG dans ce dossier.";
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} [GameForm] {msg}\n");
                MessageBox.Show(msg, "Images manquantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
            File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} [LoadFaceImage] Card {card.Id}: ImagePath='{card.ImagePath}'\n");

            if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
            {
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   File exists\n");
                try
                {
                    // Try loading with FileStream first (more forgiving for problematic JPEGs)
                    using (var stream = new FileStream(card.ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var original = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
                        File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   Loaded via stream: {original.Width}x{original.Height}\n");
                        // Clone the image to free the stream
                        var clone = (Image)original.Clone();
                        original.Dispose();
                        var resized = ResizeImage(original, 220, 220);
                        return AddTextOverlay(resized, card.Id.ToString());
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   Stream load failed ({ex.GetType().Name}), trying Bitmap...\n");
                    try
                    {
                        var bitmap = new Bitmap(card.ImagePath);
                        File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   Loaded via Bitmap: {bitmap.Width}x{bitmap.Height}\n");
                        var resized = ResizeImage(bitmap, 220, 220);
                        return AddTextOverlay(resized, card.Id.ToString());
                    }
                    catch (Exception ex2)
                    {
                        File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   Bitmap load failed ({ex2.GetType().Name}), generating theme image...\n");
                        return GenerateThemeImage(card.Id);
                    }
                }
            }
            else
            {
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss}   File does not exist, generating theme image...\n");
                return GenerateThemeImage(card.Id);
            }
        }

        private Image GenerateThemeImage(int cardId)
        {
            // Generate a simple themed image based on the theme name
            var bitmap = new Bitmap(220, 220);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Generate a hash-based color from theme name
                int themeHash = GameSettings.ThemeName.GetHashCode();
                int r = 100 + (Math.Abs(themeHash) % 156);
                int g = 100 + ((Math.Abs(themeHash) / 256) % 156);
                int b = 100 + ((Math.Abs(themeHash) / 65536) % 156);
                Color themeColor = Color.FromArgb(r, g, b);

                // Fill background
                graphics.Clear(themeColor);

                // Draw theme name
                using (var font = new Font("Segoe UI", 14, FontStyle.Bold, GraphicsUnit.Point))
                {
                    graphics.DrawString(GameSettings.ThemeName, font, Brushes.White, 10, 20);
                }

                // Draw card number prominently
                using (var largeFont = new Font("Segoe UI", 60, FontStyle.Bold, GraphicsUnit.Point))
                {
                    string text = cardId.ToString();
                    var textSize = graphics.MeasureString(text, largeFont);
                    float x = (220 - textSize.Width) / 2;
                    float y = (220 - textSize.Height) / 2 + 10;
                    graphics.DrawString(text, largeFont, Brushes.White, x, y);
                }
            }
            return bitmap;
        }

        private Image ResizeImage(Image source, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(source, 0, 0, width, height);
            }
            return resized;
        }

        private Image AddTextOverlay(Image originalImage, string text)
        {
            var finalImage = new Bitmap(originalImage);
            using (var graphics = Graphics.FromImage(finalImage))
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                using (var font = new Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Point))
                {
                    var textSize = graphics.MeasureString(text, font);

                    var rect = new RectangleF(5, 5, textSize.Width + 4, textSize.Height + 4);
                    using (var bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    {
                        graphics.FillRectangle(bgBrush, rect);
                    }

                    graphics.DrawString(text, font, Brushes.White, 7, 7);
                }
            }
            return finalImage;
        }

        private Image CreateCardBackImage()
        {
            var bitmap = new Bitmap(220, 220);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(55, 55, 60));
                using var font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
                using var brush = new SolidBrush(Color.White);
                var text = "Security";
                var textSize = g.MeasureString(text, font);
                var x = (bitmap.Width - textSize.Width) / 2;
                var y = (bitmap.Height - textSize.Height) / 2;
                g.DrawString(text, font, brush, new PointF(x, y));
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
