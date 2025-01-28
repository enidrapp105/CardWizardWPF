using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardWizardWPF
{
    public class Deck
    {
        public string Deckname { get; set; }
        public double CardHeight { get; set; }
        public double CardWidth { get; set; }
        public string FolderPath { get; set; }

        public int CardCount { get; set; }

        public List<Card> Cards;
        public List<String> Attributes { get; set; }


        public void Add_Card(Card card)
        {
            Cards.Add(card);
        }

        public void Delete_Card(Card card)
        {
            try
            {
                // Ensure the card exists in the collection
                if (!Cards.Contains(card))
                {
                    MessageBox.Show("The card does not exist in the collection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Remove the card from the collection
                Cards.Remove(card);

                // Construct the directory path
                string cardFolder = "cards";
                string folderPath = Path.Combine(this.FolderPath, cardFolder, card.Name);

                // Check if the directory exists
                if (Directory.Exists(folderPath))
                {
                    // Delete the directory and its contents
                    Directory.Delete(folderPath, true);
                    MessageBox.Show($"The card '{card.Name}' and its directory have been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"The directory for '{card.Name}' does not exist.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the card: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public Card Select_Card(string cardname)
        {
            Card card = new Card();
            foreach (Card card2 in Cards)
            {
                if (card2.Name == cardname)
                {
                    card = card2;
                }
            }
            return card;
        }
    }
}
