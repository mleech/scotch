// include Fake lib
#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.DotNetCli

// Targets
Target "Clean" (fun _ ->
    !! "Scotch/**/bin"
    ++ "Scotch.Tests/**/bin"
    |> CleanDirs
)

Target "Build" (fun _ ->
    DotNetCli.Build ( fun p -> { p with Project = "Scotch/Scotch.fsproj";
                                        Configuration = "Release"; })
)

Target "RunTests" (fun _ ->
    DotNetCli.Test (fun p -> { p with Project = "Scotch.Tests/Scotch.Tests.csproj"; })
)

Target "Package" (fun _ ->
    Paket.Pack (fun p -> { p with OutputPath = "nuget"; })
)

Target "Default" DoNothing

// Dependencies
"Clean"
  ==> "Build"
  ==> "RunTests"
  ==> "Package"
  ==> "Default"

// start build
RunTargetOrDefault "Default"