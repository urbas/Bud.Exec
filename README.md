[![Build status](https://ci.appveyor.com/api/projects/status/r09w19qpguhsv28c/branch/master?svg=true)](https://ci.appveyor.com/project/urbas/bud-exec/branch/master)


__Table of contents__

* [About](#about)


## About

Bud.Exec is a wrapper around the `System.Diagnostics.Process` API. Bud.Exec provides a number of static methods for executing processes. Bud.Exec's API has been inspired by Python's subprocess functions.

## Usage

All the API is contained in the `Bud.Exec` static class. You can import this class statically for brevity:

```csharp
using static Bud.Exec;
```

## The basics

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

You can also use `Call` to suppress all output:

```csharp
var process = Call("git", "status");
```

## Spaces and double-quotation marks in arguments

If your arguments contain spaces or double-quotation marks, you can use the `Args` method:

```csharp
Run("git", Args("commit", "-m", "This message contains \" and spaces."));
```

## Exceptions on non-zero exit code

You can use methods `CheckCall` and `CheckOutput` to throw exceptions if the process returns a non-zero exit code:

```csharp
try {
  CheckCall("git", "status");
} catch (ExecException ex) {
  Console.WriteLine(ex.Message);
}
```

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
CheckCall("git", "status", "/foo/bar");
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