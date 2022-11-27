using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Trawler
{
	internal class FishingHole
	{
		internal string holeName;
		internal string holeType;
		internal string holeId;
		internal List<string> vNums;
		internal List<string> containsFish;
		internal FishingHole(string _name, string _type, List<string> _vnum, List<string> _fish)
		{
				holeName = _name;
				holeType = _type;
				vNums = _vnum;
				containsFish = _fish;
		}
	}
	internal partial class Program
	{
		private string fishDbPath = @"data/fishdb.json";
		internal static List<FishingHole> fishingHoles = new List<FishingHole>();
		internal async Task LoadFishDatabase()
		{
			if(!File.Exists(fishDbPath))
			{
				await WriteToLog($"No fish database found.");
				return;
			}
			List<string> uniqueFish = new List<string>();
			JArray fishDb = JArray.Parse(File.ReadAllText(fishDbPath));
			foreach(JToken fish in fishDb)
			{
				FishingHole fishHole = new FishingHole(
					fish["name"].ToString(), 
					fish["type"].ToString(), 
					fish["rooms"].ToObject<List<string>>(),
					fish["fish"].ToObject<List<string>>()
				);
				fishingHoles.Add(fishHole);
				fishHole.holeId = fishingHoles.Count.ToString();
				foreach(string fishName in fishHole.containsFish)
				{
					if(!uniqueFish.Contains(fishName))
					{
						uniqueFish.Add(fishName);
					}
				}
			}
			await WriteToLog($"Associated {uniqueFish.Count} fish with {fishingHoles.Count} fishing holes. Done.");
		}
	}
}