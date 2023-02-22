namespace RogueGame
{
    public partial class DungeonMain : Form
    {
        public DungeonMain()
        {
            InitializeComponent();
        }

        private void DungeonMain_Load(object sender, EventArgs e)
        {
            LoadMapLevel();

        }

        private void LoadMapLevel()
        {
            MapLevel newLevel = new MapLevel();

            lblArray.Text = newLevel.MapText();
            Application.DoEvents();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 101; i++)
            {
                LoadMapLevel();
            }

        }

private void btnNext_Click(object sender, EventArgs e)
{
    LoadMapLevel();
}
    }
}