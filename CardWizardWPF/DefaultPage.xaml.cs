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
    /// Interaction logic for DefaultPage.xaml
    /// </summary>
    public partial class DefaultPage : Page
    {
        private const string DecksFolder = "CardWizardDecks";
        private Deck? selected_deck;
        public DefaultPage()
        {
            selected_deck = null;
            InitializeComponent();
            LoadDeckButtons();

        }

        private void LoadDeckButtons()
        {
            string parentDirectory = Path.Combine(Directory.GetCurrentDirectory(), DecksFolder);
            DeckButtonPanel.Children.Clear();
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
                return;
            }

            foreach (var deckDirectory in Directory.GetDirectories(parentDirectory))
            {
                string configPath = Path.Combine(deckDirectory, "config.txt");
                if (File.Exists(configPath))
                {
                    string deckName = Path.GetFileName(deckDirectory);
                    int cardCount = 0;

                    // Attempt to parse card count from config file
                    var configLines = File.ReadAllLines(configPath);
                    var cardCountLine = configLines.FirstOrDefault(line => line.StartsWith("Card Count:"));
                    if (cardCountLine != null && int.TryParse(cardCountLine.Split(':')[1].Trim(), out int count))
                    {
                        cardCount = count;
                    }

                    // Create a button for this deck
                    var button = new Button
                    {
                        Content = $"{deckName} ({cardCount} cards)",
                        Width = 300,
                        Margin = new Thickness(5),
                        Tag = deckDirectory // Store directory for later use
                    };
                    button.Click += DeckButton_Click;

                    DeckButtonPanel.Children.Add(button);
                }
            }
        }

        private void DeckButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderPath)
            {
                string configPath = Path.Combine(folderPath, "config.txt");

                // Ensure the config file exists
                if (!File.Exists(configPath))
                {
                    MessageBox.Show($"Config file not found in {folderPath}", "Error");
                    return;
                }

                // Read the config file to populate the selected_deck
                var configLines = File.ReadAllLines(configPath);
                var deckNameLine = configLines.FirstOrDefault(line => line.StartsWith("Deck Name:"));
                var cardWidthLine = configLines.FirstOrDefault(line => line.StartsWith("Card Width:"));
                var cardHeightLine = configLines.FirstOrDefault(line => line.StartsWith("Card Height:"));

                if (deckNameLine == null || cardWidthLine == null || cardHeightLine == null)
                {
                    MessageBox.Show("Invalid or missing configuration details.", "Error");
                    return;
                }

                selected_deck = new Deck
                {
                    Deckname = deckNameLine.Split(':')[1].Trim(),
                    CardWidth = double.TryParse(cardWidthLine.Split(':')[1].Trim(), out double width) ? width : 0,
                    CardHeight = double.TryParse(cardHeightLine.Split(':')[1].Trim(), out double height) ? height : 0,
                    FolderPath = folderPath,
                    Cards = new List<Card>()
                };

                // Populate the card list by checking for card files in the "cards" folder
                string cardsFolder = Path.Combine(folderPath, "cards");
                if (Directory.Exists(cardsFolder))
                {
                    foreach (var cardFile in Directory.GetFiles(cardsFolder, "*.txt"))
                    {
                        string cardName = Path.GetFileNameWithoutExtension(cardFile);
                        selected_deck.Cards.Add(new Card { Name = cardName });
                    }
                }

                selected_deck.CardCount = selected_deck.Cards.Count;

                MessageBox.Show($"Deck '{selected_deck.Deckname}' loaded with {selected_deck.CardCount} cards.", "Deck Loaded");
            }
        }

        private void Create_Deck_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeckCreationDialog();
            if (dialog.ShowDialog() == true)
            {
                string deckName = dialog.DeckName;
                double deckWidth = dialog.DeckWidth;
                double deckHeight = dialog.DeckHeight;

                if (!string.IsNullOrEmpty(deckName) && deckHeight != 0 && deckWidth != 0)
                {
                    // Define the parent directory for all decks
                    string parentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "CardWizardDecks");

                    // Ensure the parent directory exists
                    Directory.CreateDirectory(parentDirectory);

                    // Create a deck object
                    var deck = new Deck
                    {
                        Deckname = deckName,
                        CardWidth = deckWidth,
                        CardHeight = deckHeight,
                        FolderPath = Path.Combine(parentDirectory, deckName),
                        Cards = new List<Card>()
                    };

                    // Create the folder for the deck
                    Directory.CreateDirectory(deck.FolderPath);

                    // Create subdirectories for cards and rules
                    string cardsDirectory = Path.Combine(deck.FolderPath, "cards");
                    string rulesDirectory = Path.Combine(deck.FolderPath, "rules");
                    Directory.CreateDirectory(cardsDirectory);
                    Directory.CreateDirectory(rulesDirectory);

                    // Write a config file
                    string configFilePath = Path.Combine(deck.FolderPath, "config.txt");
                    File.WriteAllText(configFilePath,
                        $"Deck Name: {deck.Deckname}\nCard Width: {deck.CardWidth}\nCard Height: {deck.CardHeight}");

                    // Notify the user
                    MessageBox.Show(
                        $"Deck created successfully!\n" +
                        $"Folder Path: {deck.FolderPath}\n" +
                        $"Subdirectories: {cardsDirectory}, {rulesDirectory}\n" +
                        $"Config File: {configFilePath}",
                        "Deck Created");
                    selected_deck = deck;
                    LoadDeckButtons();
                }
                else
                {
                    MessageBox.Show("Invalid dimensions or missing deck name.", "Error");
                }
            }
            else
            {
                MessageBox.Show("Please enter a deck name and select a card dimension.", "Error");
            }
        }


        private void Test_Deck_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow && selected_deck != null)
            {
                mainWindow.TransitionTo(new DeckTesterState(), null, selected_deck);
            }
            else if (selected_deck == null)
            {
                MessageBox.Show("Please select a deck.", "Error");
            }
        }

        private void Edit_Deck_Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (Application.Current.MainWindow is MainWindow mainWindow && selected_deck != null)
            {
                mainWindow.TransitionTo(new DeckManagerState(), null, selected_deck);
            }
            else if (selected_deck == null)
            {
                MessageBox.Show("Please select a deck.", "Error");
            }
        }
    }
}
