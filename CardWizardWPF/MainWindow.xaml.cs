using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace CardWizardWPF
{
    public partial class MainWindow : Window
    {
        private IUserState _currentState;
        private const string DecksFolder = "CardWizardDecks";
        public MainWindow()
        {
            _currentState = new DefaultState();
            InitializeComponent();
            this.TransitionTo(_currentState, null, null);
        }

        public void TransitionTo(IUserState state, Card card, Deck deck)
        {
            Deck? deck1 = null;
            Card? card1 = null;

            _currentState = state;
            _currentState.SetContext(this);

            if(deck != null)
            {
                deck1 = deck;
            }
            if(card != null)
            { 
                card1 = card; 
            }
            _currentState.Navigate(deck1, card1);
        }


        public void SetContent(Page newContent)
        {
            ContentArea.Navigate(newContent);
        }

    }
}
