using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    /// <summary>
    /// Sandbox project to test new functions before
    /// inserting them into the game.
    /// </summary>
    /// 
    internal class Game
    {
        private record struct recKeyChord(int KeyPressed, bool Ctrl, bool Shift);
        private Dictionary<recKeyChord, Action> KeyActions;

        public Game()
        {
            KeyActions = new Dictionary<recKeyChord, Action>
            {
                {new recKeyChord(70, false, true), FProc},
            };
        }


        private void FProc()
        {
            Console.WriteLine("You presed the SHIFT F key.");
        }

        internal void KeyHandler(int keyValue, bool shift, bool control)
        {
            Action method;

            if(KeyActions.TryGetValue(new recKeyChord(keyValue, control, shift), out method))
            {
                method.Invoke();
            }

        }
    }
    
}
