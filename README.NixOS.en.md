# SteamTools (Watt Toolkit) — NixOS-optimized Branch

> **Adapted build v0.0.1** (based on upstream 3.1.0). **System proxy mode is currently unavailable** (it relies on GNOME gsettings system proxy, which does not apply on NixOS). Only `hosts` and `dns` modes are supported.

This project is a **NixOS adaptation branch of the official SteamTools (Watt Toolkit, formerly Steam++) source code**, solving three core problems on immutable systems (NixOS):

1. **Declarative packaging**: `flake.nix` + `package.nix` provide three outputs — `out` (main GUI), `accelerator` (systemd accelerator service), and `ssl` (system root certificate).
2. **System-level accelerator service**: the accelerator does not spawn as a child process of the GUI. Instead it runs as a systemd service configured by the `programs.watt-toolkit` module (idle, started on demand when the UI triggers acceleration). `AmbientCapabilities` injects `cap_net_bind_service` etc., so a regular user can bind to port 443 without root / fhsenv / setcap.
3. **Version pinning + CI auto-release**: `flake.lock` pins the nixpkgs revision; on every successful build, GitHub Actions automatically uploads a Release and **backfills `url` / `sha256` in `package.nix`** (runtime package matches built artifact, preventing drift).

## Branches and Files

| Path | Description |
| --- | --- |
| `nixos-branch` | Official source + NixOS adaptation patches + CI workflow |
| `steamtools-nixos-branch.patch` | NixOS adaptation patch (applicable via `git apply`) |
| `flake.nix` / `flake.lock` | Directly importable flake (package + NixOS module + devShell) |
| `package.nix` | Package definition (url/sha256 auto-backfilled by CI) |
| `module.nix` | `programs.watt-toolkit` module (service / certificates / polkit) |
| `.github/workflows/nixos-branch-build.yml` | CI: build linux-x64 → upload Release → backfill url/sha256 |

## NixOS Usage (import this flake directly)

In your NixOS configuration:

```nix
{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    steamtools = {
      url = "github:Aozora-Wings/SteamTools-nixos/nixos-branch";
      # For China mirrors (same as nixpkgs):
      # url = "git+https://mirrors.nju.edu.cn/git/SteamTools-nixos.git?ref=nixos-branch";
    };
  };

  outputs = { nixpkgs, steamtools, ... }: {
    nixosConfigurations.myhost = nixpkgs.lib.nixosSystem {
      modules = [
        steamtools.nixosModules.default
        ({ pkgs, ... }: {
          programs.watt-toolkit = {
            enable = true;
            # Defaults to steamtools.packages.${pkgs.system}.default; override explicitly if needed:
            # package = steamtools.packages.${pkgs.system}.default;
            user = "wt";                       # user for the accelerator service (must NOT be root)
            enableAcceleratorService = true;   # system-level accelerator service (idle, started on UI click)
            trustCertificate = true;           # trust bundled root cert via security.pki.certificateFiles
            # trustFirefoxCertificate = true;   # trust the final system CA bundle (/etc/ssl/certs/ca-certificates.crt), default true
            enablePolkit = true;               # allow main user to start/stop accelerator service without password
          };
        })
      ];
    };
  };
}
```

### Shorthand Examples

```nix
# Install only (no service):
environment.systemPackages = [ steamtools.packages.${pkgs.system}.default ];

# Use module only (includes accelerator service + certificate trust + polkit):
imports = [ steamtools.nixosModules.watt-toolkit ];
programs.watt-toolkit.enable = true;

# Legacy name compatibility:
services.steamtools.enable = true;   # maps to the same option
```

## Acceleration Modes (proxyMode)

On NixOS, acceleration mode is managed declaratively via system config (**Hosts / System / DNS options in the UI are greyed out and read-only**; hover shows a hint to configure via system config. The UI displays the current system-configured mode). **System mode is currently unavailable**: its implementation depends on GNOME gsettings (`org.gnome.system.proxy`), which is incompatible with NixOS declarative configuration and non-GNOME desktops (niri/KDE etc.). Only `hosts` and `dns` modes are exposed.

