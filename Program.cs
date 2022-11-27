using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Trawler
{
	internal partial class Program
	{
		private static DiscordSocketClient client;
		private static string version = "0.1b";
		private static string token = "unsupplied";
		private static string UsageInformation = "dotnet run Trawler.csproj --token=SOMETOKEN [--version]";

		internal static Task WriteToLog(string message)
		{
			Console.WriteLine(message);
			return Task.CompletedTask;
		}

		internal static void Main(string[] args)
		{

			for(int i = 0;i < args.Length;i++)
			{
				string check_token = args[i];
				if(check_token.ToLower().Contains("version"))
				{
					WriteToLog($"Trawler v{version}");
					return;
				}
				else if(check_token.ToLower().Contains("token"))
				{
					if(Program.token != "unsupplied")
					{
						WriteToLog("Duplicate token supplied.");
						WriteToLog(UsageInformation);
						return;
					}
					int splitpoint = check_token.IndexOf('=')+1;
					if(check_token.Length >= splitpoint)
					{
						Program.token = check_token.Substring(splitpoint);
						break;
					}
				}
			}

			if(token == null)
			{
				WriteToLog("Please specify a Discord secrets token to use when connecting with this bot.");
				WriteToLog(UsageInformation);
				return;
			}

			WriteToLog($"Connecting with token '{token}'.");
			new Program().MainAsync().GetAwaiter().GetResult();
			WriteToLog("Connected.");
		}
		internal async Task MainAsync()
		{
			
			await LoadFishDatabase(); // >(>>.)

			// Create client.
			Program.client = new DiscordSocketClient(new DiscordSocketConfig());
			Program.client.Log += (msg) => { return WriteToLog($"{msg.ToString()}"); };
			Program.client.Ready += InitializeClient;
			Program.client.SlashCommandExecuted += SlashCommandHandler;

			while(true)
			{
				try
				{
					await WriteToLog("Logging in client.");
					await Program.client.LoginAsync(TokenType.Bot, token);
					await WriteToLog("Starting main loop.");
					await Program.client.StartAsync();
					await Task.Delay(-1);
				}
				catch(Exception e)
				{
					await WriteToLog($"Core loop exception: {e.Message}.");
					break;
				}
			}
		}
		private async Task InitializeClient()
		{
			await WriteToLog("Initializing client.");
			await RegisterCommands();
		}
	}
}