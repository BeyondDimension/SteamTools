#!/bin/bash
set -euo pipefail

# DTLS 0.5.1's packaged Linux libraries require glibc 2.38. Keep the glibc 2.35 release baseline.
rid="${1:?Specify linux-x64 or linux-arm64.}"
output_directory="${2:?Specify the DTLS output directory.}"
source_revision=db50168907fdfd79f24c705c80485370c10ddbd2
case "$rid:$(uname -s):$(uname -m)" in
    linux-x64:Linux:x86_64) target=x86_64-unknown-linux-gnu ;;
    linux-arm64:Linux:aarch64) target=aarch64-unknown-linux-gnu ;;
    *) printf 'Build DTLS for %s on the matching Linux architecture.\n' "$rid" >&2; exit 1 ;;
esac

working_directory="$(mktemp -d "${TMPDIR:-/tmp}/wtts-dtls.XXXXXXXX")"
trap 'rm -rf -- "$working_directory"' EXIT
source_directory="$working_directory/source"
source_archive="$working_directory/dtls.tar.gz"
curl --fail --location --silent --show-error --retry 3 --connect-timeout 15 \
    "https://codeload.github.com/HMBSbige/DTLS/tar.gz/$source_revision" -o "$source_archive"
mkdir "$source_directory"
tar -xzf "$source_archive" -C "$source_directory" --strip-components=1

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
shared_library="$release_directory/libdtls_native.so"
static_library="$release_directory/libdtls_native.a"
required_glibc="$(LC_ALL=C readelf --version-info "$shared_library" | sed -n 's/.*Name: GLIBC_\([0-9.]*\) .*/\1/p' | sort -V | tail -n 1)"
if [[ -z "$required_glibc" || "$(printf '%s\n' "$required_glibc" 2.35 | sort -V | tail -n 1)" != 2.35 ]]; then
    printf 'DTLS requires glibc %s. Build on Ubuntu 22.04 to preserve the glibc 2.35 baseline.\n' "$required_glibc" >&2
    exit 1
fi

mkdir -p "$output_directory/native" "$output_directory/native-static"
install -m 755 "$shared_library" "$output_directory/native/libdtls_native.so"
install -m 644 "$static_library" "$output_directory/native-static/libdtls_native.a"
printf '%s\n' "$linker_arguments" >"$output_directory/native-static/dtls_native.link.rsp"
printf 'Linux DTLS assets built for %s (glibc %s): %s\n' "$target" "$required_glibc" "$output_directory"
