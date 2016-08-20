using System;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.BatchExec;

namespace Bud.Exec {
  public class BatchExecTest {
    [Test]
    public void Run_returns_the_exit_code_of_the_executed_process()
      => Assert.AreEqual(1, Run(GetTesterAppPath()));

    [Test]
    public void Run_redirects_the_output_to_parents_output()
      => Assert.AreEqual("blargh",
        CheckOutput(GetTesterAppPath(), $"run {GetTesterAppPath()} echo blargh").Trim());

    [Test]
    public void Run_redirects_the_stderr_to_parents_stderr() {
      var exception = Assert.Throws<Exception>(() => {
        CheckOutput(GetTesterAppPath(), $"run {GetTesterAppPath()} error-exit 1 blargh");
      });
      Assert.That(exception.Message, Does.Contain("blargh"));
    }

    [Test]
    public void Run_passes_arguments_to_the_process_and_sets_the_working_dir() {
      using (var tmpDir = new TmpDir()) {
        Run(GetTesterAppPath(),
            args: "output-file foo.txt 42",
            cwd: tmpDir.Path);
        FileAssert.AreEqual(tmpDir.CreateFile("42", "expected.txt"),
                            tmpDir.CreatePath("foo.txt"));
      }
    }

    [Test]
    public void CheckCall_throws_exception_with_message_containing_exit_code_and_error_output() {
      using (var tmpDir = new TmpDir()) {
        var exception = Assert.Throws<Exception>(() => {
          CheckCall(GetTesterAppPath(),
                          args: "error-exit 42 Sparta",
                          cwd: tmpDir.Path);
        });
        Assert.That(exception.Message,
                    Does.Contain("'42'")
                        .And.Contains("error output: Sparta")
                        .And.Contains(GetTesterAppPath())
                        .And.Contains("error-exit 42 Sparta")
                        .And.Contains(tmpDir.Path));
      }
    }

    [Test]
    public void CheckCall_returns_the_process_object()
      => Assert.AreEqual(0, CheckCall(GetTesterAppPath(), "error-exit 0").ExitCode);

    [Test]
    public void CheckOutput_returns_the_stdout_of_the_process()
      => Assert.AreEqual("Sparta", CheckOutput(GetTesterAppPath(), "echo Sparta").Trim());

    private static string GetTesterAppPath()
      => Combine(GetDirectoryName(typeof(BatchExecTest).Assembly.Location),
                 "Bud.Exec.TesterApp.exe");
  }
}