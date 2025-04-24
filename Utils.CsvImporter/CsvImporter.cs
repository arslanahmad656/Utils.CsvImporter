using Microsoft.Data.SqlClient;

namespace Utils.CsvImporter
{
    public class CsvImporter(Settings appSettings)
    {
        /// <summary>
        /// This method performs the import. It creates a new connection and then releases it each time it's called. The class itself does not keeps ahold of the connection.
        /// </summary>
        public void ImportCsvFiles()
        {
            try
            {
                var searchOption = appSettings.SearchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var csvFiles = Directory.GetFiles(appSettings.CsvDirectoryPath, "*.csv", searchOption);

                using var connection = new SqlConnection(appSettings.ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    foreach (var csvFilePath in csvFiles)
                    {
                        var tableName = GenerateTableName(csvFilePath);
                        var headers = GetCsvHeaders(csvFilePath);

                        CreateSqlTable(connection, headers, tableName, transaction);
                        ImportCsvDataIntoTable(connection, csvFilePath, headers, tableName, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        private static string[] GetCsvHeaders(string csvFilePath)
        {
            using var reader = new StreamReader(csvFilePath);
            return reader.ReadLine()?.Split(',') ?? [];
        }

        private static void CreateSqlTable(SqlConnection connection, string[] headers, string tableName, SqlTransaction transaction)
        {
            string createTableQuery = $"CREATE TABLE [{tableName}] (";

            foreach (var header in headers)
            {
                createTableQuery += $"[{header}] NVARCHAR(MAX), ";
            }

            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";

            using (var command = new SqlCommand(createTableQuery, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void ImportCsvDataIntoTable(SqlConnection connection, string csvFilePath, string[] headers, string tableName, SqlTransaction transaction)
        {
            using var reader = new StreamReader(csvFilePath);
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var row = reader.ReadLine()?.Split(',') ?? [];
                InsertRowInTable(connection, row, tableName, transaction);
            }
        }

        private static void InsertRowInTable(SqlConnection connection, string[] row, string tableName, SqlTransaction transaction)
        {
            string insertQuery = $"INSERT INTO [{tableName}] VALUES(";

            foreach (var value in row)
            {
                insertQuery += $"'{value.Replace("'", "''")}', ";  // Sanitize single quotes in data
            }

            insertQuery = insertQuery.TrimEnd(',', ' ') + ");";

            using var command = new SqlCommand(insertQuery, connection, transaction);
            command.ExecuteNonQuery();
        }

        private string GenerateTableName(string csvFilePath)
        {
            var fileInfo = new FileInfo(csvFilePath);

            // Start with just the file name, replacing spaces and removing the extension
            var fileName = fileInfo.Name.Replace(" ", "_").Replace(".csv", "");

            if (!appSettings.IncludeDirectoryInTableName)
            {
                return fileName;
            }

            // Calculate the relative path from CsvDirectoryPath to the file's directory
            var relativePath = Path.GetRelativePath(appSettings.CsvDirectoryPath, fileInfo?.DirectoryName ?? string.Empty);

            // Check for directories; ensure no unwanted periods ("." or "..") remain
            if (!string.IsNullOrEmpty(relativePath) && relativePath != ".")
            {
                // Split relative path into directory segments
                var dirSegments = relativePath
                    .Split(Path.DirectorySeparatorChar)     // Split path into directory segments
                    .Select(dir => dir.Replace(" ", "_"));  // Format each to replace spaces

                // Concatenate directory segments and file name with underscores
                fileName = string.Join("_", dirSegments.Append(fileName));
            }

            // Ensure valid SQL table name length is within the acceptable limits
            return fileName.Length > 128 ? fileName.Substring(fileName.Length - 128) : fileName;
        }
    }
}


/***
 * The code was generated with the of GPT (Netwrix AI)
 */