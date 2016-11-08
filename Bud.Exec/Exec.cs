using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Bud {
  /// <summary>
  ///   This class contains a bunch of static methods for batch execution of processes.
  ///   The executed processes must not expect any input.
  /// </summary>
  /// <remarks>
  ///   The naming of the functions is partially inspired by Python's subprocess API.
  /// </remarks>
  public static class Exec {
    /// <summary>
    ///   Runs the executable at path '<paramref name="executablePath" />'
    ///   with the given args '<paramref name="args" />' in the
    ///   given working directory '<paramref name="cwd" />',
    ///   waits for the executable to finish,
    ///   and returns the exit code of the process.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="args">the args to be passed to the executable.</param>
    /// <param name="cwd">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <param name="env">environment variables to pass to the process.</param>
    /// <param name="stdout">
    ///   the text writer to which the standard output of the process should be written.
    ///   If no text writer is given, then the output will be streamed to the standard output of the calling process
    /// </param>
    /// <param name="stderr">
    ///   the text writer to which the standard error of the process should be written.
    ///   If no text writer is given, then the standard error will be streamed to the standard error of the calling process
    /// </param>
    /// <param name="stdin">the contents of this text reader will be </param>
    /// <returns>the process object that was used to run the process.</returns>
    /// <remarks>
    ///   The standard output and standard error of the process are redirected to standard output and
    ///   standard error of this process.
    ///   <para>This method blocks until the process finishes.</para>
    /// </remarks>
    public static Process Run(string executablePath,
                              string args = null,
                              string cwd = null,
                              IDictionary<string, string> env = null,
                              TextWriter stdout = null,
                              TextWriter stderr = null,
                              TextReader stdin = null) {
      var process = CreateProcess(executablePath, args, cwd, env);
      HandleStdout(stdout, process);
      HandleStderr(stderr, process);
      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      HandleStdin(stdin, process);
      process.WaitForExit();
      return process;
    }
    /// <summary>
    ///   Runs the executable at path '<paramref name="executablePath" />'
    ///   with the given args '<paramref name="args" />' in the
    ///   given working directory '<paramref name="cwd" />',
    ///   waits for the executable to finish,
    ///   and returns the exit code of the process.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="args">the args to be passed to the executable.</param>
    /// <param name="cwd">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <param name="env">environment variables to pass to the process.</param>
    /// <param name="stdin">the contents of this text reader will be </param>
    /// <returns>the process object that was used to run the process.</returns>
    /// <remarks>
    ///   The standard output and standard error of the process are suppressed.
    ///   <para>This method blocks until the process finishes.</para>
    /// </remarks>
    public static Process Call(string executablePath,
                               string args = null,
                               string cwd = null,
                               IDictionary<string, string> env = null,
                               TextReader stdin = null)
      => Run(executablePath, args, cwd, env, stdout: TextWriter.Null, stderr: TextWriter.Null, stdin: stdin);

    /// <summary>
    ///   Runs the command in batch mode (without any input), suppresses all output, and throws an exception
    ///   if it returns a no-zero error code.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="args">the args to be passed to the executable.</param>
    /// <param name="cwd">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <param name="env">environment variables to pass to the process.</param>
    /// <param name="stdin">the contents of this text reader will be </param>
    /// <returns>the <see cref="Process" />object used to run the executable.</returns>
    /// <exception cref="ExecException">
    ///   thrown if the output exits with non-zero error code. The message
    ///   of the exception will contain the error output and the exit code.
    /// </exception>
    public static Process CheckCall(string executablePath,
                                    string args = null,
                                    string cwd = null,
                                    IDictionary<string, string> env = null,
                                    TextReader stdin = null) {
      var stderr = new StringWriter();
      var process = Run(executablePath, args, cwd, env, TextWriter.Null, stderr, stdin);
      AssertProcessSucceeded(executablePath, args, cwd, stderr.ToString(), process.ExitCode);
      return process;
    }

    /// <summary>
    ///   Runs the executable at <paramref name="executablePath" /> in batch mode (not passing any input
    ///   to the process), captures all its standard output (ignoring standard error) into a string,
    ///   waits for the process to finish, and returns the captured output as a string upon completion.
    /// </summary>
    /// <param name="executablePath">the path of the executable to run.</param>
    /// <param name="args">the args to be passed to the executable.</param>
    /// <param name="cwd">
    ///   the working directory in which to run. If omitted, the executable will run in the current
    ///   working directory.
    /// </param>
    /// <param name="env">environment variables to pass to the process.</param>
    /// <param name="stdin">the contents of this text reader will be </param>
    /// <returns>the captured output of the executed process.</returns>
    /// <exception cref="ExecException">
    ///   thrown if the output exits with non-zero error code. The message
    ///   of the exception will contain the error output and the exit code.
    /// </exception>
    public static string CheckOutput(string executablePath,
                                     string args = null,
                                     string cwd = null,
                                     IDictionary<string, string> env = null,
                                     TextReader stdin = null) {
      var stderr = new StringWriter();
      var stdout = new StringWriter();
      var process = Run(executablePath, args, cwd, env, stdout, stderr, stdin);
      AssertProcessSucceeded(executablePath, args, cwd, stderr.ToString(), process.ExitCode);
      return stdout.ToString();
    }

    /// <summary>
    ///   Converts a list of command-line parameters to a string. This implementation conforms to the
    ///   specification at https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.arguments.
    /// </summary>
    /// <param name="args">a list of command-line parameters.</param>
    /// <returns>
    ///   A string that can be used as the <paramref name="args" /> parameter
    ///   to the process-invoking functions like <see cref="CheckOutput" /> and others.
    /// </returns>
    public static string Args(params string[] args) => Args((IEnumerable<string>) args);

    /// <summary>
    ///   Converts a list of command-line parameters to a string. This implementation conforms to the
    ///   specification at https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.arguments.
    /// </summary>
    /// <param name="args">a list of command-line parameters.</param>
    /// <returns>
    ///   A string that can be used as the <paramref name="args" /> parameter
    ///   to the process-invoking functions like <see cref="CheckOutput" /> and others.
    /// </returns>
    public static string Args(IEnumerable<string> args) => string.Join(" ", args.Select(Arg));

    /// <returns>a copy of the current processes' environment.</returns>
    public static IDictionary<string, string> EnvCopy(params Tuple<string, string>[] overrides) {
      var envCopy = ToStringDictionary(Environment.GetEnvironmentVariables());
      foreach (var envVar in overrides) {
        envCopy[envVar.Item1] = envVar.Item2;
      }
      return envCopy;
    }

    /// <param name="varName">The name of the environment variable.</param>
    /// <param name="varValue">The value of the environment variable.</param>
    /// <returns>a 2-element tuple that can be used in the <see cref="EnvCopy" /> method.</returns>
    public static Tuple<string, string> EnvVar(string varName, string varValue)
      => Tuple.Create(varName, varValue);

    private static void AssertProcessSucceeded(string executablePath,
                                               string args,
                                               string cwd,
                                               string errorOutput,
                                               int exitCode) {
      if (exitCode != 0) {
        throw new ExecException(executablePath, args, cwd, errorOutput, exitCode);
      }
    }

    private static Process CreateProcess(string executablePath,
                                         string args = null,
                                         string cwd = null,
                                         IDictionary<string, string> env = null) {
      var process = new Process();
      var argumentsString = args ?? string.Empty;
      process.StartInfo = new ProcessStartInfo(executablePath) {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        Arguments = argumentsString,
      };
      if (env != null) {
        process.StartInfo.Environment.Clear();
        foreach (var envVar in env) {
          process.StartInfo.Environment[envVar.Key] = envVar.Value;
        }
      }
      if (cwd != null) {
        process.StartInfo.WorkingDirectory = cwd;
      }
      return process;
    }

    private static object Arg(string arg) {
      var containsSpaces = arg.Contains(" ");
      var quotesEscaped = arg.Replace("\"", containsSpaces ? "\"\"" : "\"\"\"");
      return containsSpaces ? $"\"{quotesEscaped}\"" : quotesEscaped;
    }

    private static Dictionary<string, string> ToStringDictionary(IDictionary originalDict) {
      var dictEnum = originalDict.GetEnumerator();
      var stringDictionary = new Dictionary<string, string>();
      while (dictEnum.MoveNext()) {
        stringDictionary.Add((string) dictEnum.Key, (string) dictEnum.Value);
      }
      return stringDictionary;
    }

    private static void HandleStdout(TextWriter stdout, Process process) {
      if (stdout == null) {
        process.OutputDataReceived += ProcessOnOutputDataReceived;
      } else {
        process.OutputDataReceived += (sender, eventArgs) => {
          if (eventArgs.Data != null) {
            stdout.WriteLine(eventArgs.Data);
          }
        };
      }
    }

    private static void HandleStderr(TextWriter stderr, Process process) {
      if (stderr == null) {
        process.ErrorDataReceived += ProcessOnErrorDataReceived;
      } else {
        process.ErrorDataReceived += (sender, eventArgs) => {
          if (eventArgs.Data != null) {
            stderr.WriteLine(eventArgs.Data);
          }
        };
      }
    }

    private static void HandleStdin(TextReader stdin, Process process) {
      if (stdin == null) {
        return;
      }
      const int bufferLength = 8192;
      var buffer = new char[bufferLength];
      int readCount;
      while ((readCount = stdin.Read(buffer, 0, bufferLength)) > 0) {
        process.StandardInput.Write(buffer, 0, readCount);
      }
    }

    private static void ProcessOnOutputDataReceived(object sender,
                                                    DataReceivedEventArgs outputLine) {
      if (outputLine.Data != null) {
        Console.WriteLine(outputLine.Data);
      }
    }

    private static void ProcessOnErrorDataReceived(object sender,
                                                   DataReceivedEventArgs outputLine) {
      if (outputLine.Data != null) {
        Console.Error.WriteLine(outputLine.Data);
      }
    }
  }
}