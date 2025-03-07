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
using System.Windows.Shapes;

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for DiscardWindow.xaml
    /// </summary>
    public partial class DiscardWindow : Window
    {
        
        private List<SearchableImage> disCards;
        public delegate void CardActionHandler(SearchableImage card, string action);
        public event CardActionHandler CardActionRequested;

        public DiscardWindow(List<SearchableImage> discard)
        {
            InitializeComponent();
            disCards = discard;
            PopulateDiscard();
        }

        public void UpdateDiscard(List<SearchableImage> cards)
        {
            disCards = cards;
            PopulateDiscard(); // Refreshes the hand UI
        }

        private void HandleCardMove(SearchableImage card, string action)
        {
            if (disCards.Contains(card))
            {
                disCards.Remove(card); // Remove from hand
                CardActionRequested?.Invoke(card, action); // Invoke action
                UpdateDiscard(disCards); // Refresh the UI
            }
        }


        private void PopulateDiscard()
        {
            DiscardPanel.Children.Clear();

            foreach (var card in new List<SearchableImage>(disCards)) // Create a copy to prevent modification issues
            {
                Image cardCopy = new Image
                {
                    Source = card.image.Source,
                    Width = 100,
                    Height = 150,
                    Margin = new Thickness(5)
                };

                // Create Context Menu
                ContextMenu contextMenu = new ContextMenu();

                MenuItem moveToTop = new MenuItem { Header = "Move to Top of Deck" };
                moveToTop.Click += (s, e) => HandleCardMove(card, "TopDeck");

                MenuItem moveToBottom = new MenuItem { Header = "Move to Bottom of Deck" };
                moveToBottom.Click += (s, e) => HandleCardMove(card, "BottomDeck");

                MenuItem discard = new MenuItem { Header = "Move to Hand" };
                discard.Click += (s, e) => HandleCardMove(card, "Hand");

                MenuItem moveToField = new MenuItem { Header = "Move to Field" };
                moveToField.Click += (s, e) => HandleCardMove(card, "Field");

                contextMenu.Items.Add(moveToTop);
                contextMenu.Items.Add(moveToBottom);
                contextMenu.Items.Add(discard);
                contextMenu.Items.Add(moveToField);

                cardCopy.ContextMenu = contextMenu;
                DiscardPanel.Children.Add(cardCopy);
            }
        }
    }
}
