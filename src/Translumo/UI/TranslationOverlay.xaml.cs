using System;
using System.Windows;
using System.Windows.Threading;

namespace Translumo.UI
{
    public partial class TranslationOverlay : Window
    {
        private readonly DispatcherTimer followTimer;
        private bool pinned = false;
        private System.Drawing.Point lastCursorPosition;

        private string FrontText;
        private string BackText;

        public TranslationOverlay(string original, string translated)
        {
            InitializeComponent();

            OriginalText.Text = original;
            TranslatedText.Text = translated;

            var p = System.Windows.Forms.Cursor.Position;
            this.Left = p.X + 16;
            this.Top = p.Y + 16;

            followTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
            followTimer.Tick += FollowTimer_Tick;
            followTimer.Start();

            PinButton.Click += (s, e) => TogglePin();
            CloseButton.Click += (s, e) => Close();
            AnkiButton.Click += async (s, e) =>
            {
                try
                {
                    var svc = new Translumo.Services.AnkiService();
                    await svc.AddBasicNoteAsync(FrontText ?? original, BackText ?? translated);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Anki error: " + ex.Message);
                }
            };

            this.MouseLeftButtonDown += (s, e) =>
            {
                try { this.DragMove(); pinned = true; UpdatePinButton(); followTimer.Stop(); } catch { }
            };

            SetFields(original, translated);
        }

        private void FollowTimer_Tick(object? sender, EventArgs e)
        {
            if (pinned) return;
            var p = System.Windows.Forms.Cursor.Position;
            if (p == lastCursorPosition) return;
            lastCursorPosition = p;
            this.Left = p.X + 16;
            this.Top = p.Y + 16;
        }

        private void TogglePin()
        {
            pinned = !pinned;
            if (pinned) followTimer.Stop(); else followTimer.Start();
            UpdatePinButton();
        }

        private void UpdatePinButton() => PinButton.Content = pinned ? "Pinned" : "Pin";

        public void SetFields(string front, string back)
        {
            FrontText = front;
            BackText = back;
        }
    }
}
