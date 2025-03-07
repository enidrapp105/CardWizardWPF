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
                        Margin = new Thickness(5),
                        Padding = new Thickness(10)
                    };
                    cardButton.Click += CardButton_Click;
                    Button plusButton = new Button
                    {
                        Content = $"{card.AmountInDeck}+",
                        Tag = card,
                        Padding = new Thickness(5),
                        Margin = new Thickness(10)
                    };
                    plusButton.Click += CardPlusButton_Click;
                    // Create the delete button for the card
                    Button deleteButton = new Button
                    {
                        Content = "Delete",
                        Tag = card, // Store the card object in the Tag property
                        Margin = new Thickness(5),
                        Padding = new Thickness(10)
                    };
                    deleteButton.Click += Manager_Delete_Card_Click;

                    // Add buttons to the stack panel
                    
                    cardButtonPanel.Children.Add(cardButton);
                    cardButtonPanel.Children.Add(plusButton);
                    cardButtonPanel.Children.Add(deleteButton);

                    // Add the stack panel to the parent container
                    CardButtonsPanel.Children.Add(cardButtonPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading card buttons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                double margin = 10; // Reduced margin to minimize space wastage
                double spacing = 0;  // Minimal spacing between images
                double maxWidthPerImage = 180;  // Maximum width per image
                double maxHeightPerImage = 180; // Maximum height per image

                double pageWidth = XUnit.FromPoint(595).Point - (2 * margin); // A4 width minus margins
                double pageHeight = XUnit.FromPoint(842).Point - (2 * margin); // A4 height minus margins

                double x = margin;
                double y = margin;
                double maxRowHeight = 0;
                int imagesOnPage = 0;

                for (int i = 0; i < imagePaths.Count; i++)
                {
                    if (imagesOnPage == 0 || page == null)
                    {
                        // Create a new page if needed
                        page = pdfDocument.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        x = margin;
                        y = margin;
                        maxRowHeight = 0;
                        imagesOnPage = 0;
                    }

                    XImage img = XImage.FromFile(imagePaths[i]);

                    // Maintain aspect ratio
                    double aspectRatio = (double)img.PixelWidth / img.PixelHeight;
                    double drawWidth = maxWidthPerImage;
                    double drawHeight = maxHeightPerImage;

                    if (aspectRatio > 1) // Landscape
                    {
                        drawHeight = maxWidthPerImage / aspectRatio;
                    }
                    else // Portrait
                    {
                        drawWidth = maxHeightPerImage * aspectRatio;
                    }

                    // Move to the next row if the image doesn't fit
                    if (x + drawWidth > pageWidth)
                    {
                        x = margin;
                        y += maxRowHeight + spacing;
                        maxRowHeight = 0;
                    }

                    if (y + drawHeight > pageHeight)
                    {
                        // Create a new page and retry placing the image
                        page = pdfDocument.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        x = margin;
                        y = margin;
                        maxRowHeight = 0;
                        imagesOnPage = 0;
                    }

                    // Draw the image
                    gfx.DrawImage(img, x, y, drawWidth, drawHeight);

                    // Update positioning
                    x += drawWidth + spacing;
                    maxRowHeight = Math.Max(maxRowHeight, drawHeight);
                    imagesOnPage++;

                    // Move to a new page if needed
                    if (y + maxRowHeight > pageHeight)
                    {
                        page = null; // Forces a new page in the next loop
                    }
                }

                pdfDocument.Save(pdfPath);
                pdfDocument.Close();

                MessageBox.Show($"PDF saved successfully at:\n{pdfPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating PDF: " + ex.Message);
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
