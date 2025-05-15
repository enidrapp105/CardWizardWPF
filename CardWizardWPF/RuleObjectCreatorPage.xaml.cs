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
using Xceed.Wpf.Toolkit;
using static CardWizardWPF.CardCreatorPage;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using WindowStartupLocation = System.Windows.WindowStartupLocation;


namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for RuleObjectCreatorPage.xaml
    /// </summary>
    public partial class RuleObjectCreatorPage : Page
    {
        Deck deck;
        Card card;
        Canvas rulecanvas;
        Utils util;
        private double imageOffsetX, imageOffsetY;
        private bool isImageDragging = false;

        public RuleObjectCreatorPage(Deck deck, Card card)
        {
            util = new Utils();
            this.deck = deck;
            this.card = card;
            InitializeComponent();

            if (card.isRuleInitialized)
            {
                rulecanvas = new Canvas
                {
                    Name = "cardcanvas",
                    Height = card.ruleHeight,
                    Width = card.ruleWidth,
                    Background = new SolidColorBrush(Colors.White),
                };
            }
            else
            {
                rulecanvas = new Canvas
                {
                    Name = "cardcanvas",
                    Height = util.CmToDeviceIndependentUnits(card.ruleHeight),
                    Width = util.CmToDeviceIndependentUnits(card.ruleWidth),
                    Background = new SolidColorBrush(Colors.White),
                };
            }

            commandBar.Children.Add(CreateTextButton());
            commandBar.Children.Add(CreateImageButton());
            commandBar.Children.Add(CreateBorderButton());

            canvasholder.Children.Add(rulecanvas);
            if(card.isRuleInitialized) 
            {
                reconstruct_canvas_from_file();
            }
        }




        private void Rule_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Window nameprompt = new Window
            {
                Title = "Rule object Name?",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            StackPanel inputpromptpanel = new StackPanel();
            Label label = new Label
            {
                Content = "Name:"
            };
            TextBox textprompt = new TextBox
            {
                Width = 200
            };
            Button submitbutton = new Button
            {
                Content = "Submit",
                Width = 200

            };
            submitbutton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textprompt.Text))
                {
                    string filename = textprompt.Text + ".png";
                    string folderpath = deck.FolderPath;
                    string templatesfolderpath = Path.Combine(folderpath, "rules");
                    string templatedirectory = Path.Combine(templatesfolderpath, textprompt.Text);
                    string templateassetsfolder = Path.Combine(templatedirectory, "assets");
                    string templateinfo = Path.Combine(templatedirectory, "ruleinfo.json");
                    string rulesimagedirectory = Path.Combine(templatedirectory, "image");
                    string thumbnailPath = Path.Combine(rulesimagedirectory, filename);

                    if (!Directory.Exists(templatesfolderpath))
                    {
                        Directory.CreateDirectory(templatesfolderpath);
                    }
                    if (!Directory.Exists(templatedirectory))
                    {
                        Directory.CreateDirectory(templatedirectory);
                    }
                    if (!Directory.Exists(templateassetsfolder))
                    {
                        Directory.CreateDirectory(templateassetsfolder);
                    }
                    if (!Directory.Exists(rulesimagedirectory))
                    {
                        Directory.CreateDirectory(rulesimagedirectory);
                    }

                    // Get the size of the canvas
                    double canvasWidth = rulecanvas.ActualWidth;
                    double canvasHeight = rulecanvas.ActualHeight;

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
                    rulecanvas.Measure(new Size(canvasWidth, canvasHeight)); // this rearanges the canvas's position visually
                    rulecanvas.Arrange(new Rect(new Size(canvasWidth, canvasHeight)));
                    renderTargetBitmap.Render(rulecanvas);
                    canvasholder.Children.Clear(); //clear the holder
                    canvasholder.Children.Add(rulecanvas); //readd the canvas to reset it's position
                    try
                    {                                      // Save the bitmap as a PNG file
                   
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                        using (var fileStream = new FileStream(thumbnailPath, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save thumbnail: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    List<Dictionary<string, object>> elementsData = new List<Dictionary<string, object>>();

                    foreach (UIElement element in rulecanvas.Children)
                    {
                        if (element is Image image)
                        {
                            BitmapImage bitmap = image.Source as BitmapImage;
                            if (bitmap?.UriSource != null)
                            {
                                string imageSource = bitmap.UriSource.LocalPath;
                                string imageFileName = Path.GetFileName(imageSource);
                                string newImagePath = Path.Combine(templateassetsfolder, imageFileName);

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
                                { "Color", textBlock.Foreground.ToString() },
                            });
                        }
                        else if (element is Rectangle rectangle)
                        {
                            elementsData.Add(new Dictionary<string, object>
                            {
                                { "Type", "Rectangle" },
                                { "PositionX", util.GetCanvasPosition(rectangle, Canvas.LeftProperty) },
                                { "PositionY", util.GetCanvasPosition(rectangle, Canvas.RightProperty) },
                                { "Color", rectangle.Stroke.ToString() },
                                { "Width", rectangle.ActualWidth },
                                { "Height", rectangle.ActualHeight },
                                { "StrokeWidth", rectangle.StrokeThickness }
                            });
                        }
                    }
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(elementsData, jsonOptions);
                    File.WriteAllText(templateinfo, json);
                }
                nameprompt.Close();
            };

            inputpromptpanel.Children.Add(label);
            inputpromptpanel.Children.Add(textprompt);
            inputpromptpanel.Children.Add(submitbutton);
            nameprompt.Content = inputpromptpanel;
            // Show the input dialog
            nameprompt.ShowDialog();

        }
        private void reconstruct_canvas_from_file()
        {
            try
            {
                string cardFolderPath = card.FolderPath;
                string jsonFilePath = Path.Combine(cardFolderPath, "ruleinfo.json");

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
                rulecanvas.Children.Clear();

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

                            rulecanvas.Children.Add(image);
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

                        rulecanvas.Children.Add(textBlock);
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
                        rulecanvas.Children.Add(rectangle);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reconstructing canvas: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Rule_Back_Button_Click(object sender, RoutedEventArgs e)
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
        private Button CreateBorderButton()
        {
            Button borderbutton = new Button
            {
                Content = "Border",
            };
            borderbutton.Click += BorderButton_Click;
            return borderbutton;
        }
        private void BorderButton_Click(object sender, RoutedEventArgs e)
        {
            Rectangle borderrectangle = new Rectangle
            {
                Width = rulecanvas.ActualWidth,
                Height = rulecanvas.ActualHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            rulecanvas.Children.Add(borderrectangle);
        }
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
                rulecanvas.Children.Add(image);
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
                    rulecanvas.Children.Add(textBlock);
                }
                inputWindow.Close();
            };

            // Show the input dialog
            inputWindow.ShowDialog();
        }
        //****************************************************************************
        //
        //   MOUSE PRESSED/MOVED/RELEASED HANDLERS
        //
        //****************************************************************************
        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element && rulecanvas != null)
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
                Point pointerPosition = e.GetPosition(rulecanvas);
                imageOffsetX = pointerPosition.X - Canvas.GetLeft(element);
                imageOffsetY = pointerPosition.Y - Canvas.GetTop(element);
            }
        }
        private void Element_MouseMoved(object sender, MouseEventArgs e)
        {
            if (isImageDragging && sender is UIElement element && rulecanvas != null)
            {
                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(rulecanvas);

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
            if (rulecanvas != null && element != null)
            {
                rulecanvas.Children.Remove(element);
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
