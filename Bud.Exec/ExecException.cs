using System;
using System.IO;

namespace Bud {
  /// <summary>
  ///   This exception contains information about an excecuted process.
  /// </summary>
  public class ExecException : Exception {
    /// <summary>
    ///   The path to the executable that was invoked.
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    ///   The arguments passed to the invoked executable. If the process
    ///   was invoked without any arguments, then this property will return <c>null</c>.
    /// </summary>
    public string Args { get; }

    /// <summary>
    ///   The working directory in which the process was executed. If the process
    ///   was executed without a specific working directory, then this property will return <c>null</c>.
    /// </summary>
    public string Cwd { get; }

    /// <summary>
    ///   The entire string that was printed in the stderr by the invoked process.
    /// </summary>
    public string ErrorOutput { get; }

    /// <summary>
    ///   The error code returned by the invoked process.
    /// </summary>
    public int ExitCode { get; }

    /// <param name="executablePath">see <see cref="ExecutablePath" /></param>
    /// <param name="args">see <see cref="Args" /></param>
    /// <param name="cwd">see <see cref="Cwd" /></param>
    /// <param name="errorOutput">see <see cref="ErrorOutput" /></param>
    /// <param name="exitCode">see <see cref="ExitCode" /></param>
    public ExecException(string executablePath, string args, string cwd, string errorOutput, int exitCode)
      : base(ToMessage(executablePath, args, cwd, errorOutput, exitCode)) {
      ExecutablePath = executablePath;
      Args = args;
      Cwd = cwd;
      ErrorOutput = errorOutput;
      ExitCode = exitCode;
    }

    private static string ToMessage(string executablePath, string args, string cwd, string errorOutput, int exitCode)
      => $"Command '{executablePath}' " +
         GetArgumentsErrorMessagePart(args) +
         $" at working dir '{cwd ?? Directory.GetCurrentDirectory()}'" +
         $" failed with error code '{exitCode}'" +
         $" and error output: {errorOutput}";

    private static string GetArgumentsErrorMessagePart(string arguments)
      => string.IsNullOrEmpty(arguments) ? "without args" : $"with arguments '{arguments}'";
  }
}