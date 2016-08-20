[![Build status](https://ci.appveyor.com/api/projects/status/r09w19qpguhsv28c/branch/master?svg=true)](https://ci.appveyor.com/project/urbas/bud-exec/branch/master)


__Table of contents__

* [About](#about)


## About

Bud.Exec is a reactive wrapper of the [FileSystemWatcher API](https://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher.aspx).

Example of use:

```csharp
var fileWatcher = Bud.Exec.ObserveFileSystem("MyDir", "*.txt", recursive: true);

fileWatcher.Do(changedFile => System.Console.WriteLine($"File {changedFile} has changed."))
           .Wait();
```
