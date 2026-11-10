# SteamTools (Watt Toolkit) —— NixOS 优化适配分支 flake
#
# 可直接 import 本 flake 到 nixos-configuration：
#   inputs.steamtools.url = "github:Aozora-Wings/SteamTools-nixos/nixos-branch";
#   modules = [ steamtools.nixosModules.default ... ];
#
# 产物来源（package.nix）：
#   默认 publishUrl = 本仓库 GitHub Release（tag nixos-build，文件名固定 Steam++_linux_x64_nixos.tgz）。
#   CI（.github/workflows/nixos-branch-build.yml）每次构建成功后自动：
#     上传 Release → 计算 sha256 → 回填 package.nix 的 publishUrl/publishSha256 → [skip ci] 提交。
#   版本锁定：flake.lock 固定 nixpkgs revision（nix flake lock 生成），运行与构建同源。
{
  description = "SteamTools (Watt Toolkit) —— NixOS 优化适配分支（官方源码 + NixOS 补丁 + CI 自动构建发布）";

  inputs = {
    # 标准输入；flake.lock 锁定具体 revision（nix flake lock 生成）。
    # 国内网络可 --override-input nixpkgs 为镜像（见 README）。
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      systems = [ "x86_64-linux" ];
      forAllSystems = f: nixpkgs.lib.genAttrs systems (system: f system);
    in
    {
      # 包：steamtools / watt-toolkit / default（输出 out + accelerator，见 package.nix）
      packages = forAllSystems (system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
        in
        {
          # 注意：显式传 src = null —— callPackage 会把 pkgs.src（simple-revision-control）自动填入 src 参数。
          steamtools = pkgs.callPackage ./package.nix { src = null; };
          watt-toolkit = pkgs.callPackage ./package.nix { src = null; };
          default = pkgs.callPackage ./package.nix { src = null; };
        });

      # NixOS 模块：programs.watt-toolkit（包 + 加速器系统服务 + 证书信任 + polkit）
      # 默认 package = self.packages.<system>.default（可被用户覆盖）。
      nixosModules.watt-toolkit = { lib, pkgs, ... }: {
        imports = [ ./module.nix ];
        programs.watt-toolkit.package = lib.mkDefault self.packages.${pkgs.stdenv.hostPlatform.system}.default;
      };
      nixosModules.steamtools = { lib, pkgs, ... }: {
        imports = [ ./module.nix ];
        programs.watt-toolkit.package = lib.mkDefault self.packages.${pkgs.stdenv.hostPlatform.system}.default;
      };
      nixosModules.default = { lib, pkgs, ... }: {
        imports = [ ./module.nix ];
        programs.watt-toolkit.package = lib.mkDefault self.packages.${pkgs.stdenv.hostPlatform.system}.default;
      };

      # 上游开发测试环境：nix develop 进入（.NET SDK 11 preview + 构建/发布工具链）
      devShells = forAllSystems (system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
        in
        {
          default = pkgs.mkShell {
            packages = with pkgs; [
              dotnetCorePackages.sdk_11_0
              dotnetCorePackages.aspnetcore_11_0
              openssl
              git
            ];
            shellHook = ''
              echo "SteamTools NixOS 分支开发测试环境"
              echo "  初始化: git submodule update --init --recursive  # ref/ 为 git submodule，首次必须执行"
              echo "  补丁: git apply ./steamtools-nixos-branch.patch"
              echo "  构建: dotnet build src/BD.WTTS.Client.Tools.Publish -c Release -t:Rebuild -p:UseSharedCompilation=false"
              echo "  发布: cd src/BD.WTTS.Client.Tools.Publish/bin/Release/net11.0 && dotnet pub.dll run --rids linux-x64"
            '';
          };
        });
    };
}
