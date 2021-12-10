namespace TasksTracker.Clients.Main;

public class Program
{
    private static void Main(string[] args)
    {
        var dataFile = Environment.GetEnvironmentVariable("TASKS_TRACKER_DB_FILE");
        if (string.IsNullOrEmpty(dataFile))
        {
            Console.WriteLine("No database file set");
            Console.WriteLine("");
            Console.WriteLine("Please set an environmental variable called TASKS_TRACKER_DB_FILE with the full path to your SQLite database file.");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Console.WriteLine($"\texport TASKS_TRACKER_DB_FILE={homeFolder}/.task-tracker/data.sqlite");
            return;
        }

        if (args.Length > 0 && args.First().Trim().ToLower().EndsWith("ui"))
        {
            Console.WriteLine("Run TUI");
            return;
        }
        
        Cli.AppRunner.Execute(args);
    }
}