// See https://aka.ms/new-console-template for more information


using Noisrev.League.IO.RST;
using Noisrev.League.IO.RST.Helper;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;

#region ExCode
const int EX_OK		= 0; /* successful termination */
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
const string ConfigConst  = "config";
const string EntriesConst = "entries";
const string VersionConst = "version";
const string ReleaseConst = "0.1.0.0-beta";
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

InitializeHashtable("RSTHashes.txt", out Dictionary<ulong, string> hashTable);

#endregion

#region info

void PrintHelp() {
    println();
    println("Usage: rion [options]");
    println("Usage: rion [input-file-path] [output-file-path]");
    println();
    println("Options:");
    println("  -e|--equals   Check whether the files are the same.");
    println("  -h|--help     Display help.");
    println("  -v|--version  Display version.");
    println();
    println("input-file-path:");
    println("  The file path to input.");
    println("output-file-path:");
    println("  The path to the output file.");
    println();
    Exit(EX_OK);
}

void PrintVersion() {
    var assembly = Assembly.GetExecutingAssembly();
    var assemblyName = assembly.GetName();

    var version = $"{assemblyName.Name} {assemblyName.Version} #{Environment.OSVersion.Platform} {assemblyName.ProcessorArchitecture} {File.GetLastWriteTime(System.AppContext.BaseDirectory + Path.DirectorySeparatorChar + assemblyName.Name).ToString("R", new CultureInfo("en-US"))} {RuntimeInformation.RuntimeIdentifier}";

    println(version);
    Exit(EX_OK);
}
void PrintAppInfo() {
    println();
    println($"Version {ReleaseConst}");
    println($"Powered by .Net {Environment.Version}");
    println();
}

void println(string message = "") => Console.WriteLine(message);

#endregion

#region Methods

static void InitializeHashtable<TKey>(string hashTablePath, out Dictionary<TKey, string> dict) where TKey : IComparable, IComparable<TKey>, IConvertible, IEquatable<TKey>, ISpanFormattable, IFormattable {
    dict = new Dictionary<TKey, string>();

    var method = typeof(TKey).GetMethod("Parse", new Type[] { typeof(string), typeof(NumberStyles) });

    if (method is not null && File.Exists(hashTablePath)) {
        foreach (string? item in File.ReadLines(hashTablePath)) {

            if (item is null) continue;
            
            string hName, value;
            if (item.Contains(' '))
            {
                var index = item.IndexOf(' ');
                hName = item.Substring(0, index);
                value = item.Substring(index + 1);
            }
            else
            {
                (hName, value) = (item, item);
            }

            var hash = (TKey?)method.Invoke(null, new object[] { hName, NumberStyles.HexNumber });

            if (hash is not null && !dict.ContainsKey(hash)) {
                dict.Add(hash, value);
            }
        }
    }

}

void Equals(string[] args)
{
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

void Encoder([NotNull] string input, string? output) {
    /* Check whether it is empty */
    if (string.IsNullOrEmpty(output))
        output = GeNpath(input, ".txt");

    // Json file contents
    var json = JsonDocument.Parse(File.OpenRead(input)).RootElement.EnumerateObject();

    // temp
    JsonProperty tmp;

    // Set Version
    RVersion version;


    if ( /* tmp is null ? */    (tmp = json.FirstOrDefault(x => x.Name.ToLower() == VersionConst)).Value.ValueKind == JsonValueKind.Undefined ||
         /* version.GetRtype is null ? */    (version = (RVersion)Convert.ToByte(tmp.Value.ToString()) ).GetRType() == null)
    {
        /* No version, get the latest */
        version = RVersionHelper.GetLatestVersion();
    }

    // Initialization
    var rst = new RSTFile(version);

    if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == EntriesConst)).Value.ValueKind == JsonValueKind.Object)
    {
        // Entries
        foreach (var item in tmp.Value.EnumerateObject())
        {
            // Property Name
            string property = item.Name;

            // NULLCHECK
            if (string.IsNullOrEmpty(property))
                continue;

            // Compatible with "CommunityDragon" format
            if (property.StartsWith("{"))
                property = property.Replace("{", "").Replace("}", "");
            
            // Parse the key
            if (!ulong.TryParse(property, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong hash))
            { 
                hash = RSTHash.ComputeHash(property, rst.Type);
            }
            rst.AddEntry(hash, item.Value.ToString());
        }
    }
    // Config
    if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == ConfigConst)).Value.ValueKind == JsonValueKind.String)
        rst.SetConfig(tmp.Value.GetString());

    // Write to memory instead of using "File.Create" to avoid creating files that cannot be written out
    using var ms = new MemoryStream();
    rst.Write(ms, false);

    // Outputs data from "MemoryStream"
    println($"output: {output}");
    File.WriteAllBytes(output, ms.ToArray());

    rst.Dispose();
}

void Decoder([NotNull] string input, string? output)
{
    /* Check whether it is empty */
    if (string.IsNullOrEmpty(output))
        output = GeNpath(input, ".json");

    /* RST File */
    var rst = new RSTFile(File.OpenRead(input), true);

    // Set up an output stream
    using var ms = new MemoryStream();
    var jw = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    // Start
    jw.WriteStartObject();

    // Magic
    jw.WriteProperty("RMAG", RSTFile.Magic);
    jw.WriteProperty(VersionConst, ((byte)rst.Version).ToString());

    // Config
    if (!string.IsNullOrEmpty(rst.Config)) {
        jw.WriteProperty(ConfigConst, rst.Config);
    }

    // Entries
    jw.WritePropertyName(EntriesConst);
    // Entries.Start
    jw.WriteStartObject();

    foreach (RSTEntry item in rst.Entries) {
        // The name of the hash
        string hashName;

        if (hashTable.ContainsKey(item.Hash))
            /* Get the name from hashTable */
            hashName = hashTable[item.Hash];
        else /* hashed hexadecimal string */
            hashName = item.Hash.ToString("x");

        jw.WriteProperty(hashName, item.Text);
    }
    // Entries.End
    jw.WriteEndObject();

    // End
    jw.WriteEndObject();
    jw.Flush();

    // Output
    println($"output: {output}");
    File.WriteAllBytes(output, ms.ToArray());

    jw.Dispose();
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
    var fs = File.OpenRead(input);

    if (fs.Length == 0)
        return FileType.Unknown;

    var @byte = fs.ReadByte();
    if (@byte == 0x7B) /* 0x7B = { */
        return FileType.Json;
    if (@byte == 0x52) /* 0x52 = R */
        return FileType.Rst;

    fs.Dispose();
    return FileType.Unknown;
}

[STAThread]
void App()
{
    string input = args[0];
    string output = string.Empty;

    if (args.Length >= 2)
        output = args[1];

    println($"input:  {Path.GetFullPath(input)}");

    FileType type = GetFileType(input);

    if (type == FileType.Rst)
        Decoder(input, output);
    else if (type == FileType.Json)
        Encoder(input, output);
    else
        Error(EX_Error, "   Invalid file type!");

    println();
}

#endregion

App();

internal enum FileType {
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

static class Extensions
{
    public static void WriteProperty(this Utf8JsonWriter utf8JsonWriter, string key, string value)
    {
        utf8JsonWriter.WritePropertyName(key);
        utf8JsonWriter.WriteStringValue(value);
    }
}