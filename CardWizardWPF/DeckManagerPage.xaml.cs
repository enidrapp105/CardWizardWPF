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
            var dialog = new CardCreationDialog();
            if (dialog.ShowDialog() == true)
            {
                card = dialog.card;

                // Create a card folder within the deck folder
                string cardsFolderPath = deck.FolderPath + "\\cards";
                string cardFolderPath = Path.Combine(cardsFolderPath, card.Name);
                Directory.CreateDirectory(cardFolderPath);

                // Create an "image" subfolder
                string imageFolderPath = Path.Combine(cardFolderPath, "image");
                Directory.CreateDirectory(imageFolderPath);

                // Update card properties
                card.FolderPath = cardFolderPath;
                card.AmountInDeck = 1; // Set default amount
                card.Attributes = dialog.card.Attributes;

                // Create a JSON file for the card
                string jsonFilePath = Path.Combine(cardFolderPath, "cardinfo.json");
                var cardInfo = new
                {
                    card.Name,
                    card.Description,
                    card.FolderPath,
                    card.AmountInDeck,
                    Attributes = card.Attributes
                };

                // Serialize card information to JSON
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(cardInfo, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(jsonFilePath, jsonContent);

                // Add the card to the deck
                deck.Add_Card(card);

                // Update deck configuration
                UpdateDeckConfiguration(deck);

                MessageBox.Show($"Card '{card.Name}' created successfully in deck '{deck.Deckname}'.", "Card Created");

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

        private void UpdateDeckConfiguration(Deck deck)
        {
            string configPath = Path.Combine(deck.FolderPath, "config.json");

            // Update the deck details
            var deckInfo = new
            {
                Deckname = deck.Deckname,
                CardWidth = deck.CardWidth,
                CardHeight = deck.CardHeight,
                CardCount = deck.Cards.Count,
                Attributes = deck.Attributes,
            };

            string jsonContent = System.Text.Json.JsonSerializer.Serialize(deckInfo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(configPath, jsonContent);
        }
    }
}
