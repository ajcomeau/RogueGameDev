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
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadMapLevel();
        }
    }
}