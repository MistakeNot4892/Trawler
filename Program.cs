using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Trawler
{
	internal class Program
	{
		private static DiscordSocketClient client;
		private static bool verbose = false;
		private static string version = "0.1b";
		private static string token = "unsupplied";
		private static string UsageInformation = "dotnet run Trawler.csproj --token=SOMETOKEN [--verbose --version]";

		internal static void WriteToLog(string message)
		{
			if(verbose)
			{
				Console.WriteLine(message);
			}
			else
			{
				Debug.WriteLine(message);
			}
		}

		internal static void Main(string[] args)
		{

			for(int i = 0;i < args.Length;i++)
			{
				string check_token = args[i];
				if(check_token.ToLower().Contains("version"))
				{
					Console.WriteLine($"Trawler v{version}");
					return;
				}
				else if(check_token.ToLower().Contains("verbose"))
				{
					Program.verbose = true;
				}
				else if(check_token.ToLower().Contains("token"))
				{
					if(Program.token != "unsupplied")
					{
						Console.WriteLine("Duplicate token supplied.");
						Console.WriteLine(UsageInformation);
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
				Console.WriteLine("Please specify a Discord secrets token to use when connecting with this bot.");
				Console.WriteLine(UsageInformation);
				return;
			}

			Console.WriteLine($"Connecting with token '{token}'.");

			new Program().MainAsync().GetAwaiter().GetResult();
			WriteToLog("Done.");
			Console.WriteLine("Connected.");
		}
		internal async Task MainAsync()
		{

			DiscordSocketConfig config = new DiscordSocketConfig();
			config.GatewayIntents = GatewayIntents.All;
			Program.client = new DiscordSocketClient(config);

			// Create client.
			Program.client.Log += Log;
			Program.client.MessageReceived += MessageReceived;

			while(true)
			{
				try
				{
					WriteToLog("Logging in client.");
					await Program.client.LoginAsync(TokenType.Bot, token);
					WriteToLog("Starting main loop.");
					await Program.client.StartAsync();
					await Task.Delay(-1);
				}
				catch(Exception e)
				{
					WriteToLog($"Core loop exception: {e.Message}.");
					break;
				}
			}
		}

		private static Task MessageReceived(SocketMessage message)
		{
			Task.Run(() => HandleMessage(message));
			return Task.FromResult(0);
		}

		private static void HandleMessage(SocketMessage message)
		{
		}
		private Task Log(LogMessage msg)
		{
			return Task.CompletedTask;
		}
	}
}