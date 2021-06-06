# DevMark
A benchmarking tool to find the right hardware for your development environment.
 If you're a developer and ever wondered what hardware to buy to maximize your productivity you've come to the right place. DevMark is a easy to use, customizable benchmarking utility that'll help you pick the right hardware for your day to day development tasks.
## Installation
The easiest way to install DevMark is using the dotnet CLI. Run the following command to install it as a global tool:
```
dotnet tool install --global DevMark
```

## How to use
```
# We start by making sure dependencies are installed:
DevMark check --all

# After fixing any issues we could run all built in tests:
DevMark run --all

# We could also check/run a specific test by providing a name or path:
DevMark run EfCore

# Lets also upload our test results so we can compare with others:
DevMark upload "DevMark_TestResult_*timestamp*.json" --api-key *secret*
```

## Share your results

At https://www.devbenchmark.com/ you can see all results for the built in tests that have been uploaded by other users.

## The goal of DevMark

- Educate people on what hardware to use for what tasks. Will an older and slower CPU with more cores perform better than newer with less?
- See how well different hardware perform for different development tasks. Is your workload CPU intensive, or maybe a new harddrive will do the trick?
- Give you guidance when picking new hardware.
- Provide some form of evidence why your boss should buy you a new computer :)
- Let you set up your own benchmarks to test what works best for you, all using a simple YAML file.

One of the goal of DevMark is making it as easy to use as possible. Built on dotnet 5 with a simple CLI, an environment many of us are familiar with. All tests come with a dependency checker. It'll verifying that you've got everything required to run the test and promt you with simple install instructions otherwise.

# Features
 
- Highly customizable benchmarks using YAML files.
- Running multiple commands using the same Shell. Allowing for setup and configuration of external dependencies before running an isolated test.
- Built in tests for common development environments.
- Dependency checks for Visual Studio, NodeJS (npm, yarn), .NET Core and .NET Framework.
- Sanity checks, making sure tests are not beeing run from a OneDrive synced folder or network drive.

#The YAML configuration file

Here's a sample YAML file, describing the features available for anyone who would like to building their own tests.
```
name: "Sample App"
version: "1.0.1"
description: "Benchmark how well your hardware performs when building and testing our sample app."
autor: "Some additional information about the author of the test and the app we're testing."

dependencies:
  - git: 
      minVersion: "2.0"
  - path:
      # Here we make sure there work directory isnt using a path which will crash the build due to the famous 260 path limit in Windows.
      requiredLength: 172
    # Our test requires NodeJS, so we add a dependency for that.
  - NodeJS:
      minVersion: "12.16"
      # We're using native code in our modules. This will check that the required Visual Studio tools are installed on a Windows environment.
      nativeCppModules: true
      NPM:
        minVersion: "6.14"
        globalModules:
          # We also require yarn to be installed globaly for our build to work.
          - name: "yarn"
            minVersion: "1.22.0"
  # Our test could have a dependency on a specific version of Visual Studio.
  - VisualStudio:
      minVersion: "16.3"
      maxVersion: "16.99"
      # And require some specific components to be installed in order to work.
      components:
        - "Microsoft.VisualStudio.Workload.NetWeb"
        - "Microsoft.Net.Component.4.7.2.TargetingPack"
        - "Microsoft.NetCore.Component.DevelopmentTools"
        - "Microsoft.NetCore.Component.SDK"
  # We could also add dependencies on both dotnet core and dotnet framework versions.
  - dotnetFramework:
      minVersion: "4.7.1"
  - dotnet: 
      minVersion: "5.0"

initializers:
  # We start by cloning our immaginary repository.
  - git:
      name: "Clone repository"
      repository: "https://github.com/sample.git"
      branch: "1.0.1"
      clean: false
  # Then we make sure all external dependencies are installed. We dont want our test to be dependent on the speed of the internet connection.
  - shell:
      name: "Restore environment"
      cmd: "yarn --cwd packages/sample install"

tests:
  # Here we can define one or more tests to run.
  - name: "build"
    description: "This test will measure how fast your machine can build a Our Sample js library from soure code."
    category: "default"
    # We want to run 3 test itterations to get a good sample size.
    iterations: 3
    # We also want to start of with an initial warmup run for test consisitency.
    warmupIterations: 1
    benchmark:
      # Here's the actual command we measure the performance of
      - shell:
          name: "BUILD"
          cmd: "yarn --cwd packages/sample run build"

cleanup:
  - process:
      # Sometimes (like when building EF Core) the build pipeline will download and load a specific SDK version
      # that will spawn some processes. This allows us to kill any remaining process started in the work directory.
      killWorkDirChildren: true
  # When we're done we want to cleanup all temp files. We do so using a git clean -ffdx.
  - git: 
      clean: true
      # Some tests leaves a few files that will fail to clean up. This can either be libraries loaded into
      # the same appdomain as the pipeline, or a command that has created a file longer than the default 260 char limit.
      allowLockedFiles: true
```

## On the horizon
- Linux and Mac support.
- Support for more kind of dependencies.
- Environmet variables set through CLI params, allowing authentication for custom in house tests.
- Running tests through docker images to provide some level of security and prevent poluting your computer with all dependencies. If the benchmarks still make any senese when running in a container...
- UI built on .NET Multi-platform App UI (MAUI).

