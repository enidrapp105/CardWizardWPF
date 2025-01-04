using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Add_Card(Card card)
        {
            Cards.Add(card);
        }

        public void Delete_Card(Card card)
        {
            foreach(Card card2 in Cards)
            {
                if(card2 == card)
                {
                    Cards.Remove(card2);
                }
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
