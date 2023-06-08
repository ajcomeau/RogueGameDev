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
            if (txtName.TextLength > 0)
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
            // Don't send keys until the game has been instantiated
            // and then don't send CTRL / SHIFT / ALT.
            if (currentGame != null && e.KeyValue > 18)
            {
                Debug.WriteLine(e.KeyValue);
                currentGame.KeyHandler(e.KeyValue, e.Shift, e.Control);

                lblArray.Text = currentGame.ScreenDisplay;

                lblStats.Text = currentGame.StatsDisplay();
                listStatus.SelectedIndex = 0;
                listStatus.SelectedIndex = -1;
            }
        }
    }
}