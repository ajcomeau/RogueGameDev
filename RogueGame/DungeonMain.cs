using System.ComponentModel;
using System.Diagnostics;

namespace RogueGame
{
    public partial class DungeonMain : Form
    {
        Game? currentGame;
        Font mapFont = new Font("Consolas", 14, FontStyle.Regular);

        public DungeonMain()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartGame(txtName.Text);
        }

        private void StartGame(string PlayerName = "")
        {
            if (PlayerName.Length > 0)
            {
                currentGame = new Game(txtName.Text);
                listStatus.DataSource = currentGame.StatusList;
                pnlName.Visible = false;
                this.Invalidate(true);
                lblStats.Text = currentGame.StatsDisplay();
            }
            else
                MessageBox.Show("Please enter a name for your character.");
        }


        private void DungeonMain_KeyDown(object sender, KeyEventArgs e)
        {

            if (currentGame != null)
            {
                // Don't send keys until the game has been instantiated
                // and then don't send CTRL / SHIFT / ALT.
                if (e.KeyValue > 18)
                {
                    Debug.WriteLine(e.KeyValue);
                    currentGame.KeyHandler(e.KeyValue, e.Shift, e.Control);

                    this.Invalidate(true);

                    lblStats.Text = currentGame.StatsDisplay();
                    listStatus.SelectedIndex = 0;
                    listStatus.SelectedIndex = -1;
                }

                // If the game has ended, offer the choice of starting a new game.
                if (currentGame.GameMode == Game.DisplayMode.GameOver)
                {
                    lblStats.Text = "";

                    pnlName.Visible = true;
                    txtName.Text = currentGame.CurrentPlayer.PlayerName;
                }
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            int cellWidth = 10;
            int cellHeight = 18;
            int px, py;

            if (currentGame != null)
            {
                for (int y = 0; y < currentGame.ScreenDisplay.GetLength(1); y++)
                {
                    for (int x = 0; x < currentGame.ScreenDisplay.GetLength(0); x++)
                    {
                        MapGlyph g = currentGame.ScreenDisplay[x, y];
                        px = x * cellWidth;
                        py = y * cellHeight + 150;

                        TextRenderer.DrawText(
                            e.Graphics,
                            g.DisplayChar.ToString(),
                            mapFont,
                            new Point(px, py),
                            g.Foreground,
                            g.Background,
                            TextFormatFlags.NoPadding);
                    }
                }
            }

        }
    }

    public class RoguePanel : Panel
    {
        public RoguePanel()
        {
            DoubleBuffered = true;
            //ResizeRedraw = true;
        }
    }
}