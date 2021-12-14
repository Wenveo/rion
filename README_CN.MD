# 📖 rion

中文 | [English](README.md)

***rion*** 是一个 RST 文件的转换工具，它可以帮你将 RST 文件转换成 Json 文件格式。

并且可以将转换出的 Json 格式文件转换回 RST，实现 RST 与 Json 文件的相互转换。

**💡 你可以将 RST 文件转换成 Json 格式，通过编辑 Json 文件的内容，再将 Json 文件转换成 RST 文件格式,以实现修改 RST 文件的效果。** 

![Image](demo.gif)

# ⚙ 命令行参数
```
Usage: rion [options]
Usage: rion [input-file-path] [output-file-path]

Options:
  -e|--equals   Check whether the files are the same.
  -h|--help     Display help.
  -v|--version  Display version.

input-file-path:
  The file path to input.
output-file-path:
  The path to the output file.
```

# 🚀 示例
```
/* 转换单个文件 */
rion fontconfig_zh_cn.txt
```
```
/* 转换多个文件 */
rion fontconfig_zh_cn.txt fontconfig_zh_my.json fontconfig_en_us.txt fontconfig...
```
```
/* 对比文件 */
// 使用 -e 选项，输入两个文件进行比对
rion -e fontconfig_zh_cn.txt fontconfig_en_us.txt
```

**你也可以直接把文件拖拽到 rion 中打开，会直接进行转换。**

# 🔖 其它

该程序代码支持跨平台，切不需要做任何修改。

需要在其它平台运行的，请自行编译。

*pre-release 0.1.0.0 版本使用了 AOT 编译*
