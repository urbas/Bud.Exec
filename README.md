[![Build status](https://ci.appveyor.com/api/projects/status/489aqx1p9baycw7w/branch/master?svg=true)](https://ci.appveyor.com/project/urbas/bud-filesobservatory/branch/master)


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
