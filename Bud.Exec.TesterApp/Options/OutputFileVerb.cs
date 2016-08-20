using CommandLine;

namespace Bud.Exec.TesterApp.Options {
  [Verb("output-file", HelpText = "Outputs text to the given file.")]
  public class OutputFileVerb {
    [Value(0, MetaName = "OUTPUT FILE", HelpText = "The path of the file to which to output the text.", Required = true)]
    public string File { get; set; }

    [Value(1, MetaName = "TEXT", HelpText = "The text to output into the output file.", Required = false, Default = "")]
    public string Text { get; set; }
  }
}