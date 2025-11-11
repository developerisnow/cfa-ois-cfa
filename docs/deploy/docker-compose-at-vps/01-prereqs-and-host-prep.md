---
created: 2025-11-11 15:20
updated: 2025-11-11 15:20
type: runbook
sphere: [devops]
topic: [prereqs, host-prep]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [linux, docker, swap]
---

# 01 — Подготовка VPS (Ubuntu) и Docker

Аппаратные требования (dev)
- [ ] CPU 2 vCPU+
- [ ] RAM 2–4 ГБ (на 2 ГБ добавить swap, см. ниже)
- [ ] Диск 20+ ГБ

Проверка ОС и ресурсов
- [ ] `uname -a`
- [ ] `df -hT`
- [ ] `free -m`

Установка Docker + Compose
- [ ] ```bash
  curl -fsSL https://get.docker.com | sh
  ```
- [ ] Проверка версий: `docker --version && docker compose version`

Swap 2 ГБ (для стабильной сборки .NET/Node)
- [ ] ```bash
  sudo fallocate -l 2G /swapfile || sudo dd if=/dev/zero of=/swapfile bs=1M count=2048
  sudo chmod 600 /swapfile
  sudo mkswap /swapfile
  sudo swapon /swapfile
  echo "/swapfile none swap sw 0 0" | sudo tee -a /etc/fstab
  free -m
  ```

Сетевые порты (если нужен внешний доступ)
- [ ] На сервере UFW может быть выключен (ок): `sudo ufw status`
- [ ] Часто блок на стороне провайдера: открыть TCP 55000/1/5/6/7/8, 58080, 59000/59001, 55432, 59092, 52181
- [ ] Альтернатива — SSH‑туннели (см. раздел 07 и 09)

