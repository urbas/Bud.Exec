using System;
using System.IO;

namespace Bud {
  public class ExecException : Exception {
    public string ExecutablePath { get; }
    public Option<string> Args { get; }
    public Option<string> Cwd { get; }
    public string ErrorOutput { get; }
    public int ExitCode { get; }

    public ExecException(string executablePath, Option<string> args, Option<string> cwd, string errorOutput, int exitCode)
      : base(ToMessage(executablePath, args, cwd, errorOutput, exitCode)) {
      ExecutablePath = executablePath;
      Args = args;
      Cwd = cwd;
      ErrorOutput = errorOutput;
      ExitCode = exitCode;
    }

    private static string ToMessage(string executablePath, Option<string> args, Option<string> cwd, string errorOutput, int exitCode)
      => $"Command '{executablePath}' " +
         GetArgumentsErrorMessagePart(args) +
         $" at working dir '{cwd.GetOrElse(Directory.GetCurrentDirectory)}'" +
         $" failed with error code '{exitCode}'" +
         $" and error output: {errorOutput}";

    private static string GetArgumentsErrorMessagePart(Option<string> arguments)
      => arguments.Map(argStr => $"with arguments '{argStr}'")
                  .GetOrElse("without args");
  }
}