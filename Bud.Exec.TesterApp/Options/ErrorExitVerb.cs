using CommandLine;

namespace Bud.Exec.TesterApp.Options {
  [Verb("error-exit", HelpText = "Exits with the given error code and outputs the given error output.")]
  public class ErrorExitVerb {
    [Value(0, MetaName = "EXIT CODE", HelpText = "The exit code with which this process should terminate.", Required = true)]
    public byte ExitCode { get; set; }

    [Value(1, MetaName = "ERROR TEXT", HelpText = "The text to output via stderr.", Required = false, Default = "")]
    public string ErrorText { get; set; }
  }
}