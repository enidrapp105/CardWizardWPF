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
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for TemplateCreatorPage.xaml
    /// </summary>
    public partial class TemplateCreatorPage : Page
    {
        Deck deck;
        Canvas templatecanvas;
        Utils util;
        private double imageOffsetX, imageOffsetY;
        private bool isImageDragging = false;

        public TemplateCreatorPage(Deck deck)
        {
            util = new Utils();
            this.deck = deck;
            InitializeComponent();

            templatecanvas = new Canvas
            {
                Name = "cardcanvas",
                Height = util.CmToDeviceIndependentUnits(this.deck.CardHeight),
                Width = util.CmToDeviceIndependentUnits(this.deck.CardWidth),
                Background = new SolidColorBrush(Colors.White),
            };


            commandBar.Children.Add(CreateTextButton());
            commandBar.Children.Add(CreateImageButton());
            commandBar.Children.Add(CreateBorderButton());

            canvasholder.Children.Add(templatecanvas);
        }

        private void Template_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Window nameprompt = new Window
            {
                Title = "Template Name?",
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
                    string folderpath = deck.FolderPath;
                    string templatesfolderpath = Path.Combine(folderpath, "templates");
                    string templatedirectory = Path.Combine(templatesfolderpath, textprompt.Text);
                    string templateassetsfolder = Path.Combine(templatedirectory, "assets");
                    string templateinfo = Path.Combine(templatedirectory, "templateinfo.json");

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
                    List<Dictionary<string, object>> elementsData = new List<Dictionary<string, object>>();

                    foreach (UIElement element in templatecanvas.Children)
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
                                { "Color", textBlock.Foreground.ToString() }
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

        private void Template_Back_Button_Click(object sender, RoutedEventArgs e)
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
                Width = templatecanvas.ActualWidth,
                Height = templatecanvas.ActualHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            templatecanvas.Children.Add(borderrectangle);
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
                templatecanvas.Children.Add(image);
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
                    templatecanvas.Children.Add(textBlock);
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
            if (sender is UIElement element && templatecanvas != null)
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
                Point pointerPosition = e.GetPosition(templatecanvas);
                imageOffsetX = pointerPosition.X - Canvas.GetLeft(element);
                imageOffsetY = pointerPosition.Y - Canvas.GetTop(element);
            }
        }
        private void Element_MouseMoved(object sender, MouseEventArgs e)
        {
            if (isImageDragging && sender is UIElement element && templatecanvas != null)
            {
                // Get the current mouse position relative to the canvas
                Point pointerPosition = e.GetPosition(templatecanvas);

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
            if (templatecanvas != null && element != null)
            {
                templatecanvas.Children.Remove(element);
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
