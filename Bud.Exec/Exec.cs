using System;
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
    /// <returns>the exit code of the process.</returns>
    /// <remarks>
    ///   The standard output and standard error of the process are redirected to standard output and
    ///   standard error of this process.
    ///   <para>Note that this function does not redirect standard input.</para>
    /// </remarks>
    public static int Run(string executablePath,
                          Option<string> args = default(Option<string>),
                          Option<string> cwd = default(Option<string>)) {
      var process = CreateProcess(executablePath, args, cwd);
      process.OutputDataReceived += ProcessOnOutputDataReceived;
      process.Start();
      process.BeginOutputReadLine();
      ReadErrorOutput(process);
      process.WaitForExit();
      return process.ExitCode;
    }

    /// <summary>
    ///   Runs the command in batch mode (without any input), suppresses all output, and throws an exception
    ///   if it returns a no-zero error code.
    /// </summary>
    /// <returns>the <see cref="Process" />object used to run the executable.</returns>
    /// <exception cref="Exception">
    ///   thrown if the output exits with non-zero error code. The message
    ///   of the exception will contain the error output and the exit code.
    /// </exception>
    public static Process CheckCall(string executablePath,
                                    Option<string> args = default(Option<string>),
                                    Option<string> cwd = default(Option<string>)) {
      var process = CreateProcess(executablePath, args, cwd);
      // NOTE: If we don't read the output to end then sometimes processes get stuck.
      process.OutputDataReceived += (s, a) => {};
      process.Start();
      process.BeginOutputReadLine();
      var errorOutput = process.StandardError.ReadToEnd();
      process.WaitForExit();
      AssertProcessSucceeded(executablePath, args, cwd, errorOutput, process.ExitCode);
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
    /// <returns>the captured output of the executed process.</returns>
    /// <exception cref="Exception">
    ///   thrown if the output exits with non-zero error code. The message
    ///   of the exception will contain the error output and the exit code.
    /// </exception>
    public static string CheckOutput(string executablePath,
                                     Option<string> args = default(Option<string>),
                                     Option<string> cwd = default(Option<string>)) {
      var process = CreateProcess(executablePath, args, cwd);
      var errorOutput = new StringWriter();
      process.ErrorDataReceived += (s, a) => {
        errorOutput.Write(a.Data);
      };
      process.Start();
      process.BeginErrorReadLine();
      var output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();
      AssertProcessSucceeded(executablePath, args, cwd, errorOutput.ToString(), process.ExitCode);
      return output;
    }

    /// <summary>
    ///   Converts a list of command-line parameters to a string. This implementation conforms to the
    ///   specification at https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.arguments.
    /// </summary>
    /// <param name="args">a list of command-line parameters.</param>
    /// <returns>
    ///   A string that can be used as the <paramref name="args"/> parameter
    ///   to the process-invoking functions like <see cref="CheckOutput"/> and others.
    /// </returns>
    public static string Args(params string[] args) => Args((IEnumerable<string>)args);

    /// <summary>
    ///   Converts a list of command-line parameters to a string. This implementation conforms to the
    ///   specification at https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.arguments.
    /// </summary>
    /// <param name="args">a list of command-line parameters.</param>
    /// <returns>
    ///   A string that can be used as the <paramref name="args"/> parameter
    ///   to the process-invoking functions like <see cref="CheckOutput"/> and others.
    /// </returns>
    public static string Args(IEnumerable<string> args) => string.Join(" ", args.Select(Arg));

    private static void AssertProcessSucceeded(string executablePath,
                                               Option<string> args,
                                               Option<string> cwd,
                                               string errorOutput, int exitCode) {
      if (exitCode != 0) {
        throw new Exception($"Command '{executablePath}' " +
                            GetArgumentsErrorMessagePart(args) +
                            $" at working dir '{cwd.GetOrElse(Directory.GetCurrentDirectory)}'" +
                            $" failed with error code '{exitCode}'" +
                            $" and error output: {errorOutput}");
      }
    }

    private static string GetArgumentsErrorMessagePart(Option<string> arguments)
      => arguments.Map(argStr => $"with arguments '{argStr}'")
                  .GetOrElse("without args");

    private static Process CreateProcess(string executablePath,
                                         Option<string> args = default(Option<string>),
                                         Option<string> cwd = default(Option<string>)) {
      var process = new Process();
      var argumentsString = args.GetOrElse(String.Empty);
      process.StartInfo = new ProcessStartInfo(executablePath) {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        Arguments = argumentsString
        
      };
      if (cwd.HasValue) {
        process.StartInfo.WorkingDirectory = cwd.Value;
      }
      return process;
    }

    private static void ReadErrorOutput(Process process) {
      var errorOutput = process.StandardError.ReadToEnd();
      if (!String.IsNullOrEmpty(errorOutput)) {
        Console.Error.Write(errorOutput);
      }
    }

    private static void ProcessOnOutputDataReceived(object sender,
                                                    DataReceivedEventArgs outputLine) {
      if (outputLine.Data != null) {
        Console.WriteLine(outputLine.Data);
      }
    }

    private static object Arg(string arg) {
      var containsSpaces = arg.Contains(" ");
      var quotesEscaped = arg.Replace("\"", containsSpaces ? "\"\"" : "\"\"\"");
      return containsSpaces ? $"\"{quotesEscaped}\"" : quotesEscaped;
    }
  }
}