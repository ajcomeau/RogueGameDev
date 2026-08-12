using System.Diagnostics;
using static RogueGame.Game;

namespace RogueGame
{
    public partial class DungeonMain : Form
    {
        Game? currentGame;
        Font mapFont = new Font("Consolas", 16, FontStyle.Regular);

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
                pnlName.Enabled = false;
                this.Invalidate(true);  // Invalidate to draw map.
                lblStats.Text = currentGame.StatsDisplay();
            }
            else
                MessageBox.Show("Please enter a name for your character.");
        }


        private void DungeonMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentGame != null)
            {
                if (currentGame.GameMode < DisplayMode.GameOver)
                {
                    // Invalidate to redraw map.
                    this.Invalidate(true);
                    listStatus.Visible = (currentGame.GameMode is DisplayMode.Primary or DisplayMode.Inventory);

                    // Don't send keys until the game has been instantiated
                    // and then don't send CTRL / SHIFT / ALT.
                    if (e.KeyValue > 18)
                    {
                        e.SuppressKeyPress = true;
                        currentGame.KeyHandler(e.KeyValue, e.Shift, e.Control);
                        lblStats.Text = currentGame.StatsDisplay();
                        listStatus.SelectedIndex = 0;
                        listStatus.SelectedIndex = -1;
                    }
                }

                // Evaluate GameMode again. If the game has ended, clear and let player start new game.                
                if (currentGame.GameMode >= DisplayMode.GameOver)
                {
                    listStatus.Visible = false;
                    pnlName.Visible = true;
                    pnlName.Enabled = true;
                    txtName.Text = currentGame.CurrentPlayer.CharacterName;
                    lblStats.Text = "";
                }

                e.Handled = true;
            }           

        }


        protected override void OnPaint(PaintEventArgs e)
        {
            // Redraw the map from the ScreenDisplay array.
            int cellWidth = 12;
            int cellHeight = 24;
            int px, py;
            string[] lines;

            if (currentGame == null)
            {                
                lines = TitleScreen().Split('\n');

                for (int y = 0; y < lines.Length; y++)
                {
                    TextRenderer.DrawText(
                        e.Graphics,
                        lines[y],
                        mapFont,
                        new Point(75, (y * cellHeight) + 25),
                        Color.SandyBrown, Color.Black,
                        TextFormatFlags.NoPadding);
                }
            }
            else
            {
                // Iterate through array cells and draw glyphs on screen.
                for (int y = 0; y < currentGame.CurrentMap.DisplayMap.GetLength(1); y++)
                {
                    for (int x = 0; x < currentGame.CurrentMap.DisplayMap.GetLength(0); x++)
                    {
                        MapGlyph g = currentGame.CurrentMap.DisplayMap[x, y];
                        px = x * cellWidth + 25;   // Add pixels on top and left as margin.
                        py = y * cellHeight + 125;

                        TextRenderer.DrawText(
                            e.Graphics,
                            g.DisplayChar.ToString(),
                            mapFont,
                            new Point(px, py - 2),
                            g.Foreground,
                            g.Background,
                            TextFormatFlags.NoPadding);
                    }
                }
            }
        }

        private string TitleScreen()
        {
            string screenText;
            // Assemble the ASCII graphic and return it.
            screenText = "\n\n" +
            "   ╔════════════════════════════════════════════════════════════════════════╗\n" +
            "   ║                                                                        ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                               ROGUE C#                                 ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                    An homage to the original Rogue                     ║\n" +
            "   ║                            written in C#.                              ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                      Designed by Andrew Comeau                         ║\n" +
            "   ║               as a demo program for the Rogue C# series.               ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                     https://www.AndrewComeau.com                       ║\n" +
            "   ║               https://github.com/ajcomeau/RogueGameDev                 ║\n" +
            "   ║                                                                        ║\n" +
            "   ║                                                                        ║\n" +
            "   ╚════════════════════════════════════════════════════════════════════════╝\n" +
           
            "\n";

            return screenText;

        }
    }
}