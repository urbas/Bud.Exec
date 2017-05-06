Simply create a pull request on GitHub.


# Prerequisites

__Windows__:

- Install Visual Studio 2015 or later.

__Linux or OS X__:

- Install [latest Mono](http://www.mono-project.com/download/).


# Building

Open the solution in Visual Studio 2015, build it, and run NUnit tests.



# Releasing a new version

1. Update the version in [appveyor.yml](./appveyor.yml). Create a pull request for
   this change.
1. Let the builds pass and review the changes.
1. Merge the pull request.
1. [Create a new release on GitHub](../../releases/new) and publish it. Appveyor will automatically deploy the package to
   the NuGet server.