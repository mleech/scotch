#!/bin/bash

mono .paket/paket.bootstrapper.exe || exit 1

mono .paket/paket.exe restore || exit 1

mono packages/build/FAKE/tools/FAKE.exe build.fsx $*

