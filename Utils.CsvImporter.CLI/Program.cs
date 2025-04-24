
// Load configuration
using Microsoft.Extensions.Configuration;
using Utils.CsvImporter;

try
{
    Console.WriteLine("Reading the configurations.");

    var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

    // Bind the configuration to the AppSettings record
    var settings = config.Get<Settings>() ?? throw new Exception("Could not load the settings.");

    Console.WriteLine($"Configurations: {settings}");


    Console.WriteLine("Importing the CSV files.");
    // Initialize the CsvImporter with the settings
    var importer = new CsvImporter(settings);

    // Run the import process
    importer.ImportCsvFiles();
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().FullName}: {ex.Message}");
}
finally
{
    Console.WriteLine("Program ended. Press any key to exit...");
    Console.ReadKey(true);
}