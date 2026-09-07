# SocketSight

SocketSight 是一个用于监控 TCP 连接的本地 Windows 工具。它帮助用户查看活动连接，并了解这些连接涉及哪些应用程序和进程。

## 功能

- 基于规则的本地 TCP 连接监控
- 可配置的连接检查间隔
- 检测到的连接对应的应用和进程信息
- CSV 事件日志
- 包含详细连接信息的清晰事件日志
- 通过预填充的 GitHub Issue 提交由用户审核后的反馈
- 手动检查更新，不自动安装

## 项目状态

SocketSight 是一个持续开发中的个人项目。当前版本改进了界面、事件日志、进程信息以及 release workflow。

当前版本请查看[最新 release](https://github.com/jimmy181211/connection-watcher/releases/latest)。

## 隐私与范围

SocketSight 面向本地使用。它不会自动上传日志，也不会自动安装更新。进程信息取决于 Windows 在检测时能够报告的内容，因此应用与进程之间的归属判断有时可能不完整。

## 技术

- C#
- Windows desktop development
- TCP connection monitoring
- CSV-based event logging

## 项目网站

[gammaacc.com](https://gammaacc.com/)
