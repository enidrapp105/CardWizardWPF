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
    public partial class DeckTesterPage : Page
    {
        public Deck deck;
        public List<Image> testerdeck;
        public List<Image> hand;
        public List<Image> discard;
        private bool isImageDragging = false;
        private double imageOffsetX, imageOffsetY;

        public DeckTesterPage(Deck deck)
        {
            this.deck = deck;
            testerdeck = new List<Image>();
            hand = new List<Image> ();
            discard = new List<Image> ();
            deck.Load_Card_images();

            
            InitializeComponent();
            deckzone.PreviewMouseRightButtonDown += Element_RightMouseDown;
            deckzone.PreviewMouseLeftButtonDown += testerdeck_LeftMouseDown;
            Load_tester_deck();
        }

        private void Load_tester_deck()
        {
            foreach(Card card in this.deck.Cards)
            {
                for(int i = 0; i < card.AmountInDeck; i++)
                {
                    testerdeck.Add(card.Image);
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
            List<Image> cardsToMove = field.Children.OfType<Image>().ToList();

            foreach (Image card in cardsToMove)
            {
                Move_card_to_deck(card, "top", null);
            }
            foreach (Image card in discard)
            {
                Move_card_to_deck(card, "top", discard);
            }
            foreach(Image card in hand)
            {
                Move_card_to_deck(card, "top", hand);

            }
            handWindow.UpdateHand();
            discardWindow.UpdateDiscard();
            ShuffleTesterDeck();
        }
        private void Draw()
        {
            if (testerdeck.Count == 0)
            {
                return;
            }
            Move_card_generic(testerdeck.First(), hand, testerdeck);
            handWindow.UpdateHand();
        }
        private void DrawX(int x)
        {
            for(int i = 0;i < x;i++)
            {
                Draw();
            }
            handWindow.UpdateHand();
        }
        
        private void Move_card_to_deck(Image card, string position, List<Image> sender)
        {
            if (sender == null) //canvas case
            {
                field.Children.Remove(card);
            }
            else 
            {
                sender.Remove(card);
            }
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
        private void Move_card_generic(Image card, List<Image> location, List<Image> sender)
        {
            if(sender != null) 
            {
                sender.Remove(card);
                location.Add(card);
            }
        }
        private void Move_Multiple_cards(List<Image> cards, List<Image> location, List<Image> sender)
        {
            foreach (Image card in cards) 
            {
                Move_card_generic(card, location, sender);
            }
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
        private HandWindow handWindow;
        private void Hand_Button_Click(object sender, RoutedEventArgs e) 
        {
            if (handWindow == null || !handWindow.IsLoaded)
            {
                handWindow = new HandWindow(hand);
                handWindow.CardActionRequested += HandHandleCardAction;
                handWindow.Show();
            }
            else
            {
                handWindow.UpdateHand(); // Refresh the window if already open
            }
        }
        private DiscardWindow discardWindow;
        private void Discard_Button_Click(object sender, RoutedEventArgs e)
        {
            if (discardWindow == null || !discardWindow.IsLoaded)
            {
                discardWindow = new DiscardWindow(discard);
                discardWindow.CardActionRequested += DiscardHandleCardAction;
                discardWindow.Show();
            }
            else
            {
                discardWindow.UpdateDiscard(); // Refresh the window if already open
            }
        }
        private void DiscardHandleCardAction(Image card, string action)
        {

            switch (action)
            {
                case "TopDeck":
                    Move_card_to_deck(card, "top", discard); // Assume Deck is a List<Image>
                    break;
                case "BottomDeck":
                    Move_card_to_deck(card, "bottom", discard); // Assume Deck is a List<Image>
                    break;
                case "Hand":
                    Move_card_generic(card, hand, discard); // Assume Deck is a List<Image>
                    break;
                case "Field":
                    Image newImage = new Image
                    {
                        Source = card.Source, // Copy the card's image
                        Width = card.Width > 0 ? card.Width : 100,   // Default width
                        Height = card.Height > 0 ? card.Height : 150 // Default height
                    };

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
            handWindow?.UpdateHand();
        }
        private void HandHandleCardAction(Image card, string action)
        {

            switch (action)
            {
                case "TopDeck":
                    Move_card_to_deck(card, "top", hand); // Assume Deck is a List<Image>
                    break;
                case "BottomDeck":
                    Move_card_to_deck(card, "bottom", hand); // Assume Deck is a List<Image>
                    break;
                case "Discard":
                    Move_card_generic(card, discard, hand); // Assume Deck is a List<Image>
                    break;
                case "Field":
                    Image newImage = new Image
                    {
                        Source = card.Source, // Copy the card's image
                        Width = card.Width > 0 ? card.Width : 100,   // Default width
                        Height = card.Height > 0 ? card.Height : 150 // Default height
                    };

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
            handWindow?.UpdateHand();
        }
        private void Tester_Back_Button_Click(object sender, RoutedEventArgs e)
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

        private DateTime _lastClickTime = DateTime.MinValue;
        private const int DoubleClickThreshold = 500; // 500 ms (adjustable)
        private void Element_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isImageDragging)
            {
                // Reset last click time to prevent double-click detection during dragging
                _lastClickTime = DateTime.MinValue;
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
                if (element is Image)
                {
                    // Add options specific to an Image
                    MenuItem bottomOption = new MenuItem { Header = "Put at Bottom of Deck" };
                    bottomOption.Click += (s, args) => Move_card_to_deck((Image)element, "bottom", null);
                    MenuItem topOption = new MenuItem { Header = "Put at Top of Deck" };
                    topOption.Click += (s, args) => Move_card_to_deck((Image)element, "top", null);


                    rightClickMenu.Items.Add(bottomOption);
                    rightClickMenu.Items.Add(topOption);
                }
                if(element is Button button && button.Name == "deckzone")
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

                    rightClickMenu.Items.Add(drawOption);
                    rightClickMenu.Items.Add(drawXOption);
                    rightClickMenu.Items.Add(shuffleOption);
                    rightClickMenu.Items.Add(scoopOption);
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
                Image cardImage = testerdeck[0];
                testerdeck.RemoveAt(0); // Remove it from the deck

                // Create a new Image element to place on the Canvas
                Image newImage = new Image
                {
                    Source = cardImage.Source, // Copy the image source
                    Width = cardImage.Width > 0 ? cardImage.Width : 100,   // Default width
                    Height = cardImage.Height > 0 ? cardImage.Height : 150 // Default height
                };
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
