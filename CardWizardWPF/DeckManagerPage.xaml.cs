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
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Path = System.IO.Path;
using System.Reflection.Metadata;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Text.Json;


namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for DeckManagerPage.xaml
    /// </summary>
    public partial class DeckManagerPage : Page
    {
        public Deck deck;
        public Card selectedcard;
        public DeckManagerPage(Deck deck)
        {
            this.deck = deck;
            InitializeComponent();
            Deckname.Content = this.deck.Deckname;
            Manager_Load_Card_Buttons();
            Manager_Load_Rule_Buttons();
        }
        private void Manager_Load_Rule_Buttons()
        {
            string folderPath = Path.Combine(deck.FolderPath, "rules");
            RuleButtonsPanel.Children.Clear();

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Rule folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> ruleDirectories = new List<string>(Directory.GetDirectories(folderPath));

            foreach (string ruleDir in ruleDirectories)
            {
                string ruleName = Path.GetFileName(ruleDir);
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };
                Button ruleButton = new Button
                {
                    Content = ruleName,
                    Tag = ruleDir,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Width = 100
                };
                Button deleteButton = new Button
                {
                    Content = "-",
                    Tag = ruleDir,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),

                };

                ruleButton.Click += RuleButton_Click;
                deleteButton.Click += RuleDeleteButton_Click;
                stackPanel.Children.Add(ruleButton);
                stackPanel.Children.Add(deleteButton);
                RuleButtonsPanel.Children.Add(stackPanel);
            }
        }
        public class RuleInfoItem
        {
            public string Type { get; set; }
            public string Source { get; set; }
            public double PositionX { get; set; }
            public double PositionY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
        private void RuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is string)
            {
                string cardFolderPath = clickedButton.Tag.ToString();
                string jsonFilePath = Path.Combine(cardFolderPath, "ruleinfo.json");
                double width = 0;
                double height = 0;

                if (!File.Exists(jsonFilePath))
                {
                    return; // No saved data to load
                }

                string jsonText = File.ReadAllText(jsonFilePath);

                List<RuleInfoItem> ruleItems = JsonSerializer.Deserialize<List<RuleInfoItem>>(jsonText);

                if (ruleItems != null && ruleItems.Count > 0)
                {
                    width = ruleItems[0].Width;
                    height = ruleItems[0].Height;
                }
                Card ruleobject = new Card
                {
                    isRuleInitialized = true,
                    FolderPath = cardFolderPath,
                    ruleWidth = width,
                    ruleHeight = height,
                };
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.TransitionTo(new RuleObjectCreatorState(), ruleobject, this.deck);
                }
                else
                {
                    MessageBox.Show("Unable to navigate to rule creator.", "Error");
                }
            }
        }
        private void RuleDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is string folderPath)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the rule folder:\n{folderPath}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                    );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete(folderPath, true); // true ensures deletion of non-empty folders
                        MessageBox.Show("Rule folder deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Optionally, refresh the UI after deletion
                        Manager_Load_Rule_Buttons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void Manager_Load_Card_Buttons()
        {
            try
            {
                // Construct the card folder path
                string folderPath = Path.Combine(deck.FolderPath, "cards");

                // Clear existing buttons in the UI
                CardButtonsPanel.Children.Clear();

                // Ensure the card folder exists
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show("Card folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Loop through the cards in the deck
                foreach (var card in deck.Cards)
                {
                    // Create a stack panel for each card
                    StackPanel cardButtonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(5)
                    };

                    // Create the main button for the card
                    Button cardButton = new Button
                    {
                        Content = card.Name,
                        Tag = card, // Store the card object in the Tag property
                        Margin = new Thickness(0),
                        Padding = new Thickness(10),
                    };
                    cardButton.Click += CardButton_Click;
                    Button plusButton = new Button
                    {
                        Content = $"{card.AmountInDeck}+",
                        Tag = card,
                        Margin = new Thickness(0),
                        Padding = new Thickness(10)

                    };
                    plusButton.Click += CardPlusButton_Click;
                    // Create the delete button for the card
                    Button deleteButton = new Button
                    {
                        Content = "Delete",
                        Tag = card, // Store the card object in the Tag property
                        Margin = new Thickness(0),
                        Padding = new Thickness(10)
                    };
                    deleteButton.Click += Manager_Delete_Card_Click;

                    Button minusButton = new Button
                    {
                        Content = "-",
                        Tag = card,
                        Margin = new Thickness(0),
                        Padding = new Thickness(10)
                    };
                    minusButton.Click += CardMinusButton_Click;

                    Button moreButton = new Button
                    {
                        Content = "...",
                        Tag = card,
                        Margin = new Thickness(0),
                        Padding = new Thickness(10)
                    };
                    moreButton.ContextMenu = CardMoreButton_ContextMenu(card);
                    // Add buttons to the stack panel

                    cardButtonPanel.Children.Add(cardButton);
                    cardButtonPanel.Children.Add(minusButton);
                    cardButtonPanel.Children.Add(plusButton);
                    cardButtonPanel.Children.Add(deleteButton);
                    cardButtonPanel.Children.Add(moreButton);

                    // Add the stack panel to the parent container
                    CardButtonsPanel.Children.Add(cardButtonPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading card buttons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void addToFeaturedcards(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is Card card)
            {
                string thumbnailpath = Path.Combine(card.FolderPath, "image/thumbnail.png");
                var newCard = new Card_Model
                {
                    ImagePath = thumbnailpath,
                    CardName = card.Name,
                    AddedDate = DateTime.Today
                };

                var cards = DatabaseManager.GetCardsCollection();
                var result = cards.Insert(newCard);
                if (result == null)
                {
                    MessageBox.Show("Failed to insert card into database.");
                }
                else
                {
                    MessageBox.Show("Card inserted successfully!");
                }
            }
        }
        private void CardPlusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Card card)
            {
                card.AmountInDeck += 1;
                card.SaveAttributetoFile(card.AmountInDeck, "AmountInDeck");
                Manager_Load_Card_Buttons();
            }

        }
        private ContextMenu CardMoreButton_ContextMenu(Card card)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem DailyCardOption = new MenuItem
            {
                Header = "Submit to Featured cards",
                Tag = card
            };
            DailyCardOption.Click += addToFeaturedcards;
            menu.Items.Add(DailyCardOption);
            return menu;

        }
        private void CardMinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Card card)
            {
                if(card.AmountInDeck > 0)
                {
                    card.AmountInDeck -= 1;
                    card.SaveAttributetoFile(card.AmountInDeck, "AmountInDeck");
                    Manager_Load_Card_Buttons();
                }
            }

        }
        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Card card)
            {
                //MessageBox.Show($"You selected the card: {card.Name}", "Card Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                string cardName = button.Content.ToString(); // Extract card name from button
                string cardPath = Path.Combine(deck.FolderPath, "cards", cardName);
                Card selectedCard = new Card { FolderPath = cardPath };
                selectedCard.LoadFromFile();
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.TransitionTo(new CardCreatorState(), selectedCard, this.deck);
                }
                else
                {
                    MessageBox.Show("Unable to navigate to card creator.", "Error");
                }
            }
        }
        private void TemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new TemplateCreatorState(), null, this.deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate to template creator.", "Error");
            }
        }
        private void Manager_Delete_Card_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is Card card)
                {
                    // Confirm deletion
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete the card '{card.Name}'?",
                        "Confirm Deletion",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Call the Delete_Card method
                        Delete_Card(card);

                        // Reload the card buttons after deletion
                        Manager_Load_Card_Buttons();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the card: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Delete_Card(Card card)
        {
            deck.Delete_Card(card);
            
            deck.Load_Deck_From_File();
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
                string assetsFolderPath = Path.Combine(cardFolderPath, "assets");
                Directory.CreateDirectory(assetsFolderPath);
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
        private void Manager_Create_PDF_Button_Click(object sender, RoutedEventArgs e)
        {
            string pdfPath = GetSaveFilePath();
            if (string.IsNullOrEmpty(pdfPath))
            {
                MessageBox.Show("No file selected. PDF generation canceled.");
                return;
            }

            List<string> imagePaths = new List<string>();

            // Collect valid image paths
            foreach (var card in deck.Cards)
            {
                for (int i = 0; i < card.AmountInDeck; i++)
                {
                    string imagePath = Path.Combine(card.FolderPath, "image", "thumbnail.png");
                    if (File.Exists(imagePath))
                    {
                        imagePaths.Add(imagePath);
                    }
                }
            }

            if (imagePaths.Count == 0)
            {
                MessageBox.Show("No valid images found. PDF generation canceled.");
                return;
            }

            try
            {
                PdfDocument pdfDocument = new PdfDocument();
                PdfPage page = null;
                XGraphics gfx = null;

                // === Sizes from deck in CM ===
                double cardWidthCm = this.deck.CardWidth;
                double cardHeightCm = this.deck.CardHeight;
                double spacingCm = 0.0; // tightly packed
                double printerSafeMarginCm = 0.5; // Add 1cm margins for printers

                // === Convert cm to PDF points ===
                double CmToPoints(double cm) => cm * (72.0 / 2.54);
                double cardWidth = CmToPoints(cardWidthCm);
                double cardHeight = CmToPoints(cardHeightCm);
                double spacing = CmToPoints(spacingCm);
                double safeMargin = CmToPoints(printerSafeMarginCm);

                double pageWidth = XUnit.FromMillimeter(210).Point;
                double pageHeight = XUnit.FromMillimeter(297).Point;

                // Compute how many cards fit WITHIN printable area
                double printableWidth = pageWidth - 2 * safeMargin;
                double printableHeight = pageHeight - 2 * safeMargin;

                // Fit as many as possible horizontally
                int cardsPerRow = (int)((printableWidth + spacing) / cardWidth);
                // Then figure out how many rows we can fit vertically
                int cardsPerColumn = (int)((printableHeight + spacing) / (cardHeight + spacing));
                int cardsPerPage = cardsPerRow * cardsPerColumn;

                // Horizontal layout uses left margin only; vertical layout is centered
                // Now compute grid actual width and height
                double gridWidth = cardsPerRow * cardWidth + (cardsPerRow - 1) * spacing;
                double gridHeight = cardsPerColumn * cardHeight + (cardsPerColumn - 1) * spacing;

                // Start from top-left corner inside safe margins
                double startX = safeMargin;
                double startY = safeMargin;




                for (int i = 0; i < imagePaths.Count; i++)
                {
                    if (i % cardsPerPage == 0)
                    {
                        page = pdfDocument.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                    }

                    int indexOnPage = i % cardsPerPage;
                    int row = indexOnPage / cardsPerRow;
                    int col = indexOnPage % cardsPerRow;

                    double x = startX + col * (cardWidth + spacing);
                    double y = startY + row * (cardHeight + spacing);

                    XImage img = XImage.FromFile(imagePaths[i]);
                    double aspectRatio = (double)img.PixelWidth / img.PixelHeight;

                    double drawWidth = cardWidth;
                    double drawHeight = cardHeight;

                    if (aspectRatio > 1)
                    {
                        drawHeight = cardWidth / aspectRatio;
                        drawWidth = cardWidth;
                    }
                    else
                    {
                        drawWidth = cardHeight * aspectRatio;
                        drawHeight = cardHeight;
                    }

                    double offsetX = (cardWidth - drawWidth) / 2;
                    double offsetY = (cardHeight - drawHeight) / 2;

                    gfx.DrawImage(img, x + offsetX, y + offsetY, drawWidth, drawHeight);
                }

                pdfDocument.Save(pdfPath);
                pdfDocument.Close();

                //MessageBox.Show($"PDF saved successfully at:\n{pdfPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating PDF: " + ex.Message);
            }
        }



        private void Manager_Create_Rules_Button_Click(object sender, RoutedEventArgs e)
        {
            Card ruleobject = new Card();
            ruleobject.isRuleObject = true;
            var dialog = new RulesSelectionDialog();
            if (dialog.ShowDialog() == true)
            {
                if (dialog.SelectedOption == "Rules Card")
                {
                    ruleobject.ruleHeight = deck.CardHeight;
                    ruleobject.ruleWidth = deck.CardWidth;
                }
                else
                {
                    double paperWidthCm = 21.59;  // 21.59 cm
                    double paperHeightCm = 27.94; // 27.94 cm

                    // Apply the scale factor to maintain aspect ratio
                    ruleobject.ruleWidth = paperWidthCm;
                    ruleobject.ruleHeight = paperHeightCm;
                }
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.TransitionTo(new RuleObjectCreatorState(), ruleobject, this.deck);
                }
                else
                {
                    MessageBox.Show("Unable to navigate to rule creator.", "Error");
                }

            }
        }

        private string GetSaveFilePath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "output.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }
    }
}
