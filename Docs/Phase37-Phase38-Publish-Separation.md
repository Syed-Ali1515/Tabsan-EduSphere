# Phase 37 and Phase 38 Publish Separation Plan

## Goal
Ensure production app publishing excludes:
1. License tooling binaries from app artifacts.
2. Non-runtime repository folders (Docs/PPT/Project startup Docs/Scripts/UAT-SAT docs/User Guide/New Enhancements) from app artifacts.

## Phase 37 - App vs License Publish Separation

Execution script:
- `Scripts/Phase37-Separate-App-And-License-Publish.ps1`

Outputs:
- App publish root: `Artifacts/Phase37/App/`
- License publish root: `Artifacts/Phase37/License/`
- Evidence report: `Artifacts/Phase37/Publish-Separation-20260515.md`

Policy:
- API, Web, and BackgroundJobs publish as one app delivery set.
- Tabsan.Lic publishes as a separate delivery set.
- Packaging paths are separate by default.

## Phase 38 - Non-Runtime Asset Separation

Execution script:
- `Scripts/Phase38-Separate-NonRuntime-Assets.ps1`

Separated folders:
- `Docs`
- `PPT`
- `Project startup Docs`
- `Scripts`
- `UAT-SAT docs`
- `User Guide`
- `New Enhancements`

Outputs:
- Non-runtime package root: `Artifacts/Phase38/NonRuntimeAssets/`
- Evidence report: `Artifacts/Phase38/NonRuntime-Asset-Separation-20260515.md`

Policy:
- These folders are managed and distributed separately from runtime app publish outputs.
- App publish processes must consume only runtime project publish roots under `Artifacts/Phase37/App/`.