using CommandLine;

namespace Bud.ExecTesterApp.Options {
  [Verb("echo", HelpText = "Prints the given text to stdout.")]
  public class EchoVerb {
    [Value(0, MetaName = "ECHO TEXT", HelpText = "The text to output via stdout.", Required = true)]
    public string EchoText { get; set; }
  }
}