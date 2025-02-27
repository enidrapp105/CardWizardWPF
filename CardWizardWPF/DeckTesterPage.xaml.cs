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

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for DeckTesterPage.xaml
    /// </summary>
    public partial class DeckTesterPage : Page
    {
        public Deck deck;
        public List<Image> testerdeck;
        public List<Card> discard;
        private bool isImageDragging = false;
        private double imageOffsetX, imageOffsetY;

        public DeckTesterPage(Deck deck)
        {
            this.deck = deck;
            testerdeck = new List<Image>();
            deck.Load_Card_images();
            

            InitializeComponent();
            Load_tester_deck();
        }

        private void Load_tester_deck()
        {
            foreach(Card card in this.deck.Cards)
            {
                for(int i = 0; i < card.AmountInDeck + 1; i++)
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
        //****************************************************************************
        //
        //   MOUSE PRESSED/MOVED/RELEASED HANDLERS
        //
        //****************************************************************************
        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element && field != null)
            {
                // Check if the right mouse button is pressed
                if (e.ChangedButton == MouseButton.Right)
                {
                    // Right-click detected, show context menu
                    return;
                }

                isImageDragging = true;

                // Capture the mouse to ensure it continues to receive mouse events even if the pointer moves outside the element
                element.CaptureMouse();

                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(field);
                imageOffsetX = pointerPosition.X - Canvas.GetLeft(element);
                imageOffsetY = pointerPosition.Y - Canvas.GetTop(element);
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
        private void testerdeck_mouseddown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
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
                newImage.MouseDown += Element_MouseDown;
                newImage.MouseMove += Element_MouseMoved;
                newImage.MouseUp += Element_MouseUp;
                // Get mouse position relative to the Canvas
                Point mousePosition = e.GetPosition(field);

                // Set the position of the image on the Canvas
                Canvas.SetLeft(newImage, mousePosition.X);
                Canvas.SetTop(newImage, mousePosition.Y);

                // Add the image to the Canvas
                field.Children.Add(newImage);
            }
        }
    }
}
