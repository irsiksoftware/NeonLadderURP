# NeonLadder Build System

**Implementation of PBI-67: Build & Deploy Menu System**

## Features

### Menu Items (Under `NeonLadder/Build & Deploy/`)

- **Build for Steam** (`Ctrl+Alt+S`) - Creates Windows Steam build
- **Build for itch.io** (`Ctrl+Alt+I`) - Creates Windows + WebGL builds  
- **Run All Tests** (`Ctrl+Alt+T`) - Executes all tests via CLITestRunner

### Automated Features

- **Pre-build test validation** - Optional test runs before builds
- **Build reporting** - Detailed reports saved to `TestOutput/BuildReports/`
- **Build validation** - File size analysis and error checking
- **Steam-specific configurations** - Optimized for Steam platform
- **itch.io multi-platform** - Both Windows and WebGL builds

### Build Output Structure

```
Builds/
├── Steam/
│   └── NeonLadder.exe
└── itch.io/
    ├── Windows/
    │   └── NeonLadder.exe
    └── WebGL/
        └── [WebGL files]
```

### Integration Points

- **CLITestRunner**: Reuses existing test infrastructure
- **Unity BuildPipeline**: Standard Unity build system
- **TestOutput**: Consistent with existing reporting structure

## Usage

1. Select build target from `NeonLadder/Build & Deploy/` menu
2. Choose whether to run pre-build tests (recommended)
3. Wait for build completion
4. Review build report and optionally open build folder

## Technical Notes

- Builds are configured for Steam launch requirements
- WebGL builds optimized for itch.io platform
- All builds include comprehensive error handling
- Build reports saved for CI/CD integration