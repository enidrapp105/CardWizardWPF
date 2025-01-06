using Microsoft.Win32;
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
    /// Interaction logic for CardCreatorPage.xaml
    /// </summary>
    public partial class CardCreatorPage : Page
    {
        public Card card;
        public Deck deck;
        Canvas cardcanvas;
        public CardCreatorPage(Deck deck, Card card)
        {
            this.card = card;
            this.deck = deck;
            
            InitializeComponent();
            
            cardcanvas = new Canvas
            {
                Name = "cardcanvas",
                Height = CmToDeviceIndependentUnits(this.deck.CardHeight),
                Width = CmToDeviceIndependentUnits(this.deck.CardWidth),
                Background = new SolidColorBrush(Colors.White),
            };
            canvasholder.Children.Add(cardcanvas);
            commandBar.Children.Add(CreateTextButton());
            commandBar.Children.Add(CreateImageButton());
        }
        //BUTTONS CREATORS *****************************************
        private Button CreateImageButton()
        {
            Button addimagebutton = new Button
            {
                Content = "Add Image",
            };
            addimagebutton.Click += ImageButton_Click;
            return addimagebutton;
        }
        private Button CreateTextButton()
        {
            Button addtextbutton = new Button
            {
                Content = "Add Text",
            };
            addtextbutton.Click += TextButton_Click;
            return addtextbutton;
        }
        //**********************************************************
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an OpenFileDialog to select image files
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select an Image",
                Filter = "Image Files (*.jpeg;*.jpg;*.png)|*.jpeg;*.jpg;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) // Default location
            };

            // Show the dialog and check if a file was selected
            if (openFileDialog.ShowDialog() == true) // Returns true when a file is selected
            {
                string selectedFilePath = openFileDialog.FileName;

                // Load the selected image into a BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(selectedFilePath, UriKind.Absolute);
                bitmapImage.EndInit();

                // Create an Image control to display the selected image
                Image image = new Image
                {
                    Source = bitmapImage,
                    Stretch = Stretch.Uniform
                };

                // Set initial position (optional)
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                // Calculate the scale factors
                double canvasWidth = cardcanvas.ActualWidth;
                double canvasHeight = cardcanvas.ActualHeight;
                double imageWidth = bitmapImage.PixelWidth;
                double imageHeight = bitmapImage.PixelHeight;

                // Scale image to fit inside the canvas while maintaining its aspect ratio
                double widthRatio = canvasWidth / imageWidth;
                double heightRatio = canvasHeight / imageHeight;
                double scaleRatio = Math.Min(widthRatio, heightRatio);

                // Set the scaled width and height for the image
                image.Width = imageWidth * scaleRatio;
                image.Height = imageHeight * scaleRatio;

                // Set initial position (optional)
                Canvas.SetLeft(image, (canvasWidth - image.Width) / 2);  // Center horizontally
                Canvas.SetTop(image, (canvasHeight - image.Height) / 2); // Center vertically

                // Add the Image control to the Canvas
                cardcanvas.Children.Add(image);
            }
        }
        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new window for text input
            Window inputWindow = new Window
            {
                Title = "Enter Text",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // Create a StackPanel for layout
            StackPanel stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Create a TextBox for text input
            TextBox textBox = new TextBox
            {
                AcceptsReturn = true,
                Height = 80,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(textBox);

            // Create an OK button
            Button okButton = new Button
            {
                Content = "OK",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            stackPanel.Children.Add(okButton);

            // Set the content of the input window
            inputWindow.Content = stackPanel;

            // Close the dialog and use the entered text when OK is clicked
            okButton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Create a TextBlock to display the entered text
                    TextBlock textBlock = new TextBlock
                    {
                        Text = textBox.Text,
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                    };

                    // Set initial position
                    Canvas.SetLeft(textBlock, 10); // Example positioning, adjust as needed
                    Canvas.SetTop(textBlock, 10);

                    // Add the TextBlock to the Canvas
                    cardcanvas.Children.Add(textBlock);
                }
                inputWindow.Close();
            };

            // Show the input dialog
            inputWindow.ShowDialog();
        }


        private void Creator_Back_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.TransitionTo(new DeckManagerState(), null, deck);
            }
            else
            {
                MessageBox.Show("Unable to navigate back.", "Error");
            }
        }
        private double CmToDeviceIndependentUnits(double cm)
        {
            const double cmPerInch = 2.54;
            const double dpi = 96.0; // WPF uses 96 DPI for device-independent units
            return cm * (dpi / cmPerInch);
        }
        private void Creator_Save_Card_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
