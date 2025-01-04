using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardWizardWPF
{
    public interface IUserState
    {
        void SetContext(MainWindow mainWindow);
        //some states use the deck some states use the card
        //if it doesn't use it, pass null
        void Navigate(Deck deck, Card card);
    }
}
