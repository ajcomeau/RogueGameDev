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
                pnlName.Visible = false;
                lblArray.Text = currentGame.CurrentMap.MapText();
                lblStatusMsg.Text = currentGame.StatusMessage;
                lblStats.Text = currentGame.StatsDisplay;
            }
            else
                MessageBox.Show("Please enter a name for your character.");
        }

        private void DungeonMain_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void DungeonMain_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void DungeonMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentGame != null)
            {
                Debug.WriteLine(e.KeyValue);
                currentGame.KeyHandler(e.KeyValue, e.Shift, e.Control);
                
                lblArray.Text = currentGame.DevMode ? 
                    currentGame.CurrentMap.MapCheck() : currentGame.CurrentMap.MapText();
                
                lblStatusMsg.Text = currentGame.StatusMessage;
                lblStats.Text = currentGame.StatsDisplay;
            }
        }
    }
}