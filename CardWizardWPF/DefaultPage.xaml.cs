using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                string configPath = Path.Combine(deckDirectory, "config.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        // Read and deserialize the JSON configuration
                        string jsonContent = File.ReadAllText(configPath);
                        var config = JsonSerializer.Deserialize<Deck>(jsonContent);

                        if (config != null)
                        {
                            string deckName = config.Deckname ?? Path.GetFileName(deckDirectory);
                            int cardCount = config.CardCount;

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
                    catch (JsonException ex)
                    {
                        // Handle JSON parsing errors (log or display an error)
                        Debug.WriteLine($"Error parsing JSON in {configPath}: {ex.Message}");
                    }
                }
            }
        }

        private void DeckButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderPath)
            {
                string configPath = Path.Combine(folderPath, "config.json");

                // Ensure the config file exists
                if (!File.Exists(configPath))
                {
                    MessageBox.Show($"Config file not found in {folderPath}", "Error");
                    return;
                }

                try
                {
                    // Read and deserialize the JSON configuration
                    string jsonContent = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<Deck>(jsonContent);

                    if (config == null)
                    {
                        MessageBox.Show("Invalid or missing configuration details.", "Error");
                        return;
                    }

                    selected_deck = new Deck
                    {
                        Deckname = config.Deckname,
                        CardWidth = config.CardWidth,
                        CardHeight = config.CardHeight,
                        FolderPath = folderPath,
                        Cards = new List<Card>()
                    };

                    // Populate the card list by checking for card files in the "cards" folder
                    string cardsFolder = Path.Combine(folderPath, "cards");
                    if (Directory.Exists(cardsFolder))
                    {
                        foreach (var cardFile in Directory.GetFiles(cardsFolder))
                        {
                            string cardName = Path.GetFileNameWithoutExtension(cardFile);
                            selected_deck.Cards.Add(new Card { Name = cardName });
                        }
                    }

                    selected_deck.CardCount = selected_deck.Cards.Count;

                    MessageBox.Show($"Deck '{selected_deck.Deckname}' loaded with {selected_deck.CardCount} cards.", "Deck Loaded");
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading configuration file: {ex.Message}", "Error");
                }
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
                        Cards = new List<Card>(),
                        Attributes = new List<string>()
                    };

                    Directory.CreateDirectory(deck.FolderPath);
                    Directory.CreateDirectory(Path.Combine(deck.FolderPath, "cards"));
                    Directory.CreateDirectory(Path.Combine(deck.FolderPath, "rules"));

                    // Create JSON config file
                    UpdateDeckConfiguration(deck);

                    MessageBox.Show($"Deck '{deckName}' created successfully!", "Deck Created");
                    LoadDeckButtons();
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
