using LiteDB;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardWizardWPF
{
    public static class DatabaseManager
    {
        private static readonly string DbPath = "cards.db";
        private static LiteDatabase? _database;

        public static LiteDatabase Instance
        {
            get
            {
                if (_database == null)
                {
                    _database = new LiteDatabase(DbPath);
                }
                return _database;
            }
        }

        public static ILiteCollection<Card_Model> GetCardsCollection()
        {
            return Instance.GetCollection<Card_Model>("cards");
        }
    }

}
