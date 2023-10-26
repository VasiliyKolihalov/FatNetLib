# FatNetLib - Beta

[![Nuget](https://img.shields.io/badge/nuget-0.1.0-blue)](https://www.nuget.org/packages/FatNetLib/)
[![GitHub Releases](https://img.shields.io/badge/github_releases-0.1.0-blue)](https://github.com/VasiliyKolihalov/FatNetLib/releases/)

FatNetLib is a network framework for building client-server applications using the UDP protocol.
FatNetLib was conceived for use in gamedev and Unity, but can be freely used in other
areas.
FatNetLib has familiar features and concepts from large enterprise frameworks (ASP.NET core, Spring etc.).
FatNetLib uses the [LiteNetLib](https://github.com/RevenantX/LiteNetLib#litenetlib) for transport.

**Attention, this is a beta version.
The basic functionality is implemented, but there may be problems with thread-safety.
If you encounter any problems, please let us know.**

## Features

* Endpoints
* Delivery reliabilities
* Middlewares
* Serialization
* Encryption
* Compression
* Asynchronous API
* Modules

## Get started

To get started, follow the [Getting Started](docs/1-geting-started) instructions.

## Packages

There are the following packages in this repository:

* FatNetLib.Core — the main package of a framework
* FatNetLib.Json — json serialization package
* FatNetLib.MicrosoftLogging — package for logging using a standard logger from Microsoft.
* FatNetLib.UnityLogging — package for logging using a standard logger from Unity.

Nuget packages are available in [Nuget Repository](https://www.nuget.org/packages/FatNetLib/).
Unity packages are available
in [GitHub Releases](https://github.com/VasiliyKolihalov/FatNetLib/releases/).

## Documentation

FatNetLib contains [Essential documentation](docs/2-essentials) with a guide for all features.

## Compatibility

Minimal .NET and .NET Core version is 3.0 (We are using .Net Standard 2.1)
Minimal unity version is 2021.3 LTS

## Development, help and feedback

If you encounter a problem or want to contribute, we will be happy to make positive changes.
You can create an issue on GitHub or write to [our discord server](https://discord.gg/TvqeBK9amN/).
We will be glad if you create any training materials like videos, articles, etc.