#!/usr/bin/env bash
set -euo pipefail

rm -rf nuget
mkdir nuget

dotnet tool restore

pushd ./src/Storage
./build.sh "$@"
popd

pushd ./src/IdentityServer4
./build.sh "$@"
popd

pushd ./src/EntityFramework.Storage
./build.sh "$@"
popd

pushd ./src/EntityFramework
./build.sh "$@"
popd

pushd ./src/AspNetIdentity
./build.sh "$@"
popd
