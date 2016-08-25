using CommandLine;

namespace Bud.ExecTesterApp.Options {
  [Verb("envvar-to-file", HelpText = "Outputs the value of the given environment variable to a file.")]
  public class EnvVarToFileVerb {
    [Value(0, MetaName = "OUTPUT FILE", HelpText = "The path of the file to which to output the value of the variable.", Required = true)]
    public string File { get; set; }

    [Value(1, MetaName = "ENVVAR", HelpText = "The name of the variable whose value to output into the output file.", Required = false, Default = "")]
    public string EnvVar { get; set; }
  }
}