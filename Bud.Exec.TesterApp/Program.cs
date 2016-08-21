using System;
using System.Collections.Generic;
using System.IO;
using Bud.ExecTesterApp.Options;
using CommandLine;

namespace Bud.ExecTesterApp {
  public class Program {
    public static int Main(string[] args)
      => Parser.Default
               .ParseArguments<OutputFileVerb, ErrorExitVerb, EchoVerb, RunVerb>(args)
               .MapResult<OutputFileVerb, ErrorExitVerb, EchoVerb, RunVerb, int>(
                 DoOutputFile, DoErrorExit, DoEcho, DoRun, OnError);

    private static int DoRun(RunVerb args)
      => Exec.Run(args.Executable, string.Join(" ", args.Arguments));

    private static int DoOutputFile(OutputFileVerb args) {
      File.WriteAllText(args.File, args.Text);
      return 0;
    }

    private static int DoErrorExit(ErrorExitVerb args) {
      Console.Error.WriteLine(args.ErrorText);
      return args.ExitCode;
    }

    private static int DoEcho(EchoVerb args) {
      Console.WriteLine(args.EchoText);
      return 0;
    }

    private static int OnError(IEnumerable<Error> errors) {
      return 1;
    }
  }
}