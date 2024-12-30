using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Model
{

    public static class AuctionConfig
    {
        public static AuctionConfigModel config = new AuctionConfigModel();
        public static void ReadConfig()
        {
            try
            {
                var val = File.ReadAllText(@"C:\Users\Denve\Desktop\AuctionHelper\AuctionConfig.json");

                config = JsonConvert.DeserializeObject<AuctionConfigModel>(val);
                config.operation();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error : {e}");
            }
        }
    }

    public class AuctionConfigModel
    {
        [JsonProperty("TeamNames")]
        public List<string> TeamNames { get; set; } = new List<string>();

        [JsonProperty("CategoryModel")]
        public Dictionary<CATEGORIES, CategoryDetailsModel> CategoryModel { get; set; }

        [JsonProperty("MaxPlayersInaTeam")]
        public int MaxPlayersInaTeam { get; set; }

        [JsonProperty("TotalPurseValue")]
        public long TotalPurseValue { get; set; }

        [JsonProperty("FullPlayerList")]
        public List<PlayerList> FullPlayerList { get; set; }

        public Dictionary<string, (ROLE, CATEGORIES, string)> PlayerList { get; set; }

        public void operation()
        {
            PlayerList = FullPlayerList?.ToDictionary(x => x.Name.ToLower(), x => (x.Role, x.Category, x.imageLocation));
        }
    }
}
