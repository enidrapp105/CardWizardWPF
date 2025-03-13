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

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for RulesSelectionDialog.xaml
    /// </summary>
    public partial class RulesSelectionDialog : Window
    {
        public string SelectedOption { get; private set; } = "Rules Card"; // Default to Rules Card

        public RulesSelectionDialog()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (RulesPageRadio.IsChecked == true)
            {
                SelectedOption = "Rules Page";
            }
            else
            {
                SelectedOption = "Rules Card";
            }

            DialogResult = true; // Close dialog and return result
        }
    }

}
