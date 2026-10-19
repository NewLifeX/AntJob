# AntJob - Plataforma distribuída de agendamento de tarefas

![GitHub License](https://img.shields.io/github/license/NewLifeX/AntJob?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.AntJob?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.AntJob?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.AntJob?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/AntJob?logo=github)](https://github.com/NewLifeX/AntJob/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Esta tradução pode estar desatualizada em relação à versão em chinês.

AntJob é uma plataforma distribuída de agendamento de tarefas para big data, com despacho em tempo real e offline, cálculo por fragmentos, failover e interface de gestão.

## Instalação

```bash
dotnet add package NewLife.AntJob
```

## Início rápido

```csharp
var setting = new JobSetting { Server = "http://127.0.0.1:5000" };
var client = new JobClient(setting);
client.Start();
```

## Recursos

- Alta taxa de transferência e baixa latência, comprovados em produção
- Integração perfeita com o ecossistema NewLife (XCode/Cube/Remoting)
- Arquitetura extensível e conectável para desenvolvimento próprio

## Documentação

> 📖 Documentação completa (em chinês): [documentos do repositório](Doc)

## Licença

MIT License © 2002-2026 Equipe NewLife

[GitHub](https://github.com/NewLifeX/AntJob) | [Website](https://newlifex.com)
