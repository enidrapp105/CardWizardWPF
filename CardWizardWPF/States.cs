using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CardWizardWPF
{
    public class DefaultState : IUserState
    {
        private MainWindow _mainWindow;

        public void SetContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Navigate(Deck deck, Card card)
        {
            // Replace the entire content of the window
            _mainWindow.SetContent(new DefaultPage());
        }
    }
    public class DeckManagerState : IUserState
    {
        private MainWindow _mainWindow;

        public void SetContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Navigate(Deck deck, Card card)
        {
            // Replace the entire content of the window
            _mainWindow.SetContent(new DeckManagerPage(deck));
        }
    }

    public class CardCreatorState : IUserState
    {
        private MainWindow _mainWindow;

        public void SetContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Navigate(Deck deck, Card card)
        {
            _mainWindow.SetContent(new CardCreatorPage(deck, card));
        }
    }

    public class DeckTesterState : IUserState
    {
        private MainWindow _mainWindow;

        public void SetContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Navigate(Deck deck, Card card)
        {
            // Replace the entire content of the window
            _mainWindow.SetContent(new DeckTesterPage(deck));
        }
    }
    public class TemplateCreatorState : IUserState
    {
        private MainWindow _mainWindow;
        public void SetContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Navigate(Deck deck, Card card)
        {
            // Replace the entire content of the window
            _mainWindow.SetContent(new TemplateCreatorPage(deck));
        }
    }
}
