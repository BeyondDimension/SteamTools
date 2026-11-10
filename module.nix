# NixOS 模块：programs.watt-toolkit（Steam++ / Watt Toolkit）
#
# 用法（在 nixos-config 的 configuration 中）：
#   imports = [ steamtools-flake.nixosModules.default ];
#   programs.watt-toolkit = {
#     enable = true;
#     user = "wt";            # 加速器服务运行用户（不能是 root）
#     enableAcceleratorService = true;   # 系统级加速器服务（空闲，UI 点击时拉起）
#     trustCertificate = true;           # security.pki.certificates 信任运行时导出证书
#     enablePolkit = true;               # 允许主用户无密码启停加速器服务
#     proxyMode = "hosts";    # 加速模式：hosts（静态域名，默认）| dns（全局 dnsmasq 动态劫持）
#   };
#
# 对应现网部署（nixos-config/modules/system/security/default.nix 中
# systemd.services.steampp-accelerator + polkit 规则的独立可复用形式）。
# 兼容旧用法：services.steamtools = { ... } 仍可用（映射到同一配置）。

{ lib, config, pkgs, ... }:

let
  cfg = config.programs.watt-toolkit;
  legacyCfg = config.services.steamtools;

  # dns 模式：加速器服务启停时更新 dnsmasq 域名映射的 root oneshot 服务
  dnsUpdateScript = pkgs.writeShellScript "watt-toolkit-dns-update" ''
    set -e
    mkdir -p /run/dnsmasq.d
    if [ -f /run/watt-toolkit/domains.conf ]; then
      # 每行一个域名 → address=/域名/127.0.0.1
      sed 's|^|address=/|; s|$|/127.0.0.1|' /run/watt-toolkit/domains.conf > /run/dnsmasq.d/steampp.conf
    else
      : > /run/dnsmasq.d/steampp.conf
    fi
    # SIGHUP（reload）不会重新扫描 conf-dir（只重读 /etc/hosts），
    # 必须 restart 才会加载刚写入的 steampp.conf；失败时回退 reload。
    systemctl restart dnsmasq || systemctl reload dnsmasq || true
  '';
