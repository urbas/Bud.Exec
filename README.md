[![Build status](https://ci.appveyor.com/api/projects/status/r09w19qpguhsv28c/branch/master?svg=true)](https://ci.appveyor.com/project/urbas/bud-exec/branch/master) [![NuGet](https://img.shields.io/nuget/v/Bud.Exec.svg)](https://www.nuget.org/packages/Bud.Exec/)

## About

Bud.Exec is a wrapper around the `System.Diagnostics.Process` API. Bud.Exec provides a number of static methods for executing processes. Bud.Exec's API has been inspired by Python's subprocess functions.

## Usage

All the API is contained in the `Bud.Exec` static class. You can import this class statically for brevity:

```csharp
using static Bud.Exec;
```

## `Run` method

`Run` is the most general method in the `Bud.Exec` API. All other methods in `Bud.Exec` delegate to it.

Here's the simplest way to use `Run`:

```csharp
var process = Run("git", "status");
```

The `Run` method returns an object of type `System.Diagnostics.Process`. The process will have terminated before this method returns. You can get the exit code via `process.ExitCode`.

Note that by default the `Run` method pipes standard output and standard error to the standard output and standard error of the parent process. You can change this behaviour by passing `stdout` and `stderr` parameters to the `Run` method:

```csharp
var stdout = new StringWriter();
var stderr = new StringWriter();
var process = Run("git", "status", stdout: stdout, stderr: stderr);
Console.WriteLine($"stdout: {stdout.ToString}, stderr: {stderr.ToString()}");
```

## `Call` method

You can also use `Call` to suppress all output:

```csharp
var process = Call("git", "status");
```

## `Args` and `Arg` methods

If your arguments contain spaces or double-quotation marks, you can use the `Args` or `Arg` methods.

You can use the `Args` method to generate the arguments string from a list of strings:

```csharp
Run("git", Args("commit", "-m", "This message contains \" and spaces."));
```

You can also use the `Args` and `Arg` method inside a string:

```csharp
Run("git", $"--git-dir {Arg(gitDir)} --work-tree {Arg(workDir)} add {Args(filesList)}");
```

## `CheckCall` and `CheckOutput` methods

`CheckCall` and `CheckOutput` methods throw exceptions if the process returns with a non-zero exit code.

`CheckCall` suppresses the output of the invoked process. It also returns the `Process` object. Here's an example of how to use the `CheckCall` method:

```csharp
try {
  CheckCall("git", "status");
} catch (ExecException ex) {
  Console.WriteLine(ex.Message);
}
```

`CheckOutput` returns the string containing the captured standard output of the process. The error output of the process will end up in the exception if the process fails. Here's how you can use the `CheckOutput` method:

```csharp
try {
  var gitStatus = CheckOutput("git", "status");
  Console.WriteLine($"Git status: {gitStatus}");
} catch (ExecException ex) {
  Console.WriteLine(ex.Message);
}
```

Note: the `Message` property contains a great deal of information about the failed process, such as the path of the executable, the arguments passed to it, the working directory of the process, the exit code, and the error output of the process.

## Working directory

All process execution methods in the `Bud.Exec` API take the `cwd` parameter. This parameter is optional and sets the working directory in which the executed process will run. Here's an example of how to use it:

```csharp
CheckCall("git", "status", cwd: "/foo/bar");
```

## Environment variables

All process execution methods in the `Bud.Exec` API take the `env` parameter. This parameter is optional and sets the environment variables of the executed process. Here's an example of how to use it:

```csharp
CheckCall("git", "status", env: EnvCopy(EnvVar("FOO_BAR", "42")));
```

The example above creates a copy of the environment of the current process and adds the variable `FOO_BAR` to it.

## Standard input

All process execution methods in the `Bud.Exec` API take the `stdin` parameter. This parameter is optional and passes some input to the executed process. Here's an example of how to use it:

```csharp
var stdin = new StringReader("this is sparta\n");
CheckCall("hello-world", stdin: stdin);
```
