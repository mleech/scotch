// include Fake lib
#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.DotNetCli
open System.IO

// Properties
let buildDir = Path.Combine(FileUtils.pwd(), "build/")
let testDir  = Path.Combine(FileUtils.pwd(), "test/")

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "Build" (fun _ ->
    DotNetCli.Build ( fun p -> { p with Project = "Scotch/Scotch.fsproj";
                                        Configuration = "Release";
                                        Output = buildDir })
)

Target "Test" (fun _ ->
    DotNetCli.Test (fun p -> { p with Project = "Scotch.Tests/Scotch.Tests.csproj";
                                      AdditionalArgs = [ "--output " + testDir ] })
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean"
  ==> "Build"
  ==> "Test"
  ==> "Default"

// start build
RunTargetOrDefault "Default"
