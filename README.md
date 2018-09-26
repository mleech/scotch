[![Appveyor Build status](https://ci.appveyor.com/api/projects/status/912u30wvvkdxdrr7/branch/master?svg=true)](https://ci.appveyor.com/project/mleech/scotch/branch/master)
[![NuGet Status](https://img.shields.io/nuget/v/Scotch.svg?style=flat)](https://www.nuget.org/packages/Scotch/)

# Scotch

## What is Scotch?
Scotch is a library for recording and replaying HTTP interactions in your test suite.
This can be useful for speeding up your test suite,
or for running your tests on a CI server which doesn't have
connectivity to the HTTP endpoints you need to interact with.

Scotch is based on the [VCR gem](https://github.com/vcr/vcr).

### Step 1.
Run your test suite locally against a real HTTP endpoint in recording mode

```csharp
//Create a HttpClient which uses a RecordingHandler
var scotchMode = ScotchMode.Recording;
var httpClient = HttpClients.NewHttpClient(pathToCassetteFile, scotchMode);
//Use this HttpClient in any class making HTTP calls
var myService = new SomeService(httpClient);
```
Real HTTP calls will be made and recorded to the cassette file.

### Step 2.
Switch to replay mode:
```csharp
var scotchMode = ScotchMode.Replaying;
var httpClient = HttpClients.NewHttpClient(pathToCassetteFile, scotchMode);
```
Commit the code and cassette file(s).
Now when tests are run no real HTTP calls will be made,
the HTTP responses will be replayed from the cassette file. 
Requests are currently matched on HTTP verb and URL, more customisable matching will be added in the future.

## Why "Scotch"?
In keeping with the VCR theme, Scotch was a famous brand of VHS cassettes with [a particularly catchy ad campaign](https://www.youtube.com/watch?v=g4rv81zxBGQ).
