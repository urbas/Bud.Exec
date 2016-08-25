using System;
using System.Collections.Generic;
using System.IO;
using Bud.ExecTesterApp.Options;
using CommandLine;

namespace Bud.ExecTesterApp {
  public class Program {
    public static int Main(string[] args)
      => Parser.Default
               .ParseArguments<OutputFileVerb, ErrorExitVerb, EchoVerb, RunVerb, EnvVarToFileVerb, EchoInputVerb>(args)
               .MapResult<OutputFileVerb, ErrorExitVerb, EchoVerb, RunVerb, EnvVarToFileVerb, EchoInputVerb, int>(
                 DoOutputFile, DoErrorExit, DoEcho, DoRun, DoEnvVarToFile, DoEchoInputVerb, OnError);

    private static int DoRun(RunVerb args)
      => Exec.Run(args.Executable, Exec.Args(args.Arguments)).ExitCode;

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

    private static int DoEnvVarToFile(EnvVarToFileVerb args) {
      File.WriteAllText(args.File, Environment.GetEnvironmentVariable(args.EnvVar));
      return 0;
    }

    private static int DoEchoInputVerb(EchoInputVerb arg) {
      var inputLine = Console.ReadLine();
      Console.WriteLine(inputLine);
      return 0;
    }

    private static int OnError(IEnumerable<Error> errors) {
      return 1;
    }
  }
}