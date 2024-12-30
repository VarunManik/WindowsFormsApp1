using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WindowsFormsApp1.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CATEGORIES
    {
        [EnumMember(Value = "Icon")]
        Icon,
        [EnumMember(Value = "A")]
        A,
        [EnumMember(Value = "B")]
        B,
        [EnumMember(Value = "C")]
        C,
        [EnumMember(Value = "D")]
        D,
        [EnumMember(Value = "Unknown")]
        Unknown
    };

    public enum ROLE
    {
        [EnumMember(Value = "BATSMAN")]
        BATSMAN,
        [EnumMember(Value = "BOWLER")]
        BOWLER,
        [EnumMember(Value = "ALL_ROUNDER")]
        ALL_ROUNDER,
        [EnumMember(Value = "Unknown")]
        Unknown
    };

    public class CategoryDetailsModel
    {
        [JsonProperty("MaxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("Base")]
        public long Base { get; set; }

        [JsonProperty("DefaultIncrement")]
        public long DefaultIncrement { get; set; }

    }
}
