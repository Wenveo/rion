# 📖 rion

中文 | [English](README.md)

***rion*** 是一个 RST 文件的转换器。你可以使用该工具将 RST 文件转换成 Json 文件格式，以及将 Json 文件转换回 RST文件格式。

**💡 您可以将 RST 文件转换成 Json 格式，然后编辑 Json 文件的内容，再将 Json 文件转换成 RST 文件格式，以实现修改 RST 文件的效果。** 

![Image](demo.gif)

# ⚙ 命令行参数
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

# 🚀 示例

```
/* 转换单个文件 */
rion fontconfig_zh_cn.txt
```

```
/* 转换单个文件并使用 -o (或 --output) 设置输出路径 */
rion fontconfig_zh_cn.txt -o outputFilePath
```

```
/* 转换多个文件 */
rion fontconfig_zh_cn.txt fontconfig_zh_my.json fontconfig_en_us.txt ...
```

```
/* 对比文件 */
// 使用 -e 选项，输入两个文件进行比对
rion -e fontconfig_zh_cn.txt fontconfig_en_us.txt
```

**你也可以直接把文件拖拽到 rion 中打开，会直接进行转换。**

# 🔖 其它

**支持跨平台。如有需要，请自行克隆并编译。**

*Release 版本使用了 AOT 编译*
