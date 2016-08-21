[![Build status](https://ci.appveyor.com/api/projects/status/r09w19qpguhsv28c/branch/master?svg=true)](https://ci.appveyor.com/project/urbas/bud-exec/branch/master)


__Table of contents__

* [About](#about)


## About

Bud.Exec is a C# library for running processes inspired by Python's subprocess functions like `check_call`.

Example of use:

```csharp
// This method redirects stdout and stderr to the parent process.
int exitCode = Bud.Exec.Run("git", "status");

// Throws if the process returns non-zero exit code.
// Also suppresses all output from stderr and stdout of the executed process.
// The exception will contain the exit code and error output (if any).
Bud.Exec.CheckCall("git", "status");

// Throws if the process returns non-zero exit code.
// Returns the stdout output of the executed process.
// The exception will contain the exit code and error output (if any).
Bud.Exec.CheckOutput("git", "status");

// If your arguments contain spaces, you can use `Bud.Exec.Args()`:
Bud.Exec.CheckOutput("git", Bud.Exec.Args("commit", "-m", "This is a message."));

// To make the above shorter, you can import Bud.Exec statically:
using static Bud.Exec;
CheckOutput("git", Args("commit", "-m", "This is a message."));
```
