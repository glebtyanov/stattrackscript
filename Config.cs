using Newtonsoft.Json;
using System.IO;

namespace Scriptrunok
{
	public class Upd
	{
		public int[] Xy { get; set; } = [788, 305];
		public int[] Cvet { get; set; } = [5000000, 7500000];
	}

	public class Podtverdit
	{
		public int[] Xy { get; set; } = [950, 660];
		public int[] Cvet { get; set; } = [8000000, 11200000];
	}

	public class Ok
	{
		public int[] Xy { get; set; } = [950, 660];
		public int[] Cvet { get; set; } = [9000000, 11500000];
	}
	public class Nazad
	{
		public int[] Xy { get; set; } = [467, 975];
		public int[] Cvet { get; set; } = [3000000, 9000000];
	}

	public class Sleeps
	{
		public int kpSleep { get; set; } = 300;
		public int pdSleep { get; set; } = 300;
		public int updSleep { get; set; } = 10000;
		public int upSleep { get; set; } = 300;
		public int nazadSleep { get; set; } = 300;
	}

	public class Lot
	{
		public int[] Kupit { get; set; } = [1400, 360];
		public int[] Nakl { get; set; } = [1080, 360];
		public int[] Cvet { get; set; } = [1, 10900000, 15500000, 16500000];
	}

	public class Knopki
	{
		public Lot Lot1 { get; } = new()
		{
			Nakl = [1078, 359],
			Kupit = [1401, 358],
			Cvet = [1, 12000000, 15000000, 16700000]
		};

		public Upd Upd = new();
		public Podtverdit Podtverdit = new();
		public Ok Ok = new();
		public Nazad Nazad = new();
		public int VisotaSlota { get; set; } = 80;
		public int SlotsColvo { get; set; } = 7;
		public Lot[] Lots { get; private set; } = null!;

		public void FillLots()
		{
			Lots = new Lot[SlotsColvo];
			for (var i = 0; i < Lots.Length; i++)
			{
				Lots[i] = new Lot
				{
					Kupit =
					[
						Lot1.Kupit[0],
						Lot1.Kupit[1] + VisotaSlota * i
					],
					Nakl =
					[
						Lot1.Nakl[0],
						Lot1.Nakl[1] + VisotaSlota * i
					],
					Cvet = Lot1.Cvet
				};
			}
		}
	}

	public class ConfigFromFile
	{
		public Knopki? Knopki { get; set; }
		public Sleeps? Sleeps { get; set; }
	}

	public class Config
	{
		public Config()
		{
			var jsonFilePath = "./Defaults.json";

			if (File.Exists(jsonFilePath))
			{
				try
				{
					var jsonContent = File.ReadAllText(jsonFilePath);
					var configFromFile = JsonConvert.DeserializeObject<ConfigFromFile>(jsonContent);

					if (configFromFile != null)
					{
						ButtonConfig = configFromFile.Knopki ?? new Knopki();
						ButtonConfig.FillLots();
						SleepConfig = configFromFile.Sleeps ?? new Sleeps();
					}
					else
					{
						Console.WriteLine(
							"Ошибка при десериализации файла конфигурации. Взяты значения по умолчанию");
					}
				}
				catch (Exception _)
				{
					Console.WriteLine(
						"Произошла ошибка при чтении файла конфигурации. Взяты значения по умолчанию");
				}
			}
			else
			{
				Console.WriteLine("Файл конфигурации не найден.");
			}
		}

		public Knopki ButtonConfig { get; set; } = new Knopki();
		public Sleeps SleepConfig { get; set; } = new Sleeps();
	}
}

