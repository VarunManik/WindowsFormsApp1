using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Model
{
    public class TeamDetails
    {
        public Dictionary<string, TeamInfo> Details;

        public static void SaveInCache()
        {
            lock (_lock)
            {

                try
                {
                    string path = @"C:\Users\Denve\Desktop\AuctionHelper\AuctionCache.json";
                    var val = JsonConvert.SerializeObject(TeamDetails.Instance.Details);

                    if (!File.Exists(path))
                    {
                        using (File.Create(path)) { }
                    }
                    using (StreamWriter writer = new StreamWriter(path, false))
                    {
                        writer.Write(val);
                    }

                    string path1 = @"C:\Users\Denve\Desktop\AuctionHelper\History";

                    if(!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(path1);
                    }

                    path1 = Path.Combine(path1,$"AuctionCache_{DateTime.Now.Ticks.ToString()}.json");

                    if (!File.Exists(path1))
                    {
                        using (File.Create(path1)) { }
                    }
                    using (StreamWriter writer = new StreamWriter(path1, false))
                    {
                        writer.Write(val);
                    }


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static Dictionary<string, TeamInfo> ReadFromCache()
        {
            try
            {
                string path = @"C:\Users\Denve\Desktop\AuctionHelper\AuctionCache.json";
                if (File.Exists(path))
                {
                    string val = string.Empty;
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            val = reader.ReadToEnd();
                        }
                    }
                    if (!string.IsNullOrEmpty(val))
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, TeamInfo>>(val);
                    }
                }
            }
            catch (Exception E)
            {

            }

            var res = new Dictionary<string, TeamInfo>();


            foreach (var tname in AuctionConfig.config.TeamNames)
            {
                TeamInfo tinfo = new TeamInfo();
                tinfo.TotalPurse = AuctionConfig.config.TotalPurseValue;
                tinfo.playerInfos = new Dictionary<string, PlayerInfo>();
                tinfo.PurseRem = AuctionConfig.config.TotalPurseValue;
                tinfo.PurseUtilized = 0;

                var minPlayerReq = new MinPlayerReq();
                minPlayerReq.reqCount = AuctionConfig.config.MaxPlayersInaTeam;
                minPlayerReq.diCategoryWiseCount = AuctionConfig.config.CategoryModel.ToDictionary(x => x.Key, y => y.Value.MaxPlayers);
                tinfo.MinPlayerReq = minPlayerReq;

                tinfo.MinBaseRequired = GetBaseRequired(minPlayerReq.diCategoryWiseCount);

                res.Add(tname, tinfo);
            }
            return res;
        }

        public static long GetBaseRequired(Dictionary<CATEGORIES, int> diCategoryWiseCount)
        {
            long result = 0;
            foreach (var i in diCategoryWiseCount)
            {
                result += i.Value * AuctionConfig.config.CategoryModel[i.Key].Base;
            }
            return result;
        }

        private static TeamDetails _instance = null;
        private static readonly object _lock = new object();

        private TeamDetails()
        {
        }

        public static TeamDetails Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TeamDetails();
                            _instance.Details = ReadFromCache();
                        }
                    }
                }
                return _instance;
            }
        }
    }
    public class TeamInfo
    {
        [JsonProperty("TotalPurse")]
        public long TotalPurse { get; set; }

        [JsonProperty("PurseUtilized")]
        public long PurseUtilized { get; set; } = 0;

        [JsonProperty("PurseRem")]
        public long PurseRem { get; set; }

        [JsonProperty("MinBaseRequired")]
        public long MinBaseRequired { get; set; }

        [JsonProperty("MinimumTeamReq")]
        public MinPlayerReq MinPlayerReq { get; set; }

        [JsonProperty("TopUpValue")]
        public long TopUpValue { get; set; } = 0;


        [JsonProperty("playerList")]
        public Dictionary<string, PlayerInfo> playerInfos { get; set; } = new Dictionary<string, PlayerInfo>();

    }

    public class PlayerInfo
    {
        [JsonProperty("PlayerCategory")]
        public CATEGORIES PlayerCategory { get; set; }

        [JsonProperty("PriceSold")]
        public long PriceSold { get; set; } = 0;
    }

    public class MinPlayerReq
    {
        [JsonProperty("ReqCount")]
        public int reqCount { get; set; }
        public Dictionary<CATEGORIES, int> diCategoryWiseCount { get; set; }

        public override string ToString()
        {
            string Value = $"ReqCount - {reqCount}\n";
            foreach(var i in diCategoryWiseCount)
            {
                Value += $"{i.Key.ToString()} - {i.Value.ToString()} \n";
            }
            return Value;
        }
    }
}
