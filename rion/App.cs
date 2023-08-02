// See https://aka.ms/new-console-template for more information

using Noisrev.League.IO.RST;
using Noisrev.League.IO.RST.Helpers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

#region ExCode
const int EX_OK     = 0; /* successful termination */
const int EX_Error  = 1; /* error termination */
const int EX_USAGE  = 2; /* command line usage error */

void Exit(int ex_code) {
    Environment.Exit(ex_code);
}

void Error(int ex_code, string message) {
    Console.Error.WriteLine(message);
    Exit(ex_code);
}

#endregion

#region Const
const string ASSEMBLY_NAME      = "rion";
const string ASSEMBLY_VERSION   = "0.1.3.23082-patch";
const string HASHTABLE_NAME     = "RSTHashes.txt";

const string JSON_CONFIG_NAME   = "config";
const string JSON_ENTRIES_NAME  = "entries";
const string JSON_VERSION_NAME  = "version";
#endregion

#region OnStartup
if (args.Length < 1)
    Error(EX_USAGE, "No input file specified.");
else if (args.Contains("-h") || args.Contains("--help"))
    PrintHelp();
else if (args.Contains("-v") || args.Contains("--version"))
    PrintVersion();
else if (args.Contains("-e") || args.Contains("--equals"))
    Equals(args.Where(x => x != "-e" && x != "--equals").ToArray());
else if (args.Length >= 1 && File.Exists(args[0]))
    PrintAppInfo();
else
    Error(EX_USAGE, "Input file does not exist.");


InitializeHashtable($"{AppDomain.CurrentDomain.BaseDirectory}{HASHTABLE_NAME}", out var hashTable);

#endregion

#region info

void PrintHelp() {
    println();
    println("Usage: rion [options]");
    println("Usage: rion [input-file-path]");
    println();
    println("Options:");
    println("  -e|--equals   Check whether the files are the same.");
    println("  -o|--output   Path to the output file.");
    println("  -h|--help     Display help.");
    println("  -v|--version  Display version.");
    println();
    println("input-file-path:");
    println("  The file path to input.");
    println();
    Exit(EX_OK);
}

void PrintVersion() {
    var version = $"{ASSEMBLY_NAME} {ASSEMBLY_VERSION}";

    println(version);
    Exit(EX_OK);
}
void PrintAppInfo() {
    println();
    println($"Version {ASSEMBLY_VERSION}");
    println($"Powered by .Net 7.0.0");
    println();
}

void println(string message = "") => Console.WriteLine(message);

#endregion

#region Methods

static void InitializeHashtable(string hashTablePath, [NotNull] out Dictionary<ulong, string> dict) {
    dict = new Dictionary<ulong, string>();
    foreach (var item in File.ReadLines(hashTablePath)) {
        if (item is null) continue;

        string hName, value;
        if (item.Contains(' ')) {
            var index = item.IndexOf(' ');
            hName = item[..index];
            value = item[(index + 1)..];
        } else {
            hName = value = item;
        }

        var hash = ulong.Parse(hName, NumberStyles.HexNumber);
        if (!dict.ContainsKey(hash)) {
            dict.Add(hash, value);
        }
    }
}

void Equals(string[] args) {
    if (args.Length == 1) {
        Error(EX_USAGE, "Equals requires two files.");
    }
    if (args.Length < 2) {
        Error(EX_USAGE, "Error: Not enough arguments.");
    }

    if (!File.Exists(args[0])) {
        Error(EX_Error, "Error: File not found.");
    }

    if (!File.Exists(args[1])) {
        Error(EX_Error, "Error: File not found.");
    }

    var rst1 = new RSTFile(File.OpenRead(args[0]), false);
    var rst2 = new RSTFile(File.OpenRead(args[1]), false);

    if (rst1.Equals(rst2)) {
        println("Files are the same.");
    } else {
        println("Files are different.");
    }

    Exit(EX_OK);
}

