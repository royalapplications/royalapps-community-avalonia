# Contributing

Contributions can improve controls, fix bugs, add tests, or make the documentation easier to use. Keep each change focused and include enough context for someone else to understand the problem and verify the result.

## Working on the code

Use the .NET 10 SDK. The component browser runs on Windows, macOS, and Linux. Windows is required only for the Windows Forms integration sample.

The source is organized into:

- `src/RoyalApps.Community.Avalonia.Common/`: reusable, cross-platform controls and behaviors
- `src/RoyalApps.Community.Avalonia.Windows/`: Windows-specific controls and integration
- `src/RoyalApps.Community.Avalonia.Common.Tests/`: control, rendering, and lifetime tests
- `src/RoyalApps.Community.Avalonia.Demo/`: the cross-platform component browser

Follow the conventions of the surrounding code. Keep cross-platform functionality in Common and Windows-specific functionality in Windows. Keep implementation helpers internal and document public APIs with XML comments, including defaults, supported values, and lifetime requirements where relevant.

For control changes, preserve styling and binding behavior. Check attachment and detachment, visibility changes, and disposal of event handlers, subscriptions, and rendering resources. Avoid leaving background work running when a control is hidden or detached.

## Verifying a change

Build the affected libraries and run the relevant tests. From the repository root:

```sh
dotnet build src/RoyalApps.Community.Avalonia.Common -c Release
dotnet test src/RoyalApps.Community.Avalonia.Common.Tests -c Release
```

For Windows integration changes, also build the Windows library and try the affected behavior in the sample application on Windows:

```sh
dotnet build src/RoyalApps.Community.Avalonia.Windows -c Release
dotnet run --project src/RoyalApps.Community.Avalonia.Demo/RoyalApps.Community.Avalonia.Demo
```

Run the demo tests on the host platform, or force the portable build on Windows:

```sh
dotnet run --project src/RoyalApps.Community.Avalonia.Demo.Tests
dotnet build src/RoyalApps.Community.Avalonia.Demo/RoyalApps.Community.Avalonia.Demo -p:DemoWindows=false
dotnet run --project src/RoyalApps.Community.Avalonia.Demo.Tests -p:DemoWindows=false
```

Build or run the demo project directly on macOS/Linux; the full solution also contains Windows-only library projects. `DemoWindows` defaults to true on Windows and false elsewhere. The portable demo excludes Windows source, XAML, and project references. The sidebar still includes the WinForms documentation page.

Add a regression test when fixing a bug and cover new public behavior with tests. For visual changes, check relevant sizes, display scales, themes, and active or inactive states. For lifetime changes, verify repeated attachment and detachment as well as final cleanup.

## Adding or editing documentation

Edit the Markdown guides in `docs/articles/`. Update the relevant guide alongside any change to a control's behavior or public API.

When adding a guide, use a descriptive filename and a clear page title. Explain what the feature does, show a small working C# or XAML example, and describe any important defaults or limitations. Link to related guides so readers can find the next step.

Keep examples self-contained: include the namespaces, styles, and setup needed to use them. Put supporting images in `docs/public/assets/` and give them descriptive alternative text.

Sample-browser media lives in `docs/public/assets/demo/`. Capture the running Windows demo in both Light and Dark themes at its default window size. Use matching `-light` and `-dark` PNG/GIF filenames. The `DemoMedia` component follows the documentation theme toggle, including changes after page load. Capture the UI-thread-blocking comparison on the glow and spinner pages. Keep screenshots of the full client area, crop animations to the live preview, and use concise captions for interaction sequences. GIFs should preserve the observed behavior; the splitter clip demonstrates dragging and Reset, rather than double-tap equalization. Check the README and relevant guide after replacing an asset.

Update API descriptions in the XML comments beside the public members in `src/`. Keep the README brief and link to guides for detailed explanations. For a new feature or guide, include any needed navigation updates in the contribution.

Check spelling, links, and code examples before submitting. Write for someone using the feature for the first time.

## Submitting a contribution

In the pull request, explain the problem, what the change does, and how you verified it. Include reproduction steps for a bug fix and screenshots for visual changes when they help reviewers. Call out any compatibility changes or behavior you could not verify.
