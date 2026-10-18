# Clicker

A small desktop auto clicker built with C#, [Avalonia](https://avaloniaui.net/) and [SharpHook](https://github.com/TolikPylypchuk/SharpHook).

Pick two points on screen, choose how many times to click at each, and Clicker will alternate between them until you press a key to stop.

## Features

- **Capture positions anywhere on screen**: click "Get Position", then click wherever you want, inside or outside the app
- **Two click targets** with a configurable click count for each (1–20)
- **Randomised timing**: a 20–60 ms press and a 200–1000 ms gap between clicks, so clicks aren't perfectly regular
- **Stop with any key**: a global keyboard hook stops the run, even when Clicker isn't focused
- Follows the system light/dark theme

## Usage

1. Click **Get Position** next to Click 1, then click the spot on screen you want to target. The coordinates fill in automatically (you can also enter them manually/adjust them as `x,y`).
2. Do the same for Click 2.
3. Set how many times to click at each position.
4. Press **Start**. Clicker moves to Click 1, clicks the set number of times, moves to Click 2, clicks, and repeats.
5. Press **any key** to stop.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (only needed to build)

## Running from source

```bash
dotnet run --project clicker
```

## Building a Windows .exe

This produces a single self-contained `.exe` that runs without .NET installed. It can be built from macOS or Windows.

```bash
dotnet publish clicker -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Platform notes

### macOS

Global hooks and simulated input need **Accessibility** permission. Grant it to whatever launches the app (Terminal, VS Code, or the app itself) under **System Settings → Privacy & Security → Accessibility**. Without it, position capture and clicking silently do nothing.

### Windows

- **SmartScreen:** the `.exe` isn't code-signed, so Windows may show "Windows protected your PC" on first launch. Choose *More info → Run anyway*.
- **Elevated apps:** Windows blocks input from normal apps to apps running as administrator. If clicks don't register in a particular app, try running Clicker as administrator.

## Project structure

```
clicker/
  Program.cs                 App entry point
  App.axaml(.cs)             Application setup and theme
  Views/
    MainWindow.axaml         UI layout and styles
    MainWindow.axaml.cs      Position capture, click loop, and stop handling
  ViewModels/                MVVM scaffolding (not used yet)
```

## Tech stack

| | |
|---|---|
| Language | C# / .NET 10 |
| UI | Avalonia 12 (Fluent theme, Inter font) |
| Global hooks & input simulation | SharpHook 8 |