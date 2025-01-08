using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Model
{
    public class PlayerList
    {
        [JsonProperty("FORM NO")]
        public string formNo { get; set; }
        
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Category")]
        public CATEGORIES Category { get; set; }

        [JsonProperty("Role")]
        public ROLE Role { get; set; } = ROLE.Unknown;

        [JsonProperty("Image Location")]
        public string imageLocation { get; set; } = @"C:\Users\Denve\Desktop\AuctionHelper\player-images\default.jpg";

    }
}
