using System.Diagnostics;

namespace Sandbox
{
    /// <summary>
    /// Sandbox project to test new functions before
    /// inserting them into the game.
    /// </summary>
    public partial class Form1 : Form
    {
        Game currentGame = new Game();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentGame != null)
            {
                // Don't send keys until the game has been instantiated
                // and then don't send CTRL / SHIFT / ALT.
                if (e.KeyValue > 18)
                {
                    Debug.WriteLine(e.KeyValue);
                    currentGame.KeyHandler(e.KeyValue, e.Shift, e.Control);
                }
    }
}
    }
}
