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
    /// Interaction logic for CardZone.xaml
    /// </summary>
    public partial class CardZoneWindow : Window
    {
        public List<SearchableImage> Cards { get; set; }
        public List<string> OtherZones { get; set; }
        public string ZoneName { get; set; }

        public delegate void CardActionHandler(SearchableImage card, string action);
        public delegate void ZoneCloseActionHandler(string Zonename);
        public event CardActionHandler CardActionRequested;
        public event ZoneCloseActionHandler ZoneCloseRequested;

        public CardZoneWindow(string zoneName)
        {
            InitializeComponent();
            ZoneName = zoneName;
            Cards = new List<SearchableImage>();
            OtherZones = new List<string>();
            this.DataContext = this;
            this.Closed += OnCardZoneWindowClosed;
        }

        public void UpdateCards(List<SearchableImage> cards)
        {
            Cards = cards;
            PopulateCards();
        }

        private void PopulateCards()
        {
            CardsPanel.Children.Clear();

            foreach (var card in Cards)
            {
                Image cardImage = new Image
                {
                    Source = card.image.Source,
                    Width = 100,
                    Height = 150,
                    Margin = new Thickness(5)
                };

                ContextMenu contextMenu = new ContextMenu();

                MenuItem moveToTop = new MenuItem { Header = "Move to Top of Deck" };
                moveToTop.Click += (s, e) =>
                {
                    CardActionRequested?.Invoke(card, "TopDeck");
                    CardsPanel.Children.Remove(cardImage);
                };

                MenuItem moveToBottom = new MenuItem { Header = "Move to Bottom of Deck" };
                moveToBottom.Click += (s, e) =>
                {
                    CardActionRequested?.Invoke(card, "BottomDeck");
                    CardsPanel.Children.Remove(cardImage);
                };

                MenuItem moveToField = new MenuItem { Header = "Move to Field" };
                moveToField.Click += (s, e) =>
                {
                    CardActionRequested?.Invoke(card, "Field");
                    CardsPanel.Children.Remove(cardImage);
                };
                
                contextMenu.Items.Add(moveToTop);
                contextMenu.Items.Add(moveToBottom);
                contextMenu.Items.Add(moveToField);

                foreach (string zonename in OtherZones)
                {
                    string buttoncontent = "Move to " + zonename;
                    MenuItem moveToZone = new MenuItem { Header = buttoncontent };
                    moveToZone.Click += (s, e) =>
                    {
                        CardActionRequested?.Invoke(card, zonename);
                        CardsPanel.Children.Remove(cardImage);
                    };
                    contextMenu.Items.Add(moveToZone);
                }

                cardImage.ContextMenu = contextMenu;

                CardsPanel.Children.Add(cardImage);
            }
        }
        private void OnCardZoneWindowClosed(object sender, EventArgs e)
        {
            foreach (var card in new List<SearchableImage>(Cards))
            {
                CardActionRequested?.Invoke(card, "TopDeck");
            }

            // Trigger the custom event to notify the caller that the zone has been closed
            ZoneCloseRequested?.Invoke(ZoneName);
        }
    }
}
