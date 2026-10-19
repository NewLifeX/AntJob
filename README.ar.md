# AntJob - منصة موزعة لجدولة المهام

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
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md)
<!-- lang:end -->

> قد تتأخر هذه الترجمة عن النسخة الصينية.

AntJob منصة موزعة لجدولة المهام للبيانات الضخمة، تدعم التوزيع الفوري وغير المتصل والحوسبة المُجزأة والتحويل عند الفشل ولوحة إدارة.

## التثبيت

```bash
dotnet add package NewLife.AntJob
```

## البدء السريع

```csharp
var setting = new JobSetting { Server = "http://127.0.0.1:5000" };
var client = new JobClient(setting);
client.Start();
```

## الميزات الرئيسية

- إنتاجية عالية وزمن استجابة منخفض، مُثبت على نطاق الإنتاج
- تكامل سلس مع منظومة NewLife (XCode/Cube/Remoting)
- بنية قابلة للتوسع والإضافات لتطوير مخصص

## التوثيق

> 📖 التوثيق الكامل (بالصينية): [مستندات المستودع](Doc)

## الترخيص

MIT License © 2002-2026 فريق NewLife

[GitHub](https://github.com/NewLifeX/AntJob) | [Website](https://newlifex.com)
