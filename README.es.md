# AntJob - Plataforma distribuida de programación de tareas

![GitHub License](https://img.shields.io/github/license/NewLifeX/AntJob?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.AntJob?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.AntJob?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.AntJob?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/AntJob?logo=github)](https://github.com/NewLifeX/AntJob/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Esta traducción puede estar desactualizada; consulte la versión en chino.

AntJob es una plataforma distribuida de programación de tareas para big data, con despacho en tiempo real y por lotes, cálculo por fragmentos, conmutación por error y gestión visual.

## Instalación

```bash
dotnet add package NewLife.AntJob
```

## Inicio rápido

```csharp
var setting = new JobSetting { Server = "http://127.0.0.1:5000" };
var client = new JobClient(setting);
client.Start();
```

## Características

- Alto rendimiento y baja latencia, probado a escala de producción
- Integración perfecta con el ecosistema NewLife (XCode/Cube/Remoting)
- Arquitectura extensible y conectable para desarrollo propio

## Documentación

> 📖 Documentación completa (en chino): [documentos del repositorio](Doc)

## Licencia

MIT License © 2002-2026 Equipo NewLife

[GitHub](https://github.com/NewLifeX/AntJob) | [Website](https://newlifex.com)