in
{
  options.programs.watt-toolkit = {
    enable = lib.mkEnableOption "Watt Toolkit (Steam++)";

    package = lib.mkOption {
      type = lib.types.package;
      default = pkgs.watt-toolkit;
      defaultText = lib.literalExpression "pkgs.watt-toolkit";
      description = "Watt Toolkit 包（需包含 out / accelerator 两个输出）。";
    };

    user = lib.mkOption {
      type = lib.types.str;
      default = "wt";
      description = "加速器系统服务的运行用户（不能为 root；AmbientCapabilities 由 root systemd 注入）。";
    };

    group = lib.mkOption {
      type = lib.types.str;
      default = "users";
      description = "加速器系统服务的运行组。";
    };

    enableAcceleratorService = lib.mkEnableOption ''
      加速器系统级 systemd 服务（空闲，由主程序 UI 点击加速时 systemctl start 触发；
      AmbientCapabilities=CAP_NET_BIND_SERVICE CAP_NET_RAW CAP_NET_ADMIN 使普通用户
      可监听 443，无需 root/fhsenv/setcap）''
    ;

    trustCertificate = lib.mkEnableOption ''
      将程序运行时生成并导出到 /var/lib/watt-toolkit 的 SteamTools 根证书
      （.cer，私钥留在 AppData）以 security.pki.certificates 声明式信任''
    ;

    trustFirefoxCertificate = lib.mkOption {
      type = lib.types.bool;
      default = true;
      description = "让 Firefox 信任系统最终信任的证书集合：写入企业策略（/etc/firefox/policies/policies.json），Install 指向系统 CA 合并库 /etc/ssl/certs/ca-certificates.crt（包含 security.pki 声明式信任的全部证书，含运行时导出证书与用户自定义证书）。Firefox 默认不读系统 CA 库，需显式 Install。";
    };

    enablePolkit = lib.mkEnableOption ''
      允许 ${cfg.user} 无密码启停加速器服务（主程序点击加速时 systemctl start 所需）''
    ;

    proxyMode = lib.mkOption {
      type = lib.types.enum [ "hosts" "dns" ];
      default = "hosts";
      description = ''
        加速代理模式：
        - hosts：静态域名劫持（默认）。域名列表由本模块/系统配置静态写入
          （当前仅覆盖 Steam 平台域名），加速器监听 443 做 HTTPS 中间人。
        - dns：全局 dnsmasq 动态劫持。启用 dnsmasq 作为系统解析器
          （networking.nameservers=127.0.0.1，NetworkManager dns=none 禁用 DHCP 下发 DNS），
          主程序点击加速时把选中平台的监听域名列表写入 /tmp/steampp-domains.conf，
          由加速器服务 ExecStartPost 联动 root oneshot 服务更新
          /run/dnsmasq.d/steampp.conf（address=/域名/127.0.0.1）并 reload dnsmasq，
          选中域名走本地加速器、其他域名由 dnsmasq 转发上游直连；
          停止加速时自动撤销映射恢复直连。接口无关（多 WiFi/有线通用）。
      '';
    };

    serviceName = lib.mkOption {
      type = lib.types.str;
      default = "steampp-accelerator";
      description = "加速器 systemd 服务名（与主程序 NixOS 分支 StartupNixOS.SERVICE_NAME 一致）。";
    };
  };

  # 向后兼容：services.steamtools = { ... } 映射到 programs.watt-toolkit
  options.services.steamtools = {
    enable = lib.mkEnableOption "SteamTools (Watt Toolkit)（旧名，已迁移到 programs.watt-toolkit）";
    package = lib.mkOption { type = lib.types.package; };
    user = lib.mkOption { type = lib.types.str; default = "wt"; };
    group = lib.mkOption { type = lib.types.str; default = "users"; };
    enableAcceleratorService = lib.mkEnableOption "加速器系统服务（同 programs.watt-toolkit）";
    trustCertificate = lib.mkEnableOption "证书信任（同 programs.watt-toolkit）";
    enablePolkit = lib.mkEnableOption "polkit 授权（同 programs.watt-toolkit）";
    proxyMode = lib.mkOption { type = lib.types.enum [ "hosts" "dns" ]; default = "hosts"; };
    serviceName = lib.mkOption { type = lib.types.str; default = "steampp-accelerator"; };
  };

  config = lib.mkIf (cfg.enable || legacyCfg.enable) {
    # 合并：programs 优先，services.steamtools 作为兼容层填充
    programs.watt-toolkit = lib.mkIf legacyCfg.enable {
      package = lib.mkDefault legacyCfg.package;
      user = lib.mkDefault legacyCfg.user;
      group = lib.mkDefault legacyCfg.group;
      enableAcceleratorService = lib.mkDefault legacyCfg.enableAcceleratorService;
      trustCertificate = lib.mkDefault legacyCfg.trustCertificate;
      enablePolkit = lib.mkDefault legacyCfg.enablePolkit;
      proxyMode = lib.mkDefault legacyCfg.proxyMode;
      serviceName = lib.mkDefault legacyCfg.serviceName;
    };

    environment.systemPackages = [ cfg.package ];

    # 系统配置代理模式：主程序启动时读取，决定加速行为（/etc/watt-toolkit/proxy-mode）。
    # NixOS 声明式生成，只读；主程序侧 StartupNixOS 读取该文件。
    environment.etc."watt-toolkit/proxy-mode".text = cfg.proxyMode;

    # NixOS 声明式证书信任。
    # 证书由程序在加速器启动时惰性生成到 AppData（私钥不外流），再把 .cer 导出到
    # /var/lib/watt-toolkit（STEAMTOOLS_BUNDLED_CER，见 package.nix wrapper 与本模块服务环境）。
    # /nix/store 只读，所以这里读的是可写路径；首次启动前文件不存在 → 该条为空，
    # 加速器跑过一次后再 nixos-rebuild switch，证书才会进入系统信任库。
    # 证书有效期 300 天，续期后同样需要再 rebuild 一次。
    security.pki.certificates = lib.mkIf cfg.trustCertificate (
      let
        certPath = "/var/lib/watt-toolkit/SteamTools.Certificate.cer";
      in
      lib.optional (builtins.pathExists certPath) (builtins.readFile certPath)
    );

    # 证书导出目录必须可写且属主为服务用户（程序以 cfg.user 身份运行）。
    # tmpfiles 条目在下方与 dns 模式的 /run 目录合并定义；
    # 系统切换世代不重启时也要靠 activationScript 兜底。
    system.activationScripts.watt-toolkit-cert-dir = lib.mkIf (cfg.enableAcceleratorService || cfg.trustCertificate) ''
      mkdir -p /var/lib/watt-toolkit
      chown ${cfg.user}:${cfg.group} /var/lib/watt-toolkit
      chmod 0755 /var/lib/watt-toolkit
    '';

    # Firefox 默认不使用系统 CA 库（NSS），需要 enterprise policy 显式 Install。
    # Install 指向系统 CA 合并库 /etc/ssl/certs/ca-certificates.crt：
    # 该文件由 NixOS 从 security.pki.certificates 构建期生成，
    # 包含系统最终信任的全部证书（运行时导出证书 + 用户自定义），rebuild 自动跟随。
    environment.etc."firefox/policies/policies.json" =
      lib.mkIf (cfg.trustCertificate && cfg.trustFirefoxCertificate) {
        text = ''
          {
            "policies": {
              "Certificates": {
                "ImportEnterpriseRoots": true,
                "Install": [ "/etc/ssl/certs/ca-certificates.crt" ]
              }
            }
          }
        '';
      };

    systemd.services.${cfg.serviceName} = lib.mkIf cfg.enableAcceleratorService {
      description = "Steam++ (Watt Toolkit) Accelerator";
      after = [ "network.target" ];
      serviceConfig = {
        Type = "simple";
        User = cfg.user;
        Group = cfg.group;
        # 主程序点击加速时写入：STEAMPP_PIPE / STEAMPP_PID / STEAMPP_MODEL
        EnvironmentFile = "/tmp/steampp-accel.env";
        # 防呆：仅当 UI 已写入参数文件时启动（无 env 时 $STEAMPP_PIPE 等为空 → 加速器 164 循环）
        ExecStartPre = "${pkgs.coreutils}/bin/test -s /tmp/steampp-accel.env";
        Environment = [
          "DOTNET_ROOT=${pkgs.dotnetCorePackages.sdk_11_0}/share/dotnet"
          "STEAMPP_PROXY_MODE=${cfg.proxyMode}"
          # 证书导出目标：程序把 AppData 里运行时生成的 .cer 写到这里，
          # 系统侧 security.pki.certificates 读取同一文件（私钥不离开 AppData）。
          "STEAMTOOLS_BUNDLED_CER=/var/lib/watt-toolkit/SteamTools.Certificate.cer"
        ];
        ExecStart = "${pkgs.dotnetCorePackages.sdk_11_0}/bin/dotnet ${cfg.package.accelerator}/Steam++.Accelerator.dll $STEAMPP_PIPE $STEAMPP_PID $STEAMPP_MODEL";
        # root systemd 在服务进程（主用户身份）上设置 ambient capabilities，
        # 加速器因此可监听 443 而不需要以 root 运行。
        AmbientCapabilities = "CAP_NET_BIND_SERVICE CAP_NET_RAW CAP_NET_ADMIN";
        # 失败多为参数缺失/环境问题（UI 侧），自动重启无意义且刷 start-limit
        Restart = "no";
        RestartSec = 1;
        # 供 dns 模式联动脚本写域名映射（/run/watt-toolkit，属主为服务用户）
        RuntimeDirectory = "watt-toolkit";
      };
      # dns 模式：启动/停止时更新 dnsmasq 域名映射（root oneshot 服务写 /run/dnsmasq.d）
      serviceConfig.ExecStartPost = lib.mkIf (cfg.proxyMode == "dns") [
        "-${pkgs.coreutils}/bin/cp -f /tmp/steampp-domains.conf /run/watt-toolkit/domains.conf"
        "${pkgs.systemd}/bin/systemctl start watt-toolkit-dns-update.service"
      ];
      serviceConfig.ExecStopPost = lib.mkIf (cfg.proxyMode == "dns") [
        "${pkgs.coreutils}/bin/rm -f /run/watt-toolkit/domains.conf"
        "${pkgs.systemd}/bin/systemctl start watt-toolkit-dns-update.service"
      ];
      # 不设置 wantedBy：服务保持空闲，仅当 UI 点击加速模块时才被拉起。
    };

    # dns 模式：root oneshot，把加速器域名列表写入 dnsmasq 配置并 reload
    systemd.services.watt-toolkit-dns-update = lib.mkIf (cfg.enableAcceleratorService && cfg.proxyMode == "dns") {
      description = "Watt Toolkit DNS 映射更新（dnsmasq）";
      after = [ "dnsmasq.service" ];
      serviceConfig = {
        Type = "oneshot";
        User = "root";
      };
      script = "${dnsUpdateScript}";
    };

    # dns 模式：预创建 /run 下的目录（tmpfs 开机清空，dnsmasq conf-dir 与
    # 域名映射目录必须存在，否则 dnsmasq 启动报错/跳过 conf-dir）。
    # tmpfiles 只在开机时运行，nhsw 切换世代不重启系统，所以再加
    # activationScript：每次切换世代都创建，dnsmasq 重启时目录必在。
    systemd.tmpfiles.rules =
      lib.optional (cfg.enableAcceleratorService || cfg.trustCertificate)
        "d /var/lib/watt-toolkit 0755 ${cfg.user} ${cfg.group} -"
      ++ lib.optional (cfg.enableAcceleratorService && cfg.proxyMode == "dns") "d /run/dnsmasq.d 0755 root root -"
      ++ lib.optional (cfg.enableAcceleratorService && cfg.proxyMode == "dns") "d /run/watt-toolkit 0755 root root -";
    system.activationScripts.watt-toolkit-dns-dirs = lib.mkIf (cfg.enableAcceleratorService && cfg.proxyMode == "dns") ''
      mkdir -p /run/dnsmasq.d /run/watt-toolkit
      chmod 0755 /run/dnsmasq.d /run/watt-toolkit
    '';

    # dns 模式：全局 dnsmasq 作为系统解析器（接口无关，多 WiFi/有线通用）
    services.dnsmasq = lib.mkIf (cfg.enableAcceleratorService && cfg.proxyMode == "dns") {
      enable = true;
      # dnsmasq 额外读取 /run/dnsmasq.d/*.conf（root oneshot 服务写入的域名映射）
      settings = {
        conf-dir = "/run/dnsmasq.d,*.conf";
        domain-needed = true;
        bogus-priv = true;
        # 上游转发：选中域名以外的所有查询直连上游 DNS
        server = [ "223.5.5.5" "119.29.29.29" "8.8.8.8" ];
      };
    };

    # dns 模式：系统解析器全局指向 dnsmasq；NetworkManager 不管理 DNS（禁 DHCP 下发）
    networking.nameservers = lib.mkIf (cfg.enableAcceleratorService && cfg.proxyMode == "dns") [ "127.0.0.1" ];
    networking.networkmanager.settings = lib.mkIf (cfg.enableAcceleratorService && cfg.proxyMode == "dns") {
      main.dns = "none";
    };

    # polkit：允许主用户无密码启停加速器服务与 dns 更新服务（主程序以主用户身份执行 systemctl）。
    security.polkit.extraConfig = lib.mkIf cfg.enablePolkit ''
      polkit.addRule(function(action, subject) {
        if (action.id == "org.freedesktop.systemd1.manage-units" &&
            subject.user == "${cfg.user}" &&
            (action.lookup("unit") == "${cfg.serviceName}.service" ||
             action.lookup("unit") == "watt-toolkit-dns-update.service" ||
             action.lookup("unit") == "dnsmasq.service")) {
          return polkit.Result.YES;
        }
      });
    '';
  };
}
