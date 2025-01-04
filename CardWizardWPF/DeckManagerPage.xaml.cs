using System;
using System.Collections.Generic;
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

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for DeckManagerPage.xaml
    /// </summary>
    public partial class DeckManagerPage : Page
    {
        public Deck deck;
        public DeckManagerPage(Deck deck)
        {
            this.deck = deck;
            InitializeComponent();
            Deckname.Content = this.deck.Deckname;
        }

        private void Manager_Back_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new DefaultState(), null, deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate back.", "Error");
            }
        }

        private void Manager_New_Card_Button_Click(object sender, RoutedEventArgs e)
        {
            Card card = new Card();
            
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new CardCreatorState(), card, this.deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate to card creator.", "Error");
            }
        }
    }
}
