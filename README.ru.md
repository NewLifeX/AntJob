# AntJob - Платформа распределённого планирования заданий

![GitHub License](https://img.shields.io/github/license/NewLifeX/AntJob?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.AntJob?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.AntJob?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.AntJob?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/AntJob/actions/workflows/test.yml)
[![locales](https://img.shields.io/badge/locales-11-2ea44f?logo=dotnet)](Doc/多语言本地化.md)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64%20%7C%20riscv64%20%7C%20s390x-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/AntJob?logo=github)](https://github.com/NewLifeX/AntJob/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Перевод может отставать от китайской версии.

AntJob — распределённая платформа планирования заданий для больших данных: онлайн- и офлайн-распределение, шардированные вычисления, отказоустойчивость и веб-интерфейс.

## Установка

```bash
dotnet add package NewLife.AntJob
```

## Быстрый старт

```csharp
var setting = new JobSetting { Server = "http://127.0.0.1:5000" };
var client = new JobClient(setting);
client.Start();
```

## Возможности

- Высокая пропускная способность и низкая задержка, проверено в продакшене
- Бесшовная интеграция с экосистемой NewLife (XCode/Cube/Remoting)
- Расширяемая модульная архитектура для собственной разработки

## Документация

> 📖 Полная документация (на китайском): [документы репозитория](Doc)

## Лицензия

MIT License © 2002-2026 Команда NewLife

[GitHub](https://github.com/NewLifeX/AntJob) | [Website](https://newlifex.com)
