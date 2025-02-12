using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Xml;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

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

            Utils util = new Utils();

            // Initialize the Canvas
            cardcanvas = new Canvas
            {
                Name = "cardcanvas",
                Height = util.CmToDeviceIndependentUnits(this.deck.CardHeight),
                Width = util.CmToDeviceIndependentUnits(this.deck.CardWidth),
                Background = new SolidColorBrush(Colors.White),
            };

            // Initialize points based on cardcanvas size
            InitializePoints(cardcanvas.Width, cardcanvas.Height);



            canvasholder.Children.Add(cardcanvas);

            commandBar.Children.Add(CreateTextButton());
            commandBar.Children.Add(CreateImageButton());
            commandBar.Children.Add(CreateTemplateDropdown());

            CheckAndReconstructCanvas();
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
        private ComboBox CreateTemplateDropdown()
        {
            string templatesdirectory = Path.Combine(deck.FolderPath, "templates");
            List<string> subDirectories = new List<string>();
            ComboBox templatebutton = new ComboBox
            {
                Text = "Templates"
            };

            if (Directory.Exists(templatesdirectory))
            {
                subDirectories.AddRange(Directory.GetDirectories(templatesdirectory));

                // Convert full paths to just directory names
                for (int i = 0; i < subDirectories.Count; i++)
                {
                    subDirectories[i] = Path.GetFileName(subDirectories[i]);
                }

               
            }
            foreach (string subdir in subDirectories)
            {
                ComboBoxItem template = new ComboBoxItem
                {
                    Content = subdir,
                };
                
                templatebutton.Items.Add(template);
            }
            templatebutton.SelectionChanged += Templatebutton_SelectionChanged;
            return templatebutton;
        }

        private void Templatebutton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Button existingButton = commandBar.Children
            .OfType<Button>()
            .FirstOrDefault(b => b.Name == "templateButton");

                if (existingButton == null)
                {
                    Button button = new Button
                    {
                        Name = "templateButton",
                        Content = "Apply template",
                        Tag = comboBox
                    };
                    button.Click += TemplateButton_Click;
                    commandBar.Children.Add(button);
                }

                


            }
        }
        private void TemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedTemplate = selectedItem.Content.ToString();
                    string templatePath = Path.Combine(deck.FolderPath, $"templates/{selectedTemplate}");
                    string templateInfoPath = Path.Combine(templatePath, "templateinfo.json");
                    string assetsPath = Path.Combine(templatePath, "assets");

                    try
                    {
                        

                        // Check if JSON file exists
                        if (!File.Exists(templateInfoPath))
                        {
                            return; // No saved data to load
                        }

                        // Read JSON content
                        string jsonText = File.ReadAllText(templateInfoPath);

                        // Deserialize JSON into a list of CanvasItem objects
                        var canvasItems = JsonSerializer.Deserialize<List<CanvasItem>>(jsonText);

                        if (canvasItems == null || canvasItems.Count == 0)
                        {
                            return; // No valid data to load
                        }

                        // Clear the existing canvas
                        cardcanvas.Children.Clear();

                        // Iterate over the canvas items and reconstruct them
                        foreach (var item in canvasItems)
                        {
                            if (item.Type == "Image")
                            {
                                string imagePath = Path.Combine(templatePath, item.Source);
                                if (File.Exists(imagePath))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensures file is not locked
                                    bitmap.EndInit();

                                    Image image = new Image
                                    {
                                        Source = bitmap,
                                        Width = item.Width,
                                        Height = item.Height
                                    };
                                    image.MouseDown += Element_MouseDown;
                                    image.MouseUp += Element_MouseUp;
                                    image.MouseMove += Element_MouseMoved;
                                    image.MouseRightButtonDown += Element_MouseRightButtonDown;
                                    Canvas.SetLeft(image, item.PositionX);
                                    Canvas.SetTop(image, item.PositionY);

                                    cardcanvas.Children.Add(image);
                                }
                            }
                            else if (item.Type == "Text")
                            {
                                TextBlock textBlock = new TextBlock
                                {
                                    Text = item.Content,
                                    FontSize = item.FontSize,
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Color))
                                };
                                textBlock.MouseDown += Element_MouseDown;
                                textBlock.MouseUp += Element_MouseUp;
                                textBlock.MouseMove += Element_MouseMoved;
                                textBlock.MouseRightButtonDown += Element_MouseRightButtonDown;
                                Canvas.SetLeft(textBlock, item.PositionX);
                                Canvas.SetTop(textBlock, item.PositionY);

                                cardcanvas.Children.Add(textBlock);
                            }
                            else if (item.Type == "Rectangle")
                            {
                                Rectangle rectangle = new Rectangle
                                {
                                    Width = item.Width,
                                    Height = item.Height,
                                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Color)),
                                    StrokeThickness = item.StrokeWidth
                                };
                                Canvas.SetLeft(rectangle, item.PositionX);
                                Canvas.SetTop(rectangle, item.PositionY);
                                cardcanvas.Children.Add(rectangle);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reconstructing canvas: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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
                //// Calculate the scale factors
                //double canvasWidth = cardcanvas.ActualWidth;
                //double canvasHeight = cardcanvas.ActualHeight;
                //double imageWidth = bitmapImage.PixelWidth;
                //double imageHeight = bitmapImage.PixelHeight;

                //// Scale image to fit inside the canvas while maintaining its aspect ratio
                //double widthRatio = canvasWidth / imageWidth;
                //double heightRatio = canvasHeight / imageHeight;
                //double scaleRatio = Math.Min(widthRatio, heightRatio);

                //// Set the scaled width and height for the image
                //image.Width = imageWidth * scaleRatio;
                //image.Height = imageHeight * scaleRatio;

                // Set initial position (optional)
                Canvas.SetLeft(image, 0);  // Center horizontally
                Canvas.SetTop(image, 0); // Center vertically

                image.MouseDown += Element_MouseDown;
                image.MouseMove += Element_MouseMoved;
                image.MouseUp += Element_MouseUp;
                image.MouseRightButtonDown += Element_MouseRightButtonDown;
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
                    textBlock.MouseRightButtonDown += Element_MouseRightButtonDown;
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
                string filenamewithimagepath = "image\\thumbnail.png";
                string filepath = Path.Combine(card.FolderPath, filenamewithimagepath);
                //combine file name with card.FolderPath
                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
                save_assets_to_file();
                MessageBox.Show($"Image saved as {filepath}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void save_assets_to_file()
        {
            try
            {
                string cardFolderPath = card.FolderPath;
                string assetsFolderPath = Path.Combine(cardFolderPath, "assets");
                string jsonFilePath = Path.Combine(cardFolderPath, "assets.json");

                // Ensure the assets folder exists
                if (!Directory.Exists(assetsFolderPath))
                {
                    Directory.CreateDirectory(assetsFolderPath);
                }

                List<Dictionary<string, object>> elementsData = new List<Dictionary<string, object>>();

                foreach (UIElement element in cardcanvas.Children)
                {
                    if (element is Image image)
                    {
                        BitmapImage bitmap = image.Source as BitmapImage;
                        if (bitmap?.UriSource != null)
                        {
                            string imageSource = bitmap.UriSource.LocalPath;
                            string imageFileName = Path.GetFileName(imageSource);
                            string newImagePath = Path.Combine(assetsFolderPath, imageFileName);

                            // Copy image to assets folder if it doesn't already exist
                            if (!File.Exists(newImagePath))
                            {
                                File.Copy(imageSource, newImagePath, true);
                            }

                            elementsData.Add(new Dictionary<string, object>
                            {
                                { "Type", "Image" },
                                { "Source", $"assets/{imageFileName}" },
                                { "PositionX", Canvas.GetLeft(image) },
                                { "PositionY", Canvas.GetTop(image) },
                                { "Width", image.ActualWidth },
                                { "Height", image.ActualHeight }
                            });
                        }
                    }
                    else if (element is TextBlock textBlock)
                    {
                        elementsData.Add(new Dictionary<string, object>
                        {
                            { "Type", "Text" },
                            { "Content", textBlock.Text },
                            { "PositionX", Canvas.GetLeft(textBlock) },
                            { "PositionY", Canvas.GetTop(textBlock) },
                            { "FontSize", textBlock.FontSize },
                            { "Color", textBlock.Foreground.ToString() }
                        });
                    }
                }

                // Wrap everything in an object with the "IsPopulated" flag
                var saveData = new Dictionary<string, object>
                {
                    { "IsPopulated", elementsData.Count > 0 }, // Set flag if elements exist
                    { "Elements", elementsData }
                };

                // Serialize data to JSON and write to file
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(elementsData, jsonOptions);
                File.WriteAllText(jsonFilePath, json);

                MessageBox.Show("Card assets saved successfully!", "Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving card assets: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void reconstruct_canvas_from_file()
        {
            try
            {
                string cardFolderPath = card.FolderPath;
                string jsonFilePath = Path.Combine(cardFolderPath, "assets.json");

                // Check if JSON file exists
                if (!File.Exists(jsonFilePath))
                {
                    return; // No saved data to load
                }

                // Read JSON content
                string jsonText = File.ReadAllText(jsonFilePath);

                // Deserialize JSON into a list of CanvasItem objects
                var canvasItems = JsonSerializer.Deserialize<List<CanvasItem>>(jsonText);

                if (canvasItems == null || canvasItems.Count == 0)
                {
                    return; // No valid data to load
                }

                // Clear the existing canvas
                cardcanvas.Children.Clear();

                // Iterate over the canvas items and reconstruct them
                foreach (var item in canvasItems)
                {
                    if (item.Type == "Image")
                    {
                        string imagePath = Path.Combine(cardFolderPath, item.Source);
                        if (File.Exists(imagePath))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensures file is not locked
                            bitmap.EndInit();

                            Image image = new Image
                            {
                                Source = bitmap,
                                Width = item.Width,
                                Height = item.Height
                            };
                            image.MouseDown += Element_MouseDown;
                            image.MouseUp += Element_MouseUp;
                            image.MouseMove += Element_MouseMoved;
                            image.MouseRightButtonDown += Element_MouseRightButtonDown;
                            Canvas.SetLeft(image, item.PositionX);
                            Canvas.SetTop(image, item.PositionY);
                            
                            cardcanvas.Children.Add(image);
                        }
                    }
                    else if (item.Type == "Text")
                    {
                        TextBlock textBlock = new TextBlock
                        {
                            Text = item.Content,
                            FontSize = item.FontSize,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Color))
                        };
                        textBlock.MouseDown += Element_MouseDown;
                        textBlock.MouseUp += Element_MouseUp;
                        textBlock.MouseMove += Element_MouseMoved;
                        textBlock.MouseRightButtonDown += Element_MouseRightButtonDown;
                        Canvas.SetLeft(textBlock, item.PositionX);
                        Canvas.SetTop(textBlock, item.PositionY);

                        cardcanvas.Children.Add(textBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reconstructing canvas: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class CanvasItem
        {
            public string Type { get; set; }
            public string Source { get; set; }
            public double PositionX { get; set; }
            public double PositionY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }

            // For "Text" items
            public string Content { get; set; }
            public int FontSize { get; set; }
            public string Color { get; set; }
            public int StrokeWidth { get; set; }
        }
        
        private void CheckAndReconstructCanvas()
        {
            string jsonFilePath = Path.Combine(card.FolderPath, "assets.json");

            if (File.Exists(jsonFilePath))
            {
                try
                {
                    string jsonText = File.ReadAllText(jsonFilePath);

                    // Deserialize JSON into a list of CanvasItem objects
                    var canvasItems = JsonSerializer.Deserialize<List<CanvasItem>>(jsonText);

                    if (canvasItems != null)
                    {
                        
                        // Call your method to reconstruct the canvas
                        reconstruct_canvas_from_file(); // Load saved data
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading card data: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
        private void Element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                // Create the context menu
                ContextMenu rightClickMenu = new ContextMenu();

                // Check the type of the element
                if (element is Image)
                {
                    // Add options specific to an Image
                    MenuItem resizeOption = new MenuItem { Header = "Resize" };
                    resizeOption.Click += (s, args) => ResizeImage(element as Image);

                    MenuItem rotateOption = new MenuItem { Header = "Rotate" };
                    rotateOption.Click += (s, args) => RotateElement(element as Image);

                    MenuItem removeOption = new MenuItem { Header = "Remove" };
                    removeOption.Click += (s, args) => RemoveElement(element);

                    rightClickMenu.Items.Add(resizeOption);
                    rightClickMenu.Items.Add(rotateOption);
                    rightClickMenu.Items.Add(removeOption);
                }
                else if (element is TextBlock textBlock)
                {
                    // Add options specific to a TextBlock
                    MenuItem fontOption = new MenuItem { Header = "Change Font" };
                    fontOption.Click += (s, args) => ChangeFont(textBlock);

                    // TextBox inside the context menu
                    TextBox textChange = new TextBox
                    {
                        Width = 100,
                        Text = textBlock.Text
                    };

                    textChange.TextChanged += (s, args) =>
                    {
                        textBlock.Text = textChange.Text; // Update text block in real-time
                    };

                    MenuItem textChangeItem = new MenuItem();
                    textChangeItem.Header = textChange;

                    // Create the ColorPicker for changing text color
                    ColorPicker colorSlider = new ColorPicker
                    {
                        Width = 50,
                        Height = 50,
                        SelectedColor = ((SolidColorBrush)textBlock.Foreground).Color
                    };

                    colorSlider.SelectedColorChanged += (s, args) =>
                    {
                        if (colorSlider.SelectedColor.HasValue)
                        {
                            textBlock.Foreground = new SolidColorBrush(colorSlider.SelectedColor.Value);
                        }
                    };

                    // Remove option
                    MenuItem removeOption = new MenuItem { Header = "Remove" };
                    removeOption.Click += (s, args) => RemoveElement(textBlock);

                    // Add items to the context menu
                    rightClickMenu.Items.Add(fontOption);
                    rightClickMenu.Items.Add(textChangeItem); // Add the text change option
                    rightClickMenu.Items.Add(colorSlider);
                    rightClickMenu.Items.Add(removeOption);
                }

                // Show the context menu at the mouse position
                rightClickMenu.PlacementTarget = element;
                rightClickMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                rightClickMenu.IsOpen = true;

                // Prevent further propagation of the event
                e.Handled = true;
            }
        }



        private void ResizeImage(Image image)
        {
            if (image != null)
            {
                // Example: Open a dialog or resize directly
                image.Width *= 1.2; // Increase width by 20%
                image.Height *= 1.2; // Increase height by 20%
            }
        }
        private void RotateElement(UIElement element)
        {
            if (element != null)
            {
                RotateTransform rotateTransform = element.RenderTransform as RotateTransform ?? new RotateTransform();
                rotateTransform.Angle += 45; // Rotate by 45 degrees
                element.RenderTransform = rotateTransform;
            }
        }

        private void RemoveElement(UIElement element)
        {
            if (cardcanvas != null && element != null)
            {
                cardcanvas.Children.Remove(element);
            }
        }


        private void ChangeFont(TextBlock textBlock)
        {
            if (textBlock != null)
            {
                textBlock.FontSize = 20; // Example: Change font size
                textBlock.FontWeight = FontWeights.Bold; // Example: Change font weight
            }
        }

        
        

    }
}
