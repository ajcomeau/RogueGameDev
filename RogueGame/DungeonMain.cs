using System.ComponentModel;
using System.Diagnostics;

namespace RogueGame
{
    public partial class DungeonMain : Form
    {
        Game? currentGame;

        public DungeonMain()
        {
            InitializeComponent();
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
                lblArray.Text = currentGame.ScreenDisplay;
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

                    lblArray.Text = currentGame.ScreenDisplay;

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
    }
}