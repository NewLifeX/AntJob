// 自动选择最新的文件源
var di = ".".AsDirectory();
var srcs = new String[] { @"..\Bin\netstandard2.0", @"..\..\Bin\netstandard2.0", @"..\..\..\X\Bin\netstandard2.0" };
di.CopyIfNewer(srcs, "*.dll;*.exe;*.xml;*.pdb;*.cs");
