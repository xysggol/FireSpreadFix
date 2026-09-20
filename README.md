# Fire Spread Fix

A bug-fix mod for [Survivalcraft API](https://gitee.com/SC-SPM/SurvivalcraftApi) 1.9.x.

[中文说明 / Chinese](README.zh-CN.md)

---

## The problem this mod solves

In vanilla (including the plugin/API edition), fire ignores non-flammable blocks when it spreads. If a flammable block is burning, another flammable block up to ~2.5 blocks away has a chance to catch fire **even when a solid, non-flammable block sits directly between them**:

> two wooden planks on both sides of a brick pillar can ignite each other.

This applies to any flammable block, not just planks.

## Root cause

`SubsystemFireBlockBehavior.Update` builds fire-expansion targets from `m_expansionProbabilities`. That table contains offsets of up to ~2.5 blocks (for example `(±2, 0, 0)`, `(0, 2, 0)`), and the result is passed to `SetCellOnFire`.

`SetCellOnFire` only checks whether the target block is flammable. It never checks whether the path between the fire and the target is obstructed, so fire can "tunnel" straight through a wall.

## How it works

Via HarmonyX, this mod replaces `SubsystemFireBlockBehavior.Update` with a prefix that skips the original. The vanilla behaviour is kept (timers, burn-away, particles, sound), and one extra check is added to every probabilistic far-expansion attempt:

- There must be an **unobstructed path** from the fire cell to the target flammable block: a chain of passable cells (air / fire / block 61) whose step count does not exceed the rounded-up Euclidean distance between the two cells.

Resulting behaviour:

| Situation | Vanilla | With this mod |
| --- | --- | --- |
| Flammable block directly behind a 1-block-thick wall | may ignite | does not ignite |
| Sealed diagonal corner (both orthogonal neighbours solid) | may ignite | does not ignite |
| Adjacent flammable block | ignites | ignites |
| Open-air gap / diagonal with an opening / path over the top of a wall | ignites | ignites |

Only the obstruction rule is added. The ignition radius, probabilities and timers are unchanged, so normal fire spread still looks and feels vanilla.

## Compatibility & limitations

- Built and tested against [Survivalcraft API 1.9.3.1](https://gitee.com/SC-SPM/SurvivalcraftApi/releases/tag/API_1.9.3.1).
- Covers every ignition source that goes through the fire subsystem: fire blocks, matches, explosions, incendiary projectiles, magma, etc.
- Adds no blocks, items or entities and stores nothing in the save. `NonPersistentMod` is `true`, so it can be removed at any time without warnings.
- Windows / Linux / Android share a single `.scmod`.

## Installation

1. Download `FireSpreadFix_API193.scmod` from Releases.
2. **PC (Windows / Linux)**: put the `.scmod` into the game's `Mods/` folder.
3. **Android**: push it to `/storage/emulated/0/Survivalcraft2.4_API1.9/Mods`, or simply open the file on the device to install it.

## Build from source

Requires the .NET 10 SDK. The project needs the bundled `nuget.config` to restore `SurvivalcraftAPI.Survivalcraft`.

```bash
dotnet build FireSpreadFix_API193.csproj -c Release
```

On success, `FireSpreadFix_API193.scmod` is generated in `bin/Release/`.

## Releasing (maintainers)

Pushing a tag matching `v*` triggers the workflow in `.github/workflows/release.yml`. It builds the `.scmod` on GitHub's runners and attaches it (plus a `.sha256` checksum) to the GitHub release, so nothing has to be uploaded from your machine:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## License

Released under the **GNU Lesser General Public License v3.0 (LGPL-3.0)**. See [LICENSE](LICENSE). LGPL-3.0 incorporates the terms of the GNU GPL v3.0, whose full text is in [LICENSE.GPL-3.0](LICENSE.GPL-3.0).
