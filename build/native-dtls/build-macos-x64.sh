#!/bin/bash
set -euo pipefail

# NuGet DTLS 0.5.1 ships macOS ARM64 only. Build the missing Intel assets from its source revision.
repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)"
output_directory="${1:-$repository_root/build/artifacts/native-dependencies/dtls/osx-x64}"
source_revision=db50168907fdfd79f24c705c80485370c10ddbd2
target=x86_64-apple-darwin

if [[ "$(uname -s)" != Darwin ]]; then
    printf 'Build the Intel DTLS assets on macOS with Xcode and Rust.\n' >&2
    exit 1
fi

working_directory="$(mktemp -d "${TMPDIR:-/tmp}/wtts-dtls.XXXXXXXX")"
trap 'rm -rf -- "$working_directory"' EXIT
source_directory="$working_directory/source"
git init --quiet "$source_directory"
git -C "$source_directory" fetch --quiet --depth 1 https://github.com/HMBSbige/DTLS.git "$source_revision"
git -C "$source_directory" checkout --quiet --detach FETCH_HEAD

export MACOSX_DEPLOYMENT_TARGET="${MACOSX_DEPLOYMENT_TARGET:-14.0}"
rustup target add "$target"
(
    cd "$source_directory/native"
    cargo rustc --release --locked --target "$target" --target-dir "$working_directory/target" --color never -- --print=native-static-libs
) 2>&1 | tee "$working_directory/build.log"

linker_arguments="$(sed -n 's/^note: native-static-libs: //p' "$working_directory/build.log" | tail -n 1)"
if [[ -z "$linker_arguments" ]]; then
    printf 'rustc did not report the static linker arguments for %s.\n' "$target" >&2
    exit 1
fi

release_directory="$working_directory/target/$target/release"
lipo -verify_arch x86_64 "$release_directory/libdtls_native.dylib"
lipo -verify_arch x86_64 "$release_directory/libdtls_native.a"
mkdir -p "$output_directory/native" "$output_directory/native-static"
install -m 755 "$release_directory/libdtls_native.dylib" "$output_directory/native/libdtls_native.dylib"
install -m 644 "$release_directory/libdtls_native.a" "$output_directory/native-static/libdtls_native.a"
printf '%s\n' "$linker_arguments" >"$output_directory/native-static/dtls_native.link.rsp"
printf 'Intel DTLS assets: %s\n' "$output_directory"
