# 📖 rion

[中文](README_CN.MD) | English

***rion*** is a converter for RST files.  You can use this tool to convert RST files to Json files and Json files back to RST files.

**💡 You can convert the RST file to Json format, then edit the contents of the Json file, and then convert the Json file to the RST file format to modify the RST file.**

![Image](demo.gif)

# ⚙ Command line arguments
```
Usage: rion [options]
Usage: rion [input-file-path]

Options:
  -e|--equals   Check whether the files are the same.
  -o|--output   Path to the output file.
  -h|--help     Display help.
  -v|--version  Display version.

input-file-path:
  The file path to input.
```

# 🚀 Sample

```
/* Convert a single file */
rion fontconfig_en_us.txt
```

```
/* Convert a single file and set the output using -o (or --output) */
rion fontconfig_en_us.txt -o outputFilePath
```

```
/* Convert multiple files */
rion fontconfig_en_us.txt fontconfig_zh_cn.json fontconfig_zh_my.txt ...
```

```
/* Compare files */
// Use the -e option to enter two files for comparison
rion -e fontconfig_en_us.txt fontconfig_zh_cn.txt
```

**You can also drag and drop files into rion to open them, which will convert them directly.**

# 🔖 Other

**Cross-platform support. Clone and compile it yourself if necessary.**

*The Release Version uses AOT compilation*
