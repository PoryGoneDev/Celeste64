using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Celeste64;

class Program
{
	public static void Main(string[] args)
	{
		string errorLogPath = Directory.GetCurrentDirectory();

		Log.Info($"Celeste 64 v.{Game.Version.Major}.{Game.Version.Minor}.{Game.Version.Build}");
		Log.Info($"Celeste 64 {Game.AP_VersionString}");

		AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
		{
			HandleError(errorLogPath, (Exception)e.ExceptionObject);
		};

		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		// try
		{
			using var game = new Game(new(
				ApplicationName: Game.GamePath,
				WindowTitle: Game.GameTitle,
				Width: 1280,
				Height: 720,
				Fullscreen: false,
				Resizable: true,
				UpdateMode: UpdateMode.FixedStep(60)
			));
			errorLogPath = game.UserPath;
			game.Run();
		}
		// catch (Exception e)
		// {
		// 	HandleError(errorLogPath, e);
		// }
	}
	
	private static void HandleError(string path, Exception e)
	{
		// write error to console in case they can see stdout
		Console.WriteLine(e?.ToString() ?? string.Empty);

		// construct a log message
		const string ErrorFileName = "ErrorLog.txt";
		StringBuilder error = new();
		error.AppendLine($"Celeste 64 v.{Game.Version.Major}.{Game.Version.Minor}.{Game.Version.Build}");
		error.AppendLine($"Error Log ({DateTime.Now})");
		error.AppendLine($"Call Stack:");
		error.AppendLine(e?.ToString() ?? string.Empty);
		error.AppendLine($"Game Output:");
		error.AppendLine(Log.GetHistory());

		// write to file
		path = Path.Combine(path, ErrorFileName);
		File.WriteAllText(path, error.ToString());

		// open the file
		if (File.Exists(path))
		{
			new Process { StartInfo = new ProcessStartInfo(path) { UseShellExecute = true } }.Start();
		}
	}
}