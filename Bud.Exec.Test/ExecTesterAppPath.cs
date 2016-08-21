using System.IO;
using Bud.ExecTesterApp;

namespace Bud {
  static internal class ExecTesterAppPath {
    public static string GetTesterAppPath()
      => Path.Combine(Path.GetDirectoryName(typeof(ExecTest).Assembly.Location),
                      $"{typeof(Program).Assembly.GetName().Name}.exe");
  }
}