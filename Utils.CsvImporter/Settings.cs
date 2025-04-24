using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.CsvImporter;

public record Settings(
    string CsvDirectoryPath,       // The directory path that contains the CSV files to be imported.
    string ConnectionString,       // The connection string used to connect to the SQL database.
    bool SearchRecursively,        // Indicates whether to search CSV files recursively.
    bool IncludeDirectoryInTableName  // Indicates whether to include directory names in the SQL table name.
);