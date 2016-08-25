using System.Collections.Generic;
using CommandLine;

namespace Bud.ExecTesterApp.Options {
  [Verb("call", HelpText = "Calls the given executable with given arguments and suppresses all its output.")]
  public class CallVerb {
    [Value(0, MetaName = "EXECUTABLE", HelpText = "The path of the executable to run.", Required = true)]
    public string Executable { get; set; }

    [Value(1, MetaName = "ARGUMENTS", HelpText = "The arguments to pass to the executable.", Required = false, Default = new string[0])]
    public IEnumerable<string> Arguments { get; set; }
  }
}