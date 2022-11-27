using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trawler
{

	internal class CommandDef
	{
		internal string name;
		internal string desc;
		internal virtual async Task Execute(SocketSlashCommand command)
		{
			await command.RespondAsync($"Sorry, {command.Data.Name} is not implemented yet!");
		}
	}
	internal partial class Program
	{
		internal Dictionary<string, ApplicationCommandOptionType> paramTypeMap = new Dictionary<string, ApplicationCommandOptionType>()
		{
			{ "String", ApplicationCommandOptionType.String },
			{ "Integer", ApplicationCommandOptionType.Integer },
			{ "User", ApplicationCommandOptionType.User }
		};
		internal static Dictionary<string, CommandDef> commands = new Dictionary<string, CommandDef>();
		internal async Task RegisterCommands()
		{
			await WriteToLog("Registering commands.");

			// Load and build command data.
			List<ApplicationCommandProperties> bulkCommands = new List<ApplicationCommandProperties>();
			foreach(string file in Directory.GetFiles("data/", "cmd_*.json", SearchOption.AllDirectories))
			{
				using (StreamReader filestream = File.OpenText(file))
				{
					using (JsonTextReader reader = new JsonTextReader(filestream))
					{
						JObject cmdJson = (JObject)JToken.ReadFrom(reader);
						Type cmdType = Type.GetType(cmdJson["class"].ToString());
						try
						{
							CommandDef newCmd = (CommandDef)Activator.CreateInstance(cmdType);
							newCmd.name = cmdJson["name"].ToString();
							newCmd.desc = cmdJson["desc"].ToString();
							commands[newCmd.name] = newCmd;

							SlashCommandBuilder builder = new SlashCommandBuilder();
							builder.WithName(newCmd.name);
							builder.WithDescription(newCmd.desc);

							if(cmdJson["params"] != null)
							{
								foreach(JProperty paramJson in cmdJson["params"])
								{
									JObject children = (JObject)paramJson.Value;
									builder.AddOption(
										paramJson.Name,
										paramTypeMap[(string)children["type"]],
										(string)children["desc"],
										isRequired: (bool)children["required"]
									);
								}
							}

							bulkCommands.Add(builder.Build());
							await WriteToLog($"Built {newCmd.name}.");
						}
						catch(Exception e)
						{
							await WriteToLog($"Exception when loading command: {e.Message.ToString()}");
						}
					}
				}
			}

			try
			{
				await client.BulkOverwriteGlobalApplicationCommandsAsync(bulkCommands.ToArray());
			}
			catch(Exception e)
			{
				await WriteToLog($"Exception when bulk registering commands: {e.Message.ToString()}");
			}
		}

		private async Task SlashCommandHandler(SocketSlashCommand command)
		{
			CommandDef cmd = commands[command.CommandName];
			await cmd.Execute(command);
		}
	}

	internal class CommandDefFish : CommandDef
	{
		private bool TryAddMatch(Dictionary<FishingHole, string> matches, FishingHole fishingHole, string fishy, string searchText)
		{
			int foundAt = fishy.ToLower().IndexOf(searchText);
			if(foundAt != -1)
			{
				if(!matches.ContainsKey(fishingHole))
				{
					matches.Add(fishingHole, $"{fishy.Substring(0, foundAt)}**{fishy.Substring(foundAt, searchText.Length)}**{fishy.Substring(foundAt + searchText.Length)}");
				}
				return true;
			}
			return false;
		}

		internal override async Task Execute(SocketSlashCommand command)
		{

			string searchText = (string)command.Data.Options.First().Value;
			searchText = searchText.ToLower();
			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Search Results";
			Dictionary<FishingHole, string> matches = new Dictionary<FishingHole, string>();
			foreach(FishingHole fishHole in Program.fishingHoles)
			{
				if(fishHole.holeId.Equals(searchText))
				{
					matches.Add(fishHole, "");
					break;
				}
				if(!TryAddMatch(matches, fishHole, fishHole.holeName, searchText) && !TryAddMatch(matches, fishHole, fishHole.holeType, searchText))
				{
					foreach(string fishy in fishHole.containsFish)
					{
						if(TryAddMatch(matches, fishHole, fishy, searchText))
						{
							break;
						}
					}
				}
			}
			if(matches.Count == 0)
			{
				embedBuilder.Description = "No matches found.";
			}
			else if(matches.Count == 1)
			{
				KeyValuePair<FishingHole, string> fishHole = matches.First();
				embedBuilder.Title = fishHole.Key.holeName;
				embedBuilder.AddField("Type", fishHole.Key.holeType);
				embedBuilder.AddField("Vnums", string.Join(", ", fishHole.Key.vNums.ToArray()));
				embedBuilder.AddField("Fish", string.Join(", ", fishHole.Key.containsFish.ToArray()));
			}
			else
			{
				string fishResults = "Multiple matches found:\n";
				foreach(KeyValuePair<FishingHole, string> fishHole in matches)
				{
					fishResults = $"{fishResults}\n{fishHole.Key.holeId}. {fishHole.Key.holeName} - {fishHole.Key.holeType} - v{fishHole.Key.vNums[0]} [{fishHole.Value.ToString()}]";
				}
				embedBuilder.Description = $"{fishResults}\n\nSpecify an ID number or a more specific search string for detailed information on a fishing hole.";
			}
			await command.RespondAsync("", new Embed[] { embedBuilder.Build() });
		}
	}

	internal class CommandDefFishAdd : CommandDef
	{
		internal override async Task Execute(SocketSlashCommand command)
		{
			await command.RespondAsync(@"This command isn't implemented yet, so have this fish: >(>>.)");
		}
	}
}