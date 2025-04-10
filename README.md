<br />
<div align="center">
  <h3 align="center">UnityPeek</h3>

  <p align="center">
    An application to read Unity game data at runtime along with the <a href="https://github.com/CookieLynx/UnityPeekPlugin">UnityPeek BepInEx plugin!</a>
    <br />
    <a href="https://github.com/CookieLynx/UnityPeek/releases">Releases</a>
    ·
    <a href="https://github.com/CookieLynx/UnityPeek/issues">Report Bug</a>
  </p>
</div>



## About

UnityPeek is an application that is used along with the UnityPeek BepInEx plugin to give users the ability to read data such as the unity Hierarchy, Components, and much more of a built Unity game!

<h2>This application requires a <a href="https://github.com/BepInEx/BepInEx">BepInEx</a> plugin to work, so it requires a BepInEx compatible Unity game</h2>

FAQ:
TODO




REST TODO


# Installing

## Both this program (UnityPeek.exe) and the UnityPeekPlugin.dll are required
- Simply download the latest release in <a href="https://github.com/CookieLynx/UnityPeek/releases">Releases</a> unzip and run UnityPeek.exe inside the extracted files
- Follow steps for downloading and installing the <a href="https://github.com/CookieLynx/UnityPeekPlugin">UnityPeek BepInEx plugin</a> on its github page



# Development

## Prerequisites

- **Visual Studio 2022** or later with the following workloads installed:
  - **.NET Desktop Development**
  - **Desktop development with C++** (required for certain dependencies)
- **.NET 8.0 SDK**: Ensure the .NET 8.0 SDK is installed. You can download it from the official [.NET download page](https://dotnet.microsoft.com/download/dotnet/8.0).

## Getting Started

### 1. Clone the Repository

Open a terminal or command prompt and run:

```bash
git clone https://github.com/CookieLynx/UnityPeek.git
```

### 2. Open the Solution in Visual Studio

- Navigate to the cloned repository directory.

- Open UnityPeek.sln in Visual Studio by double-clicking the file.

### 3. Restore NuGet Packages

- Visual Studio will prompt to restore missing NuGet packages upon opening the solution. Click Restore to install all necessary dependencies.

### 4. Build the Solution

- Set the build configuration to Release:
  - In the toolbar, locate the build configuration dropdown (usually set to Debug by default) and select Release.
- Build the solution:
  - Go to the menu bar and select Build > Build Solution (or press Ctrl+Shift+B).

### 5. Locate the Executable

- After a successful build, the executable (UnityPeek.exe) will be located in:

`
<RepositoryRoot>\bin\Release\net8.0-windows
`

### 6. Run the Application

- Navigate to the directory containing UnityPeek.exe.
- Double-click UnityPeek.exe to launch the application.
