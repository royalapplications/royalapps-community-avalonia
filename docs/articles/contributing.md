# Contributing to the docs

The documentation uses VitePress with the default theme, Royal Apps branding, local search, and a guide/API sidebar, following the structure of the [RoyalApps RDP documentation](https://github.com/royalapplications/royalapps-community-rdp/tree/main/docs).

## Local development

Use Node.js 22 or newer and the .NET 10 SDK. From the repository root:

```sh
npm ci
npm run docs:dev
```

Open the local URL printed by VitePress. To validate and preview the production output:

```sh
npm run docs:build
npm run docs:preview
```

The build first compiles the Common and Windows libraries in Release mode and generates their API reference. It then checks internal page links and writes the site to `docs/.vitepress/dist`. The Windows library is cross-targeted on non-Windows hosts; native controls are not executed during generation.

## Content layout

- `docs/articles/`: task-oriented guides
- `docs/api/`: generated public API reference and sidebar (ignored by Git)
- `scripts/generate-api-docs.mjs`: adapted RDP API generator
- `docs/.vitepress/config.mts`: navigation, search, and GitHub Pages base path
- `docs/.vitepress/theme/`: default theme extension
- `docs/public/assets/`: shared site branding
- `docs/assets/`: guide media, including the existing interop demo

## API generation

The generator is adapted from [Community.Rdp](https://github.com/royalapplications/royalapps-community-rdp/blob/main/scripts/generate-api-docs.mjs). It combines compiler XML documentation with C# source inspection, without loading the built assemblies. It preserves member types, parameters, cross-links, related types, and locally resolvable `inheritdoc` comments.

Both `docs:dev` and `docs:build` regenerate the reference. To run generation independently:

```sh
npm run docs:api
npm run docs:test
```

After a successful package restore, generation can run without network access:

```sh
npm run docs:api -- --no-restore
npm exec -- vitepress build docs
```

Update XML documentation in `src/` rather than editing generated pages. The sidebar groups types by library and namespace. Internal implementation types and framework overrides are omitted; protected extension points such as `OnCreateWinFormsControl` are included. Inherited documentation from external framework assemblies is not expanded.

The lightweight source parser follows the RDP generator's file-scoped namespace and declaration conventions. Parser regression tests cover the generic hosts, attached properties, expression-bodied accessors, and multiline declarations used here. If new declaration forms are introduced, update the parser and tests alongside the source.

The generator builds both projects and renders all pages before replacing output. It removes stale Markdown only inside `docs/api/reference/`, and leaves hand-authored guides alone. Generated files are ignored by Git. The README is a short entry point; detailed usage belongs in the guides.

## Package validation and release

The package workflow builds and tests Common, builds Windows, and uploads both
Release packages. Successful pushes to `main` automatically publish both packages
to NuGet.org using the repository's `NUGET_API_KEY` secret. Existing versions are
skipped with `--skip-duplicate`, so a new shared version publishes both packages.
Pull requests only build, test, and upload artifacts. Manual runs on `main` can
retry publication.

Common and Windows share version `1.3.0`, defined once in `src/Directory.Build.props`.
Bump that shared version for each release and build both packages together, even
when only one library changes. Never reuse a published version for different contents.
Produce a local feed with:

```sh
dotnet test src/RoyalApps.Community.Avalonia.Common.Tests -c Release
dotnet pack src/RoyalApps.Community.Avalonia.Common -c Release -o artifacts/packages
dotnet pack src/RoyalApps.Community.Avalonia.Windows -c Release -o artifacts/packages
```

The package contains XML API documentation and the compiled standalone styles.
Restore consumers using this local feed and pin the same version in every
consuming project. When rebuilding an unpublished version, use a fresh validation
package cache to avoid testing a previous archive with the same version.

## GitHub Pages publishing

The site base is `/royalapps-community-avalonia/`. The documentation workflow validates pull requests and relevant pushes to `main`, and uploads the built site as an artifact. Successful runs on `main` automatically deploy to GitHub Pages; pull requests never deploy.

GitHub Pages must use **GitHub Actions** as its source. A manual documentation workflow run on `main` also deploys the site. Deployment uses the `github-pages` environment and its configured approval rules.

The expected site URL is `https://royalapplications.github.io/royalapps-community-avalonia/`. Building this repository locally does not enable Pages or publish the site.
