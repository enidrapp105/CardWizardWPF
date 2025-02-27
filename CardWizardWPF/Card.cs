using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardWizardWPF
{
    public class Card
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FolderPath { get; set; }
        public int AmountInDeck { get; set; }
        [JsonIgnore]
        public Image Image { get; set; }
        public List<String> Attributes { get; set; }

        public void SaveAttributetoFile(int num, string type)
        {
            string jsonFilePath = Path.Combine(FolderPath, "cardinfo.json");

            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("File not found: " + jsonFilePath);
                return;
            }

            try
            {
                // Read existing JSON data
                string json = File.ReadAllText(jsonFilePath);
                Card? card = JsonSerializer.Deserialize<Card>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (card == null)
                {
                    Console.WriteLine("Failed to deserialize JSON.");
                    return;
                }

                // Update the attribute
                switch (type)
                {
                    case "AmountInDeck":
                        card.AmountInDeck = num;
                        break;
                    default:
                        Console.WriteLine("Invalid attribute type.");
                        return;
                }

                // Serialize and write back to file
                string updatedJson = JsonSerializer.Serialize(card, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, updatedJson);
                Console.WriteLine("Card attributes updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating JSON: " + ex.Message);
            }
        }
        public void LoadFromFile()
        {
            try
            {
                if (string.IsNullOrEmpty(FolderPath))
                {
                    MessageBox.Show("Card folder path is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string cardInfoPath = Path.Combine(FolderPath, "cardinfo.json");

                if (!File.Exists(cardInfoPath))
                {
                    MessageBox.Show($"Card info file not found: {cardInfoPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read and deserialize JSON
                string cardContent = File.ReadAllText(cardInfoPath);
                var cardData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cardContent);

                if (cardData != null)
                {
                    Name = cardData.ContainsKey("Name") ? cardData["Name"].GetString() : "Unnamed";
                    Description = cardData.ContainsKey("Description") ? cardData["Description"].GetString() : "";
                    AmountInDeck = cardData.ContainsKey("AmountInDeck") ? cardData["AmountInDeck"].GetInt32() : 1;
                    Attributes = cardData.ContainsKey("Attributes")
                        ? JsonSerializer.Deserialize<List<string>>(cardData["Attributes"].GetRawText())
                        : new List<string>();
                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading card from {FolderPath}: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadFromFileWithImages()
        {
            try
            {
                if (string.IsNullOrEmpty(FolderPath))
                {
                    MessageBox.Show("Card folder path is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string cardInfoPath = Path.Combine(FolderPath, "cardinfo.json");

                if (!File.Exists(cardInfoPath))
                {
                    MessageBox.Show($"Card info file not found: {cardInfoPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read and deserialize JSON
                string cardContent = File.ReadAllText(cardInfoPath);
                var cardData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cardContent);

                if (cardData != null)
                {
                    Name = cardData.ContainsKey("Name") ? cardData["Name"].GetString() : "Unnamed";
                    Description = cardData.ContainsKey("Description") ? cardData["Description"].GetString() : "";
                    AmountInDeck = cardData.ContainsKey("AmountInDeck") ? cardData["AmountInDeck"].GetInt32() : 1;
                    Attributes = cardData.ContainsKey("Attributes")
                        ? JsonSerializer.Deserialize<List<string>>(cardData["Attributes"].GetRawText())
                        : new List<string>();
                }

                // Load image if available
                string imagePath = Path.Combine(FolderPath, "image", "thumbnail.png");
                if (File.Exists(imagePath))
                {
                    Image = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute))
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading card from {FolderPath}: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
