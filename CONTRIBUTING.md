Simply create a pull request on GitHub.


# Prerequisites

## Windows

- Install Visual Studio 2015 or later.

## Linux or OS X

- Install [latest Mono](http://www.mono-project.com/download/).


# Building

Open the solution in Visual Studio 2015, build it, and run NUnit tests.


# Releasing a new version

1. Update the version and release notes in [the nuspec file](./Bud.Exec/Bud.Exec.nuspec). Create a pull request for
   this change.
1. Let the builds pass and review the changes.
1. Merge the pull request.
1. [Create a new release on GitHub](./releases/new) and publish it. Appveyor will automatically deploy the package to
   the NuGet server.