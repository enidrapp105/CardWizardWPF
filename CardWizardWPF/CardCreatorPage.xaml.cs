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
    /// Interaction logic for CardCreatorPage.xaml
    /// </summary>
    public partial class CardCreatorPage : Page
    {
        public Card card;
        public Deck deck;
        public CardCreatorPage(Deck deck, Card card)
        {
            this.card = card;
            this.deck = deck;
            InitializeComponent();
        }

        private void Creator_Back_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new DeckManagerState(), null, deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate back.", "Error");
            }
        }
    }
}
