# Getting started

Choose the library for your scenario. These guides describe the current source tree, including controls that may not be present in an older published package.

## Common controls and behaviors

Reference `src/RoyalApps.Community.Avalonia.Common/RoyalApps.Community.Avalonia.Common.csproj` from your application when building from a checkout. Match its Avalonia version in your application. If using a published Common package, verify that its version includes the feature you need.

- [AmbientGlowDecorator](./ambient-glow): include its style resource and wrap your content.
- [CompositionRingSpinner](./ring-spinner): show an indeterminate progress ring with an optional track.
- [GridSplitterBehavior](./grid-splitter): enable the attached property on an existing splitter; no style include is required.

## Windows Forms hosting

Install the Windows package in your Windows application project:

```sh
dotnet add package RoyalApps.Community.Avalonia.Windows
```

The current source targets `net10.0-windows`. Enable Windows Forms in the application project:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

Follow the [WinForms hosting guide](./winforms-hosting) to declare the generic host and implement its disposal contract. Keep this reference in your Windows application project when sharing other application code across platforms.

## Build from source

From the repository root, with the .NET 10 SDK installed:

```sh
dotnet build src/RoyalApps.Community.Avalonia.Common/RoyalApps.Community.Avalonia.Common.csproj
```

Build the Windows library on Windows:

```sh
dotnet build src/RoyalApps.Community.Avalonia.Windows/RoyalApps.Community.Avalonia.Windows.csproj
```

See the [support matrix](./support-matrix) for platform boundaries.
