// include Fake lib
#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.DotNetCli

// Properties
let buildDir = "./build/"
let testDir  = "./test/"

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
