using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CardWizardWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        SolidColorBrush lilacBrush = (SolidColorBrush)Application.Current.FindResource("LilacBrush");
        SolidColorBrush PastelPinkBrush = (SolidColorBrush)Application.Current.FindResource("PastelPinkBrush");

    }
}
