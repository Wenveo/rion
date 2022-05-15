# 📖 rion

[中文](README_CN.MD) | English

***rion*** is an RST file conversion tool that allows you to convert RST files to Json files.

In addition, the converted Json file can be converted back to RST to realize the conversion between RST and Json file.

**💡 You can convert the RST file to Json format by editing the content of the Json file and then converting the Json file to RST format to achieve the effect of modifying the RST file.**

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

The program code supports cross-platform and does not need to be modified.

If you need to run on another platform, please compile it yourself.

*The Release Version uses AOT compilation*
