using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CardWizardWPF
{
    public class Card
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FolderPath { get; set; }
        public int AmountInDeck { get; set; }
        public Image Image { get; set; }
        public List<UIElement> Canvaselements { get; set; }
        public List<String> Attributes { get; set; }

    }
}
