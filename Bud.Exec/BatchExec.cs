using System;
using System.Diagnostics;
using System.IO;

namespace Bud {
  /// <summary>
  ///   This class contains a bunch of static methods for batch execution of processes.
  ///   The executed processes must not expect any input.
  /// </summary>
  /// <remarks>
  ///   The naming of the functions is partially inspired by Python's subprocess API.
  /// </remarks>
  public static class BatchExec {
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
      AssertProcessSucceeded(executablePath, args, cwd, process, errorOutput);
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
      AssertProcessSucceeded(executablePath, args, cwd, process, errorOutput.ToString());
      return output;
    }

    private static void AssertProcessSucceeded(string executablePath,
                                               Option<string> arguments,
                                               Option<string> workingDir,
                                               Process process,
                                               string errorOutput) {
      if (process.ExitCode != 0) {
        throw new Exception($"Command '{executablePath}' " +
                            GetArgumentsErrorMessagePart(arguments) +
                            $" at working dir '{workingDir.GetOrElse(Directory.GetCurrentDirectory)}'" +
                            $" failed with error code '{process.ExitCode}'" +
                            $" and error output: {errorOutput}");
      }
    }

    private static string GetArgumentsErrorMessagePart(Option<string> arguments)
      => arguments.Map(argStr => $"with arguments '{argStr}'")
                  .GetOrElse("without args");

    private static Process CreateProcess(string executablePath,
                                         Option<string> arguments = default(Option<string>),
                                         Option<string> workingDir = default(Option<string>)) {
      var process = new Process();
      var argumentsString = arguments.GetOrElse(string.Empty);
      process.StartInfo = new ProcessStartInfo(executablePath, argumentsString) {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };
      if (workingDir.HasValue) {
        process.StartInfo.WorkingDirectory = workingDir.Value;
      }
      return process;
    }

    private static void ReadErrorOutput(Process process) {
      var errorOutput = process.StandardError.ReadToEnd();
      if (!string.IsNullOrEmpty(errorOutput)) {
        Console.Error.Write(errorOutput);
      }
    }

    private static void ProcessOnOutputDataReceived(object sender,
                                                    DataReceivedEventArgs outputLine) {
      if (outputLine.Data != null) {
        Console.WriteLine(outputLine.Data);
      }
    }
  }
}