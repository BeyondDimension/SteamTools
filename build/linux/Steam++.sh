#!/bin/bash
run_path=$(cd -- "$(dirname -- "$0")" && pwd) || exit 1
link_exec="$run_path/Steam++"

# Native AOT already ships the application executable. Do not replace it
# with the link used by the managed package.
if [ -e "$link_exec" ] && [ ! -L "$link_exec" ]; then
    exec "$link_exec" "$@"
fi

assembly="$run_path/assemblies/Steam++.dll"
dotnet_path="$run_path/dotnet"
dotnet_exec="$dotnet_path/dotnet"
if [ ! -f "$assembly" ] || [ ! -x "$dotnet_exec" ]; then
    printf 'Watt Toolkit: the application executable or bundled runtime is missing.\n' >&2
    exit 1
fi

export DOTNET_ROOT="$dotnet_path"
if [ ! -L "$link_exec" ]; then
    ln -s -- "$dotnet_exec" "$link_exec" || exit 1
fi
exec "$link_exec" "$assembly" "$@"
