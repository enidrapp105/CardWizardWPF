using Microsoft.Win32;
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
        private bool isImageDragging = false;
        private double imageOffsetX, imageOffsetY;
        private Point[] points;

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

            // Initialize points based on cardcanvas size
            InitializePoints(cardcanvas.Width, cardcanvas.Height);

            canvasholder.Children.Add(cardcanvas);
            commandBar.Children.Add(CreateTextButton());
            commandBar.Children.Add(CreateImageButton());
        }

        // Initialize the points array dynamically
        private void InitializePoints(double canvasWidth, double canvasHeight)
        {
            points = new Point[]
            {
                new Point(0, 0),                  // Top-left
                new Point(canvasWidth, 0),       // Top-right
                new Point(0, canvasHeight),      // Bottom-left
                new Point(canvasWidth, canvasHeight) // Bottom-right
            };
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

                image.MouseDown += Element_MouseDown;
                image.MouseMove += Element_MouseMoved;
                image.MouseUp += Element_MouseUp;
                // Add the Image control to the Canvas
                cardcanvas.Children.Add(image);
            }
        }
        //**********************************************************
        // Function name: TextButton_Click
        //
        // Purpose: Handles the click of the add text button
        // 
        // Parameters: standard click params
        //
        // Returns: N/A
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
                    textBlock.MouseDown += Element_MouseDown;
                    textBlock.MouseMove += Element_MouseMoved;
                    textBlock.MouseUp += Element_MouseUp;
                    // Add the TextBlock to the Canvas
                    cardcanvas.Children.Add(textBlock);
                }
                inputWindow.Close();
            };

            // Show the input dialog
            inputWindow.ShowDialog();
        }
        //**********************************************************
        // Function name: Creator_Back_Button_Click
        //
        // Purpose: Handles the click of the back button
        // 
        // Parameters: standard click params
        //
        // Returns: N/A
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
        //**********************************************************
        // Function name: CmToDeviceIndependentUnits
        //
        // Purpose: converts centimeters to device independent units
        // 
        // Parameters: centimeters in float form
        //
        // Returns: device independent units
        private double CmToDeviceIndependentUnits(double cm)
        {
            const double cmPerInch = 2.54;
            const double dpi = 96.0; // WPF uses 96 DPI for device-independent units
            return cm * (dpi / cmPerInch);
        }
        //**********************************************************
        // Function name: Creator_Save_Card_Button_Click
        //
        // Purpose: Handles the click of the save card button
        // 
        // Parameters: standard click params
        //
        // Returns: N/A
        private void Creator_Save_Card_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the size of the canvas
                double canvasWidth = cardcanvas.ActualWidth;
                double canvasHeight = cardcanvas.ActualHeight;

                // Ensure the canvas size is valid
                if (canvasWidth <= 0 || canvasHeight <= 0)
                {
                    MessageBox.Show("Canvas size is invalid. Please ensure it has proper dimensions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a RenderTargetBitmap matching the canvas size
                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)Math.Ceiling(canvasWidth),
                    (int)Math.Ceiling(canvasHeight),
                    96, // DPI X
                    96, // DPI Y
                    PixelFormats.Pbgra32
                );

                //// Render the exact bounds of the canvas
                cardcanvas.Measure(new Size(canvasWidth, canvasHeight)); // this rearanges the canvas's position visually
                cardcanvas.Arrange(new Rect(new Size(canvasWidth, canvasHeight)));
                renderTargetBitmap.Render(cardcanvas);
                canvasholder.Children.Clear(); //clear the holder
                canvasholder.Children.Add(cardcanvas); //readd the canvas to reset it's position
                // Save the bitmap as a PNG file
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                string filePath = "output.png";
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                MessageBox.Show($"Image saved as {filePath}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //****************************************************************************
        //
        //   MOUSE PRESSED/MOVED/RELEASED HANDLERS
        //
        //****************************************************************************
        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element && cardcanvas != null)
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
                Point pointerPosition = e.GetPosition(cardcanvas);
                imageOffsetX = pointerPosition.X - Canvas.GetLeft(element);
                imageOffsetY = pointerPosition.Y - Canvas.GetTop(element);
            }
        }
        private void Element_MouseMoved(object sender, MouseEventArgs e)
        {
            if (isImageDragging && sender is UIElement element && cardcanvas != null)
            {
                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(cardcanvas);

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

    }
}
