using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CardWizardWPF
{
    public partial class SearchDialog : Window
    {

        private DeckTesterPage _deckTesterPage; // Store reference
        private List<SearchableImage> SearchResults;

        public delegate void CardActionHandler(SearchableImage card, string action);
        public event CardActionHandler CardActionRequested;

        public SearchDialog(DeckTesterPage deckTesterPage)
        {
            InitializeComponent();
            _deckTesterPage = deckTesterPage; // Assign instance
            
            SearchResults = new List<SearchableImage>();
            MessageBox.Show($"Deck Attributes Count: {_deckTesterPage.deck.Attributes?.Count ?? 0}");
            LoadAttributeButtons();
        }
        private void HandleCardMove(SearchableImage card, string action)
        {
            if (SearchResults.Contains(card))
            {
                SearchResults.Remove(card); // Remove from hand
                CardActionRequested?.Invoke(card, action); // Invoke action
                PopulateSearchResults(); // Refresh the UI
            }
        }
        private void OnAttrSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            lstAttributes.Visibility = Visibility.Visible;
        }
        private void OnAttrSearchButton_Unchecked(object sender, RoutedEventArgs e)
        {
            lstAttributes.Visibility = Visibility.Collapsed;
        }
        private void LoadAttributeButtons()
        {
            // Ensure the deck and its Attributes list are not null
            if (_deckTesterPage?.deck == null || _deckTesterPage.deck.Attributes == null)
            {
                MessageBox.Show("Deck or Attributes list is not initialized.");
                return;
            }

            // Populate ListBox with attributes
            foreach (string attribute in _deckTesterPage.deck.Attributes)
            {
                ListBoxItem attrButton = new ListBoxItem
                {
                    Content = attribute
                };
                lstAttributes.Items.Add(attrButton);
            }
        }
        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            wrapPanel.Children.Clear();
            SearchResults.Clear();

            if (rbName.IsChecked == true)
            {
                string searchText = txtName.Text;
                SearchResults = _deckTesterPage.SearchDeckFilterName(searchText); // Use instance
            }
            else if (rbAttribute.IsChecked == true)
            {
                List<string> selectedAttributes = new List<string>();
                foreach (ListBoxItem item in lstAttributes.SelectedItems)
                {
                    selectedAttributes.Add(item.Content.ToString());
                }
                SearchResults = _deckTesterPage.SearchDeckFilterAttribute(selectedAttributes); // Use instance
            }

            PopulateSearchResults();
        }

        private void PopulateSearchResults()
        {
            wrapPanel.Children.Clear();

            foreach (var card in new List<SearchableImage>(SearchResults))
            {
                Image cardImage = new Image
                {
                    Source = card.image.Source,
                    Width = 100,
                    Height = 150,
                    Tag = card,
                    Margin = new Thickness(5)
                };

                // Create context menu
                ContextMenu contextMenu = new ContextMenu();

                MenuItem moveToDiscard = new MenuItem { Header = "Move to Discard" };
                moveToDiscard.Click += (s, e) => HandleCardMove(card, "Discard");

                MenuItem moveToHand = new MenuItem { Header = "Move to Hand" };
                moveToHand.Click += (s, e) => HandleCardMove(card, "Hand");

                MenuItem moveToField = new MenuItem { Header = "Move to Field" };
                moveToField.Click += (s, e) => HandleCardMove(card, "Field");

                contextMenu.Items.Add(moveToDiscard);
                contextMenu.Items.Add(moveToHand);
                contextMenu.Items.Add(moveToField);

                cardImage.ContextMenu = contextMenu;
                wrapPanel.Children.Add(cardImage);
            }
        }
    }

}
