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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for CardCreationDialog.xaml
    /// </summary>
    public partial class CardCreationDialog : Window
    {
        public Card card;
        public CardCreationDialog()
        {
            InitializeComponent();
        }

        private void Add_Attribute_Button_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            TextBox textbox = new TextBox
            {
                Width = 100
            };
            Button button = new Button
            {
                Content = "-"
            };
            button.Click += (s, ev) => delete_attribute(stackPanel);

            stackPanel.Children.Add(textbox);
            stackPanel.Children.Add(button);
            AttributePanel.Children.Add(stackPanel);
        }
        

        private void delete_attribute(StackPanel stackpanel)
        {
            AttributePanel.Children.Remove(stackpanel);
        }
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            string cardName = DeckNameTextBox.Text;
            string cardDescription = CardDescriptionBox.Text;

            List<string> attributes = new List<string>();
            foreach(var child in AttributePanel.Children)
            {
                if(child is StackPanel panel)
                {
                    foreach(var element in panel.Children)
                    {
                        if (element is TextBox textBox)
                        {
                            attributes.Add(textBox.Text);
                        }
                    }
                    
                }
            }
            card = new Card
            {
                Name = cardName,
                Description = cardDescription,
                Attributes = attributes
            };

            bool emptyattributes = false;
            foreach (string attribute in card.Attributes)
            {
                if (string.IsNullOrEmpty(attribute))
                {
                    emptyattributes = true;
                }
            }
            if (!string.IsNullOrEmpty(card.Name) && !string.IsNullOrEmpty(card.Description) && !emptyattributes)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("All fields must be populated", "Error");
            }
            
        }
    }


}
