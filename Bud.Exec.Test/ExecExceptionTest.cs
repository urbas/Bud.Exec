using NUnit.Framework;

namespace Bud {
  public class ExecExceptionTest {
    [Test]
    public void Properties_are_set_correctly() {
      var exception = new ExecException("foo", "fooargs", "foocwd", "foostderr", 42);
      Assert.AreEqual("foo", exception.ExecutablePath);
      Assert.AreEqual("fooargs", exception.Args);
      Assert.AreEqual("foocwd", exception.Cwd);
      Assert.AreEqual("foostderr", exception.ErrorOutput);
      Assert.AreEqual(42, exception.ExitCode);
    }

    [Test]
    public void Message_contains_a_lot_of_information() {
      var exception = new ExecException("fooExecPath", "fooargs", "foocwd", "foostderr", 42);
      Assert.That(exception.Message,
                  Does.Contain("'42'")
                      .And.Contains("error output: foostderr")
                      .And.Contains("fooExecPath")
                      .And.Contains("fooargs")
                      .And.Contains("foocwd"));
    }
  }
}