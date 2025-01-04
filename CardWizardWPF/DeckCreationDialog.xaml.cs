using System.Windows;

namespace CardWizardWPF
{
    public partial class DeckCreationDialog : Window
    {
        public string DeckName { get; private set; }
        public double DeckWidth { get; private set; }
        public double DeckHeight { get; private set; }

        public DeckCreationDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DeckName = DeckNameTextBox.Text;

            if (Dimension1RadioButton.IsChecked == true)
            {
                //SelectedDimension = "6.35 x 8.89 cm";
                DeckWidth = 6.35;
                DeckHeight = 8.83;

            }
            else if (Dimension2RadioButton.IsChecked == true)
            {
                //SelectedDimension = "8.6 x 5.9 cm";
                DeckWidth = 8.6;
                DeckHeight = 5.9;
            }
            this.DialogResult = true;
            this.Close();
        }
    }
}
