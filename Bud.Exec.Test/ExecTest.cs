using System;
using System.IO;
using NUnit.Framework;
using static Bud.Exec;
using static Bud.ExecTesterAppPath;

namespace Bud {
  public class ExecTest {
    [Test]
    public void Run_returns_the_exit_code_of_the_executed_process()
      => Assert.AreEqual(1, Run(GetTesterAppPath(), stdout: TextWriter.Null, stderr: TextWriter.Null).ExitCode);

    [Test]
    public void Run_redirects_the_output_to_parents_output()
      => Assert.AreEqual("blargh",
                         CheckOutput(GetTesterAppPath(), $"run {GetTesterAppPath()} echo blargh").Trim());

    [Test]
    public void Run_redirects_the_stderr_to_parents_stderr() {
      var exception = Assert.Throws<ExecException>(() => {
        CheckOutput(GetTesterAppPath(), $"run {GetTesterAppPath()} error-exit 1 blargh");
      });
      Assert.That(exception.Message, Does.Contain("blargh"));
    }

    [Test]
    public void Run_passes_arguments_to_the_process_and_sets_the_working_dir() {
      using (var tmpDir = new TmpDir()) {
        Run(GetTesterAppPath(),
            args: "output-file foo.txt foobar",
            cwd: tmpDir.Path);
        FileAssert.AreEqual(tmpDir.CreateFile("foobar", "expected.txt"),
                            tmpDir.CreatePath("foo.txt"));
      }
    }

    [Test]
    public void Run_passes_environment_variables_to_the_process() {
      using (var tmpDir = new TmpDir()) {
        Run(GetTesterAppPath(),
            Args("envvar-to-file", tmpDir.CreatePath("foo.txt"), "FOOBAR"),
            env: EnvCopy(EnvVar("FOOBAR", "9001")));
        FileAssert.AreEqual(tmpDir.CreateFile("9001", "expected.txt"),
                            tmpDir.CreatePath("foo.txt"));
      }
    }

    [Test]
    public void Run_stores_the_stdout_into_the_given_string_writer() {
      var stdout = new StringWriter();
      Run(GetTesterAppPath(), Args("echo", "foobar"), stdout: stdout);
      Assert.AreEqual("foobar", stdout.ToString().Trim());
    }

    [Test]
    public void Run_stores_the_stderr_into_the_given_string_writer() {
      var stderr = new StringWriter();
      Run(GetTesterAppPath(), Args("error-exit", "1", "moozar"), stderr: stderr);
      Assert.AreEqual("moozar", stderr.ToString().Trim());
    }

    [Test]
    public void Run_redirects_text_reader_to_stdin() {
      var stdin = new StringReader("this is sparta\n");
      var stdout = new StringWriter();
      Run(GetTesterAppPath(), "echo-input", stdout: stdout, stdin: stdin);
      Assert.AreEqual("this is sparta", stdout.ToString().Trim());
    }

    [Test]
    public void Call_suppresses_the_output()
      => Assert.IsEmpty(CheckOutput(GetTesterAppPath(), Args("call", GetTesterAppPath(), "echo", "blargh")));

    [Test]
    public void CheckCall_throws_exception_with_message_containing_exit_code_and_error_output() {
      using (var tmpDir = new TmpDir()) {
        var exception = Assert.Throws<ExecException>(() => {
          CheckCall(GetTesterAppPath(),
                    args: "error-exit 42 Sparta",
                    cwd: tmpDir.Path);
        });
        Assert.AreEqual(GetTesterAppPath(), exception.ExecutablePath);
        Assert.AreEqual(Option.Some("error-exit 42 Sparta"), exception.Args);
        Assert.AreEqual(Option.Some(tmpDir.Path), exception.Cwd);
        Assert.AreEqual("Sparta", exception.ErrorOutput.Trim());
        Assert.AreEqual(42, exception.ExitCode);
      }
    }

    [Test]
    public void CheckCall_invokes_the_application() {
      using (var tmpDir = new TmpDir()) {
        var process = CheckCall(GetTesterAppPath(), Args("output-file", "foo.txt", "42"), cwd: tmpDir.Path);
        FileAssert.AreEqual(tmpDir.CreateFile("42", "expected.txt"),
                            tmpDir.CreatePath("foo.txt"));
        Assert.AreEqual(0, process.ExitCode);
      }
    }

    [Test]
    public void CheckOutput_throws_exception_with_message_containing_exit_code_and_error_output() {
      using (var tmpDir = new TmpDir()) {
        var exception = Assert.Throws<ExecException>(
          () => CheckOutput(GetTesterAppPath(), "error-exit 42 Sparta", tmpDir.Path));
        Assert.AreEqual(GetTesterAppPath(), exception.ExecutablePath);
        Assert.AreEqual(Option.Some("error-exit 42 Sparta"), exception.Args);
        Assert.AreEqual(Option.Some(tmpDir.Path), exception.Cwd);
        Assert.AreEqual("Sparta", exception.ErrorOutput.Trim());
        Assert.AreEqual(42, exception.ExitCode);
      }
    }

    [Test]
    public void CheckOutput_returns_the_stdout_of_the_process()
      => Assert.AreEqual("Sparta", CheckOutput(GetTesterAppPath(), "echo Sparta").Trim());

    [Test]
    public void Args_simply_joins_parameters_if_args_contain_no_spaces()
      => Assert.AreEqual("foo bar zar", Args("foo", "bar", "zar"));

    [Test]
    public void Args_surrounds_parameters_containing_spaces_with_double_quotes()
      => Assert.AreEqual("\"foo bar\" zar", Args("foo bar", "zar"));

    [Test]
    public void Args_escapes_quotes_in_arguments()
      => Assert.AreEqual("foo\"\"\"bar zar", Args("foo\"bar", "zar"));

    [Test]
    public void Args_integrates_with_CheckOutput() {
      Assert.AreEqual("foo", CheckOutput(GetTesterAppPath(), Args("echo", "foo")).Trim());
      Assert.AreEqual("a\"b", CheckOutput(GetTesterAppPath(), Args("echo", "a\"b")).Trim());
      Assert.AreEqual("a b", CheckOutput(GetTesterAppPath(), Args("echo", "a b")).Trim());
      Assert.AreEqual("\"", CheckOutput(GetTesterAppPath(), Args("echo", "\"")).Trim());
      Assert.AreEqual("\"a b\"\"", CheckOutput(GetTesterAppPath(), Args("echo", "\"a b\"\"")).Trim());
      Assert.AreEqual("\" \"", CheckOutput(GetTesterAppPath(), Args("echo", "\" \"")).Trim());
    }

    [Test]
    public void EnvCopy_returns_a_dictionary_that_equals_to_the_environment_of_this_process()
      => Assert.AreEqual(Environment.GetEnvironmentVariables(), EnvCopy().Value);
  }
}