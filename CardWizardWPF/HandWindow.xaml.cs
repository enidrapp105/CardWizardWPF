using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CardWizardWPF
{
    public partial class HandWindow : Window
    {
        private List<SearchableImage> handCards;
        public delegate void CardActionHandler(SearchableImage card, string action);
        public event CardActionHandler CardActionRequested;

        public HandWindow(List<SearchableImage> hand)
        {
            InitializeComponent();
            handCards = hand;
            PopulateHand();
        }

        public void UpdateHand(List<SearchableImage> cards)
        {
            handCards = cards;
            PopulateHand(); // Refreshes the hand UI
        }

        private void HandleCardMove(SearchableImage card, string action)
        {
            if (handCards.Contains(card))
            {
                handCards.Remove(card); // Remove from hand
                CardActionRequested?.Invoke(card, action); // Invoke action
                UpdateHand(handCards); // Refresh the UI
            }
        }


        private void PopulateHand()
        {
            HandPanel.Children.Clear();

            foreach (var card in new List<SearchableImage>(handCards)) // Create a copy to prevent modification issues
            {
                Image cardCopy = new Image
                {
                    Source = card.image.Source,
                    Width = 100,
                    Height = 150,
                    Margin = new Thickness(5),
                };

                // Create Context Menu
                ContextMenu contextMenu = new ContextMenu();

                MenuItem moveToTop = new MenuItem { Header = "Move to Top of Deck" };
                moveToTop.Click += (s, e) => HandleCardMove(card, "TopDeck");

                MenuItem moveToBottom = new MenuItem { Header = "Move to Bottom of Deck" };
                moveToBottom.Click += (s, e) => HandleCardMove(card, "BottomDeck");

                MenuItem discard = new MenuItem { Header = "Move to Discard" };
                discard.Click += (s, e) => HandleCardMove(card, "Discard");

                MenuItem moveToField = new MenuItem { Header = "Move to Field" };
                moveToField.Click += (s, e) => HandleCardMove(card, "Field");

                contextMenu.Items.Add(moveToTop);
                contextMenu.Items.Add(moveToBottom);
                contextMenu.Items.Add(discard);
                contextMenu.Items.Add(moveToField);

                cardCopy.ContextMenu = contextMenu;
                HandPanel.Children.Add(cardCopy);
            }
        }


    }
}
