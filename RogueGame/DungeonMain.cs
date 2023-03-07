namespace RogueGame
{
    public partial class DungeonMain : Form
    {
        Game currentGame;

        public DungeonMain()
        {
            InitializeComponent();
            currentGame = new Game();
        }

        private void DungeonMain_Load(object sender, EventArgs e)
        {
            lblArray.Text = currentGame.CurrentMap.MapText();
        }


        private void DungeonMain_KeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show(e.KeyValue.ToString());
        }
    }
}