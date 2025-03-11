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
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for DeckTesterPage.xaml
    /// </summary>
    public class SearchableImage
    {
        public Image image;
        public string Name;
        public List<string> CardAttributes { get; set; }

        public SearchableImage(Image image, string name, List<string> attributes)
        {
            this.image = image;
            Name = name;

            CardAttributes = new List<string>();
            foreach(string attribute in attributes)
            {
                CardAttributes.Add(attribute);
            }
        }
    }
    public partial class DeckTesterPage : Page
    {
        public Deck deck;
        public List<SearchableImage> testerdeck;
        private bool isImageDragging = false;
        private double imageOffsetX, imageOffsetY;
        private SearchDialog searchDialog;
        private Dictionary<string, CardZoneWindow> cardZones;
        public DeckTesterPage(Deck deck)
        {
            this.deck = deck;
            testerdeck = new List<SearchableImage>();
            cardZones = new Dictionary<string, CardZoneWindow> ();
            deck.Load_Card_images();
            deck.Load_Attributes();
            InitializeComponent();
            InitializeCardZones();
            deckzone.PreviewMouseRightButtonDown += Element_RightMouseDown;
            deckzone.PreviewMouseLeftButtonDown += testerdeck_LeftMouseDown;
            Load_tester_deck();
        }
        private void InitCardZone(string zonename)
        {
            Button Button = new Button
            {
                Content = zonename,
                Margin = new Thickness(5),
                Width = 100
            };
            Button.Click += (sender, e) => OpenCardZoneWindow(zonename);
            CommandBar.Children.Add(Button);

        }
        private void UpdateZones()
        {
            foreach(CardZoneWindow zone in cardZones.Values)
            {
                zone.OtherZones = cardZones.Keys.ToList();
                zone.UpdateCards(zone.Cards);
            }
        }
        private void InitializeCardZones()
        {
            InitCardZone("Hand");
            InitCardZone("Discard");
            UpdateZones();
        }
        private void OpenCardZoneWindow(string zoneName)
        {
            // Check if the window for the zone is already open
            if (!cardZones.ContainsKey(zoneName))
            {
                CardZoneWindow cardZoneWindow = new CardZoneWindow(zoneName);
                cardZoneWindow.CardActionRequested += HandleCardAction;
                cardZoneWindow.ZoneCloseRequested += ZoneCloseAction;

                // Optionally, populate the window with cards from a source (e.g., Hand, Discard)
                List<SearchableImage> zoneCards = GetCardsForZone(zoneName);
                cardZoneWindow.Cards = zoneCards;
                cardZoneWindow.Width = 400;  // Set desired width
                cardZoneWindow.Height = 300; // Set desired height
                // Store the window so you can track it
                cardZones[zoneName] = cardZoneWindow;
                cardZoneWindow.Title = zoneName;
                // Open the window
                cardZoneWindow.Show();
            }
            else
            {
                // If the window is already open, bring it to the front
                cardZones[zoneName].Activate();
            }
            UpdateZones();
        }
        private List<SearchableImage> GetCardsForZone(string zoneName)
        {
            // Placeholder logic to return cards for each zone
            return new List<SearchableImage>();
        }
        private void HandleCardAction(SearchableImage card, string action)
        {
            switch (action)
            {
                case "TopDeck":
                    foreach (var cardZoneEntry in cardZones)
                    {
                        // Check if the card exists in this zone's cards
                        if (cardZoneEntry.Value.Cards.Contains(card))
                        {
                            // Remove the card from the zone
                            cardZoneEntry.Value.Cards.Remove(card);

                            // Move the card to the top of the deck
                            Move_card_to_deck(card, "top");

                            break; // Exit the loop once the card is found and removed
                        }
                    }
                    break;
                case "BottomDeck":
                    foreach (var cardZoneEntry in cardZones)
                    {
                        // Check if the card exists in this zone's cards
                        if (cardZoneEntry.Value.Cards.Contains(card))
                        {
                            // Remove the card from the zone
                            cardZoneEntry.Value.Cards.Remove(card);

                            // Move the card to the top of the deck
                            Move_card_to_deck(card, "bottom");

                            break; // Exit the loop once the card is found and removed
                        }
                    }
                    break;
                case "Field":
                    foreach (var cardZoneEntry in cardZones)
                    {
                        // Check if the card exists in this zone's cards
                        if (cardZoneEntry.Value.Cards.Contains(card))
                        {
                            // Remove the card from the zone
                            LoadCardtofield(card);
                            cardZoneEntry.Value.Cards.Remove(card);
                            break;
                        }
                    }
                    break;
                default:
                    foreach (var cardZoneEntry in cardZones)
                    {
                        // Check if the card exists in this zone's cards
                        if (cardZoneEntry.Value.Cards.Contains(card))
                        {
                            cardZoneEntry.Value.Cards.Remove(card);
                            break;
                        }
                    }
                    cardZones[action].Cards.Add(card);
                    break;
            }
            UpdateZones();
        }
        private void LoadCardtofield(SearchableImage card)
        {
            Image newImage = new Image
            {
                Source = card.image.Source, // Copy the card's image
                Width = card.image.Width > 0 ? card.image.Width : 100,   // Default width
                Height = card.image.Height > 0 ? card.image.Height : 150 // Default height
            };
            newImage.Tag = card;
            // Add mouse event handlers
            newImage.MouseLeftButtonDown += Element_LeftMouseDown;
            newImage.MouseRightButtonDown += Element_RightMouseDown;
            newImage.MouseMove += Element_MouseMoved;
            newImage.MouseUp += Element_MouseUp;

            // Get mouse position relative to the Canvas
            Point mousePos = Mouse.GetPosition(field);

            // Set the image at the cursor position with an offset
            Canvas.SetLeft(newImage, 200);  // Fixed X position
            Canvas.SetTop(newImage, 300);

            // Add the image to the Canvas (field)
            field.Children.Add(newImage);
        }
        private void ZoneCloseAction(string zonename)
        {
            cardZones.Remove(zonename);
            UpdateZones();
        }
        private void Load_tester_deck()
        {
            foreach(Card card in this.deck.Cards)
            {
                for(int i = 0; i < card.AmountInDeck; i++)
                {
                    SearchableImage deckcard = new SearchableImage(card.Image, card.Name, card.Attributes);
                    testerdeck.Add(deckcard);
                }
            }
            ShuffleTesterDeck();
        }
        private void ShuffleTesterDeck()
        {
            Random random = new Random();
            int n = testerdeck.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (testerdeck[i], testerdeck[j]) = (testerdeck[j], testerdeck[i]); // Swap elements
            }
        }
        private void Scoop()
        {
            List<SearchableImage> cardsToMove = field.Children
                .OfType<Image>()
                .Select(img =>
                {
                    // Grab the Tag property of the image (it should contain the SearchableImage)
                    var searchableImage = img.Tag as SearchableImage;
                    return searchableImage != null ? searchableImage : testerdeck.FirstOrDefault(si => si.image == img);
                })
                .Where(si => si != null)
                .ToList();

            foreach (SearchableImage card in cardsToMove)
            {
                Move_card_to_deck(card, "top");
            }
            foreach (var zoneentry in cardZones)
            {
                foreach (var card in new List<SearchableImage>(zoneentry.Value.Cards))
                {
                    Move_card_to_deck(card, "top");
                    zoneentry.Value.Cards.Remove(card);
                }
            }
            UpdateZones();
            ShuffleTesterDeck();
        }
        private void Draw()
        {
            if (testerdeck.Count == 0)
            {
                return;
            }
            if (cardZones.ContainsKey("Hand"))
            {
                Move_card_generic(testerdeck.First(), cardZones["Hand"].Cards);
                cardZones["Hand"].UpdateCards(cardZones["Hand"].Cards);
                testerdeck.Remove(testerdeck.First());
            }
        }
        private void DrawX(int x)
        {
            for(int i = 0;i < x;i++)
            {
                Draw();
            }

        }
        private void Mill()
        {
            if (testerdeck.Count == 0)
            {
                return;
            }
            if (cardZones.ContainsKey("Discard"))
            {
                Move_card_generic(testerdeck.First(), cardZones["Discard"].Cards);
                cardZones["Discard"].UpdateCards(cardZones["Discard"].Cards);
                testerdeck.Remove(testerdeck.First());
            }
        }
        private void MillX(int x)
        {
            for(int j = 0;j < x;j++)
            {
                Mill();
            }
        }
        public void Move_card_to_deck(SearchableImage card, string position)
        {  
            switch (position)
            {
                case "bottom":
                    testerdeck.Add(card);
                    break;
                case "top":
                    testerdeck.Insert(0, card);
                    break;
            }
        }
        public void Move_card_generic(SearchableImage card, List<SearchableImage> location)
        {         
            location.Add(card);
        }
        public List<SearchableImage> SearchDeckFilterName(string name)
        {
            return testerdeck.FindAll(card => card.Name.Contains(name));
        }
        public List<SearchableImage> SearchDeckFilterAttribute(List<string> attributes)
        {
            return testerdeck.FindAll(card => card.CardAttributes.Any(attr => attributes.Contains(attr)));
        }
        //****************************************************************************
        //
        //   MOUSE PRESSED/MOVED/RELEASED HANDLERS
        //
        //****************************************************************************
        private void TapImage(Image image)
        {
            if (image == null) return;

            // Create or retrieve the RotateTransform
            RotateTransform rotateTransform = image.RenderTransform as RotateTransform;

            if (rotateTransform == null)
            {
                // If no rotation is applied, create a new one centered on the image
                rotateTransform = new RotateTransform(0, image.Width / 2, image.Height / 2);
                image.RenderTransform = rotateTransform;
            }

            // If the image is at 360 degrees or any multiple of it, reset the rotation to 0
            if (rotateTransform.Angle >= 360)
            {
                rotateTransform.Angle = 0;
            }
            else
            {
                // Otherwise, increase the angle by 90 degrees (clockwise)
                rotateTransform.Angle += 90;
            }
        }
        private void SearcherHandleCardAction(SearchableImage card, string action)
        {
            bool shuffle = false;
            // Move card based on the action
            switch (action)
            {
                case "TopDeck":
                    testerdeck.Remove(card);
                    Move_card_to_deck(card, "top"); // Assume Deck is a List<Image>
                    break;
                case "BottomDeck":
                    testerdeck.Remove(card);
                    Move_card_to_deck(card, "bottom"); // Assume Deck is a List<Image>
                    break;
                case "Field":
                    testerdeck.Remove(card);
                    shuffle = true;
                    Image newImage = new Image
                    {
                        Source = card.image.Source, // Copy the card's image
                        Width = card.image.Width > 0 ? card.image.Width : 100,   // Default width
                        Height = card.image.Height > 0 ? card.image.Height : 150 // Default height
                    };
                    newImage.Tag = card;
                    // Add mouse event handlers
                    newImage.MouseLeftButtonDown += Element_LeftMouseDown;
                    newImage.MouseRightButtonDown += Element_RightMouseDown;
                    newImage.MouseMove += Element_MouseMoved;
                    newImage.MouseUp += Element_MouseUp;

                    // Get mouse position relative to the Canvas
                    Point mousePos = Mouse.GetPosition(field);

                    // Set the image at the cursor position with an offset
                    Canvas.SetLeft(newImage, 200);  // Fixed X position
                    Canvas.SetTop(newImage, 300);

                    // Add the image to the Canvas (field)
                    field.Children.Add(newImage);
                    break;
            }
            if(shuffle)
            {
                ShuffleTesterDeck();
            }
        }
        private void Tester_Back_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var zone in cardZones.Values.ToList())
            {
                zone.Close();
            }
            if (searchDialog != null)
                searchDialog.Close();

                if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new DefaultState(), null, deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate back.", "Error");
            }
        }
        private void Element_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isImageDragging)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                TapImage(sender as Image);
                return; // Stop further processing for drag if it's a double-click
            }

            if (sender is UIElement element && field != null)
            {

                isImageDragging = true;

                // Capture the mouse to ensure it continues to receive mouse events even if the pointer moves outside the element
                element.CaptureMouse();

                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(field);
                imageOffsetX = pointerPosition.X - Canvas.GetLeft(element);
                imageOffsetY = pointerPosition.Y - Canvas.GetTop(element);
            }
        }
        private void Element_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                // Create the context menu
                ContextMenu rightClickMenu = new ContextMenu();

                // Check the type of the element
                if (element is Image image)
                {
                    SearchableImage searchableImage = image.Tag as SearchableImage;

                    if (searchableImage != null)
                    {
                        // Add options specific to an Image
                        MenuItem bottomOption = new MenuItem { Header = "Put at Bottom of Deck" };
                        bottomOption.Click += (s, args) =>
                        {
                            Move_card_to_deck(searchableImage, "bottom");
                            field.Children.Remove(image);
                        };
                        MenuItem topOption = new MenuItem { Header = "Put at Top of Deck" };
                        topOption.Click += (s, args) =>
                        {
                            Move_card_to_deck(searchableImage, "top");
                            field.Children.Remove(image);
                        };

                        rightClickMenu.Items.Add(bottomOption);
                        rightClickMenu.Items.Add(topOption);
                        foreach(var zone in cardZones.Keys.ToList())
                        {
                            string buttoncontent = "Move to " + zone;
                            MenuItem zoneOption = new MenuItem { Header = buttoncontent };
                            zoneOption.Click += (s, args) =>
                            {
                                Move_card_generic(searchableImage, cardZones[zone].Cards);
                                field.Children.Remove(image);
                                UpdateZones();
                            };
                            rightClickMenu.Items.Add(zoneOption);

                        }
                    }
                }
                else if(element is Button button && button.Name == "deckzone")
                {
                    MenuItem shuffleOption = new MenuItem { Header = "Shuffle" };
                    shuffleOption.Click += (s, args) => ShuffleTesterDeck();
                    MenuItem scoopOption = new MenuItem { Header = "Scoop" };
                    scoopOption.Click += (s, args) => Scoop();
                    MenuItem drawOption = new MenuItem { Header = "Draw 1" };
                    drawOption.Click += (s, args) => Draw();
                    MenuItem drawXOption = new MenuItem { Header = "Draw X..." };
                    drawXOption.Click += (s, args) =>
                    {
                        // Show input dialog to enter a number
                        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter number of cards to draw:", "Draw X", "1");
                        if (int.TryParse(input, out int numberOfCards) && numberOfCards > 0)
                        {
                            DrawX(numberOfCards);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };
                    MenuItem millOption = new MenuItem { Header = "Mill 1" };
                    millOption.Click += (s, args) => Mill();
                    MenuItem millXOption = new MenuItem { Header = "Mill X..." };
                    millXOption.Click += (s, args) =>
                    {
                        // Show input dialog to enter a number
                        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter number of cards to mill:", "Mill X", "1");
                        if (int.TryParse(input, out int numberOfCards) && numberOfCards > 0)
                        {
                            MillX(numberOfCards);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };
                    MenuItem searchOption = new MenuItem { Header = "Search" };
                    searchOption.Click += (s, args) =>
                    {
                        searchDialog = new SearchDialog(this);
                        searchDialog.CardActionRequested += SearcherHandleCardAction;
                        searchDialog.Show();

                    };

                    rightClickMenu.Items.Add(drawOption);
                    rightClickMenu.Items.Add(drawXOption);
                    rightClickMenu.Items.Add(millOption);
                    rightClickMenu.Items.Add(millXOption);
                    rightClickMenu.Items.Add(shuffleOption);
                    rightClickMenu.Items.Add(scoopOption);
                    rightClickMenu.Items.Add(searchOption);
                }

                // Show the context menu at the mouse position
                rightClickMenu.PlacementTarget = element;
                rightClickMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                rightClickMenu.IsOpen = true;

                // Prevent further propagation of the event
                e.Handled = true;
            }
        }
        private void Element_MouseMoved(object sender, MouseEventArgs e)
        {
            if (isImageDragging && sender is UIElement element && field != null)
            {
                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(field);

                double newX = pointerPosition.X - imageOffsetX;
                double newY = pointerPosition.Y - imageOffsetY;

                // Ensure the image stays within the bounds of the canvas (optional)
                //newX = Math.Max(0, Math.Min(newX, cardcanvas.ActualWidth - image.ActualWidth));
                //newY = Math.Max(0, Math.Min(newY, cardcanvas.ActualHeight - image.ActualHeight));

                // Update the position of the image
                Canvas.SetLeft(element, newX);
                Canvas.SetTop(element, newY);
            }
        }
        private void Element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                isImageDragging = false;
                element.ReleaseMouseCapture();
            }
        }
        private void testerdeck_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (testerdeck.Count == 0)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the first image from the deck
                SearchableImage card = testerdeck[0];
                testerdeck.RemoveAt(0); // Remove it from the deck

                // Create a new Image element to place on the Canvas
                Image newImage = new Image
                {
                    Source = card.image.Source, // Copy the image source
                    Width = card.image.Width > 0 ? card.image.Width : 100,   // Default width
                    Height = card.image.Height > 0 ? card.image.Height : 150 // Default height
                };

                // Store the SearchableImage in the Tag property
                newImage.Tag = card;

                newImage.MouseLeftButtonDown += Element_LeftMouseDown;
                newImage.MouseRightButtonDown += Element_RightMouseDown;
                newImage.MouseMove += Element_MouseMoved;
                newImage.MouseUp += Element_MouseUp;

                // Get mouse position relative to the Canvas
                Point mousePosition = e.GetPosition(field);

                // Set the position of the image on the Canvas
                Canvas.SetLeft(newImage, mousePosition.X - 10);
                Canvas.SetTop(newImage, mousePosition.Y - 30);

                // Add the image to the Canvas
                field.Children.Add(newImage);
            }
        }


    }
}
