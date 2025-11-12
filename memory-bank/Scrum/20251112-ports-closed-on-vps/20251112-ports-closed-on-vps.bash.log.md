# Bash log — Proof of listeners vs external reachability

## Client-side probe (from macOS)
```bash
```Bash
prj_Cifra-rwa-exachange-assets (codex/yougile-mcp-export) ❯ for p in 5000 8080 3001 3002 3003; do nc -vz 87.249.49.56 $p; done

Connection to 87.249.49.56 port 5000 [tcp/commplex-main] succeeded!
nc: connectx to 87.249.49.56 port 8080 (tcp) failed: Connection refused
nc: connectx to 87.249.49.56 port 3001 (tcp) failed: Connection refused
nc: connectx to 87.249.49.56 port 3002 (tcp) failed: Connection refused
nc: connectx to 87.249.49.56 port 3003 (tcp) failed: Connection refused
prj_Cifra-rwa-exachange-assets (codex/yougile-mcp-export) ❯ ssh cfa1    Z                                13:46:48
Welcome to Ubuntu 24.04.3 LTS (GNU/Linux 6.8.0-87-generic x86_64)

 * Documentation:  https://help.ubuntu.com
 * Management:     https://landscape.canonical.com
 * Support:        https://ubuntu.com/pro

 System information as of Tue Nov 11 12:13:34 UTC 2025

  System load:  0.0               Processes:             108
  Usage of /:   6.4% of 28.02GB   Users logged in:       1
  Memory usage: 10%               IPv4 address for eth0: 87.249.49.56
  Swap usage:   0%                IPv6 address for eth0: 2a03:6f01:1:2::1:87e1

 * Strictly confined Kubernetes makes edge and IoT secure. Learn how MicroK8s
   just raised the bar for easy, resilient and secure K8s cluster deployment.

   https://ubuntu.com/engage/secure-kubernetes-at-the-edge

Expanded Security Maintenance for Applications is not enabled.

0 updates can be applied immediately.

Enable ESM Apps to receive additional future security updates.
See https://ubuntu.com/esm or run: sudo pro status


Last login: Tue Nov 11 12:18:55 2025 from 88.249.46.132
root@6001289-dq95453:~#
root@6001289-dq95453:~# ss -ltnp | awk 'NR==1||/:((5000|8080|3001|3002|3003)) /'
State  Recv-Q Send-Q Local Address:Port  Peer Address:PortProcess                                                                                                       
LISTEN 0      4096         0.0.0.0:5000       0.0.0.0:*    users:(("docker-proxy",pid=1174296,fd=7))                                                                    
LISTEN 0      4096         0.0.0.0:8080       0.0.0.0:*    users:(("docker-proxy",pid=1225850,fd=7))                                                                    
LISTEN 0      4096            [::]:5000          [::]:*    users:(("docker-proxy",pid=1174300,fd=7))                                                                    
LISTEN 0      4096            [::]:8080          [::]:*    users:(("docker-proxy",pid=1225855,fd=7))                                                                    
root@6001289-dq95453:~# ufw status verbose
Status: inactive
root@6001289-dq95453:~# nft list ruleset | head -n 50
# Warning: table ip nat is managed by iptables-nft, do not touch!
table ip nat {
        chain DOCKER {
                iifname != "br-d04c71645561" tcp dport 9000 counter packets 119 bytes 6592 dnat to 172.19.0.2:9000
                iifname != "br-d04c71645561" tcp dport 59000 counter packets 1 bytes 40 dnat to 172.19.0.2:9000
                iifname != "br-d04c71645561" tcp dport 9001 counter packets 105 bytes 6116 dnat to 172.19.0.2:9001
                iifname != "br-d04c71645561" tcp dport 59001 counter packets 20 bytes 1200 dnat to 172.19.0.2:9001
                iifname != "br-d04c71645561" tcp dport 2181 counter packets 4 bytes 196 dnat to 172.19.0.3:2181
                iifname != "br-d04c71645561" tcp dport 52181 counter packets 0 bytes 0 dnat to 172.19.0.3:2181
                iifname != "br-d04c71645561" tcp dport 5432 counter packets 3111 bytes 185872 dnat to 172.19.0.4:5432
                iifname != "br-d04c71645561" tcp dport 55432 counter packets 0 bytes 0 dnat to 172.19.0.4:5432
                iifname != "br-d04c71645561" tcp dport 9092 counter packets 44 bytes 2512 dnat to 172.19.0.5:9092
                iifname != "br-d04c71645561" tcp dport 59092 counter packets 1 bytes 40 dnat to 172.19.0.5:9092
                iifname != "br-4b71c11ab585" tcp dport 55001 counter packets 6 bytes 368 dnat to 172.18.0.2:8080
                iifname != "br-4b71c11ab585" tcp dport 55006 counter packets 2 bytes 128 dnat to 172.18.0.3:8080
                iifname != "br-4b71c11ab585" tcp dport 55008 counter packets 3 bytes 168 dnat to 172.18.0.4:8080
                iifname != "br-4b71c11ab585" tcp dport 55005 counter packets 0 bytes 0 dnat to 172.18.0.5:8080
                iifname != "br-4b71c11ab585" tcp dport 55003 counter packets 0 bytes 0 dnat to 172.18.0.6:8080
                iifname != "br-4b71c11ab585" tcp dport 55007 counter packets 1 bytes 40 dnat to 172.18.0.7:8080
                iifname != "br-4b71c11ab585" tcp dport 5000 counter packets 1 bytes 64 dnat to 172.18.0.8:8080
                iifname != "br-d04c71645561" tcp dport 8080 counter packets 0 bytes 0 dnat to 172.19.0.6:8080
        }

        chain PREROUTING {
                type nat hook prerouting priority dstnat; policy accept;
                fib daddr type local counter packets 56880 bytes 2633439 jump DOCKER
        }

        chain OUTPUT {
                type nat hook output priority dstnat; policy accept;
                ip daddr != 127.0.0.0/8 fib daddr type local counter packets 0 bytes 0 jump DOCKER
        }

        chain POSTROUTING {
                type nat hook postrouting priority srcnat; policy accept;
                ip saddr 172.19.0.0/16 oifname != "br-d04c71645561" counter packets 2 bytes 117 masquerade
# Warning: table ip filter is managed by iptables-nft, do not touch!
                ip saddr 172.18.0.0/16 oifname != "br-4b71c11ab585" counter packets 0 bytes 0 masquerade
                ip saddr 172.17.0.0/16 oifname != "docker0" counter packets 6402 bytes 398582 masquerade
        }
}
table ip filter {
        chain DOCKER {
                ip daddr 172.19.0.6 iifname != "br-d04c71645561" oifname "br-d04c71645561" tcp dport 8080 counter packets 0 bytes 0 accept
                ip daddr 172.18.0.8 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 1 bytes 64 accept
                ip daddr 172.18.0.7 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 1 bytes 40 accept
                ip daddr 172.18.0.6 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 0 bytes 0 accept
                ip daddr 172.18.0.5 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 0 bytes 0 accept
                ip daddr 172.18.0.4 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 3 bytes 168 accept
                ip daddr 172.18.0.3 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 2 bytes 128 accept
                ip daddr 172.18.0.2 iifname != "br-4b71c11ab585" oifname "br-4b71c11ab585" tcp dport 8080 counter packets 6 bytes 368 accept
                ip daddr 172.19.0.5 iifname != "br-d04c71645561" oifname "br-d04c71645561" tcp dport 9092 counter packets 45 bytes 2552 accept
root@6001289-dq95453:~#
```