using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CardWizardWPF
{
    public partial class HandWindow : Window
    {
        private List<Image> handCards;
        public delegate void CardActionHandler(Image card, string action);
        public event CardActionHandler CardActionRequested;

        public HandWindow(List<Image> hand)
        {
            InitializeComponent();
            handCards = hand;
            PopulateHand();
        }

        public void UpdateHand()
        {
            PopulateHand(); // Refreshes the hand UI
        }

        private void HandleCardMove(Image card, string action)
        {
            if (handCards.Contains(card))
            {
                handCards.Remove(card); // Remove from hand
                CardActionRequested?.Invoke(card, action); // Invoke action
                UpdateHand(); // Refresh the UI
            }
        }


        private void PopulateHand()
        {
            HandPanel.Children.Clear();

            foreach (var card in new List<Image>(handCards)) // Create a copy to prevent modification issues
            {
                Image cardCopy = new Image
                {
                    Source = card.Source,
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