Module options:

```nix
programs.watt-toolkit = {
  enable = true;
  proxyMode = "hosts";   # "hosts" (default) | "dns"
};
```

| Mode | Mechanism | Best for |
| --- | --- | --- |
| `hosts` (default) | Static domain hijacking: domain list is written declaratively by the module/system config; the accelerator listens on 443 for HTTPS MITM | No extra resident dependency; **current static list only covers Steam platform domains** (17 domains). Other platforms (GOG/Epic etc.) are NOT included and must be added to your hosts config manually |
| `dns` | Global dnsmasq dynamic hijacking: dnsmasq runs as the system resolver (`networking.nameservers=127.0.0.1`, NetworkManager `dns=none` to disable DHCP-pushed DNS). On UI acceleration click, the **full domain list of the selected platform** is dynamically written to `/run/dnsmasq.d/steampp.conf` (`address=/domain/127.0.0.1`) and dnsmasq is restarted; stopping acceleration removes the mapping and restores direct routing | Domain list changes dynamically with selected platform, interface-agnostic (works across Wi-Fi / Ethernet). Trade-off: dnsmasq is resident (systemd-managed, `Restart=on-failure`) |

### dns Mode Details

- When acceleration starts, the main GUI writes the selected platform's listening domain list to `/tmp/steampp-domains.conf`.
- The accelerator service's `ExecStartPost` triggers a root oneshot `watt-toolkit-dns-update`, which converts the domain list to dnsmasq `address=/domain/127.0.0.1` rules in `/run/dnsmasq.d/steampp.conf` and runs `systemctl restart dnsmasq` (SIGHUP reload does not re-scan conf-dir).
- `ExecStopPost` removes the mapping (clears `steampp.conf` and restarts dnsmasq), restoring direct routing for selected domains.
- dnsmasq upstreams: `223.5.5.5 / 119.29.29.29 / 8.8.8.8` (all queries outside selected domains go directly to upstream, bypassing the accelerator).

### hosts Mode Notes

The current static hosts list **only contains Steam platform domains** (`steam-hosts.nix`: steamcdn/steamstatic/steampowered etc., 17 domains). Domains for other platforms (GOG/Epic/Origin/Uplay etc.) are **NOT written to system hosts**. If you use hosts mode for other platforms, add the corresponding domains to your system config manually, or switch to `dns` mode (which automatically covers all selected platform domains).

## Outputs

| Output | Contents | Usage |
| --- | --- | --- |
| `out` | Main program (watt-toolkit GUI, wrapper injects runtime) | `environment.systemPackages` |
| `accelerator` | Accelerator module (directory publish, includes `Steam++.Accelerator.dll`) | systemd service `ExecStart` |
| `ssl` | openssl-generated system root certificate (cer + pfx) | `security.pki.certificateFiles` / synced to AppData on app start |

## CI Auto-Backfill (url / sha256)

After a successful build, `.github/workflows/nixos-branch-build.yml` automatically:

1. Computes the sha256 of the tgz.
2. Uploads to GitHub Release (fixed tag `nixos-build`, fixed filename `Steam++_linux_x64_nixos.tgz`, stable URL).
3. Backfills `publishUrl` / `publishSha256` in `package.nix`.
4. Commits with `[skip ci]` to avoid circular builds.

**Prerequisite**: the repository's Actions permissions must be **Read and write** (`Settings → Actions → General → Workflow permissions → Read and write permissions`), otherwise Release upload / backfill commit will fail.

## Development / Test Environment

```sh
nix develop        # enter devShell (.NET SDK 11 preview + build/publish toolchain)
git apply ./steamtools-nixos-branch.patch
dotnet build src/BD.WTTS.Client.Tools.Publish -c Release -t:Rebuild -p:UseSharedCompilation=false
cd src/BD.WTTS.Client.Tools.Publish/bin/Release/net11.0
dotnet pub.dll run --rids linux-x64
```

## License

The upstream project is GPL-3.0-only; this branch follows the same license (see `LICENSE` / upstream [BeyondDimension/SteamTools](https://github.com/BeyondDimension/SteamTools)).