void Encoder([NotNull] string input, [MaybeNull] string? output) {
    /* Check whether it is empty */
    if (string.IsNullOrEmpty(output))
        output = GeNpath(input, ".stringtable");

    // Json file contents
    using var json = JsonDocument.Parse(File.OpenRead(input)).RootElement.EnumerateObject();

    // temp
    JsonProperty tmp;

    // Set Version
    RVersion version;

    if ( /* tmp is null ? */    (tmp = json.FirstOrDefault<JsonProperty>(x => x.Name.ToLower() == JSON_VERSION_NAME)).Value.ValueKind == JsonValueKind.Undefined ||
         /* version.GetRtype is null ? */    (version = (RVersion)System.Convert.ToByte(tmp.Value.ToString()) ).GetRType() == null)
    {
        /* Not version, get the latest */
        version = RVersion.Ver5;
    }

    // Initialization
    var rst = new RSTFile(version);

    // Entries
    if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == JSON_ENTRIES_NAME)).Value.ValueKind == JsonValueKind.Object) {
        foreach (var item in tmp.Value.EnumerateObject()) {
            // Property Name
            var property = item.Name;

            // NULLCHECK
            if (string.IsNullOrEmpty(property))
                continue;

            // Compatible with "CommunityDragon" format
            if (property.StartsWith("{"))
                property = property.Replace("{", "").Replace("}", "");
            
            // Parse the key
            if (!ulong.TryParse(property, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var hash)) { 
                hash = RSTHash.ComputeHash(property, rst.Type);
            }
            rst.Entries.Add(hash, item.Value.ToString());
        }
    }
    // Config
    if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == JSON_CONFIG_NAME)).Value.ValueKind == JsonValueKind.String)
        rst.Config = tmp.Value.GetString();

    // Write to memory instead of using "File.Create" to avoid creating files that cannot be written out
    using var ms = new MemoryStream();
    rst.Write(ms, false);

    // Outputs data from "MemoryStream"
    println($"output: {output}");
    File.WriteAllBytes(output, ms.ToArray());
}

void Decoder([NotNull] string input, [MaybeNull] string? output) {
    /* Check whether it is empty */
    if (string.IsNullOrEmpty(output))
        output = GeNpath(input, ".json");

    /* RST File */
    var rst = new RSTFile(File.OpenRead(input), true);

    // Set up an output stream
    using var ms = new MemoryStream();
    using var jw = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    // Start
    jw.WriteStartObject();

    // Magic
    jw.WriteProperty("RMAG", Encoding.ASCII.GetString(RSTFile.MagicCode));
    jw.WriteProperty(JSON_VERSION_NAME, ((byte)rst.Version).ToString());

    // Config
    if (!string.IsNullOrEmpty(rst.Config)) {
        jw.WriteProperty(JSON_CONFIG_NAME, rst.Config);
    }

    // Entries
    jw.WritePropertyName(JSON_ENTRIES_NAME);
    // Entries.Start
    jw.WriteStartObject();

    Debug.Assert(hashTable is not null);
    foreach (var item in rst.Entries) {
        // The name of the hash
        string hashName;

        var hash = item.Key;
        if (hashTable.ContainsKey(hash))
            /* Get the name from hashTable */
            hashName = hashTable[hash];
        else /* hashed hexadecimal string */
            hashName = hash.ToString("x");

        jw.WriteProperty(hashName, item.Value);
    }
    // Entries.End
    jw.WriteEndObject();

    // End
    jw.WriteEndObject();
    jw.Flush();

    // Output
    println($"output: {output}");
    File.WriteAllBytes(output, ms.ToArray());
}

/// <summary>
/// Generate a path
/// </summary>
string GeNpath(string input, string extension) {
    /* FullDirPath + SeparatorChar + FileName(no ext) + extension */
    return $@"{ Path.GetDirectoryName(Path.GetFullPath(input)) }{ Path.DirectorySeparatorChar }{ Path.GetFileNameWithoutExtension(input) }{ extension }";
}

/// <summary>
/// Get the type
/// </summary>
FileType GetFileType(string input) {
    // Open the file
    using var fs = File.OpenRead(input);

    if (fs.Length == 0)
        return FileType.Unknown;

    var tmp = fs.ReadByte();
    if (tmp == 0x7B) /* 0x7B = { */
        return FileType.Json;
    else if (tmp == 0x52) /* 0x52 = R */
        return FileType.Rst;
    else 
        return FileType.Unknown;
}

void Convert(string input, string output) {
    println($"input:  {Path.GetFullPath(input)}");

    var type = GetFileType(input);
    if (type == FileType.Rst)
        Decoder(input, output);
    else if (type == FileType.Json)
        Encoder(input, output);
    else
        Error(EX_Error, "   Invalid file type!");

    println();
}

[STAThread]
void App() {
    if (args.Contains("-o") || args.Contains("--output")) {
        args = args.Where(x => x != "-o" && x != "--output").ToArray();
        Convert(args[0], args[1]);
    }
    else Array.ForEach(args, delegate (string str) {
        Convert(str, string.Empty);
    });
}

#endregion

App();

enum FileType {
    /// <summary>
    /// Json File
    /// </summary>
    Json,
    /// <summary>
    /// RST File
    /// </summary>
    Rst,
    /// <summary>
    /// Other
    /// </summary>
    Unknown
}

static class Extensions {
    internal static void WriteProperty(this Utf8JsonWriter utf8JsonWriter, string key, string value) {
        utf8JsonWriter.WritePropertyName(key);
        utf8JsonWriter.WriteStringValue(value);
    }
}
