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
    /// Interaction logic for DeckTesterPage.xaml
    /// </summary>
    public partial class DeckTesterPage : Page
    {
        public Deck deck;
        public DeckTesterPage(Deck deck)
        {
            this.deck = deck;
            InitializeComponent();
        }


        private void Tester_Back_Button_Click(object sender, RoutedEventArgs e)
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
    }
}
