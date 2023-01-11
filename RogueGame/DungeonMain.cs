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

            string arrayText = "" ;

            for (int i = 0; i < 25; i++)
            {
                arrayText += newLevel.MapRow(i);
            }

            lblArray.Text = arrayText;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadMapLevel();
        }
    }
}