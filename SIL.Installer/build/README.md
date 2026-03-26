# SIL.Installer

Reusable WiX 3 installer components for SIL Windows applications.

## Package Contents

| Package path | Description |
|---|---|
| `build/Analytics.wxs` | WiX fragment: analytics properties, registry components, privacy dialog, property-set custom actions |
| `build/AnalyticsNavigation.wxs` | Optional WiX fragment: standard LicenseAgreementDlg → PrivacyDlg → VerifyReadyDlg navigation |
| `build/SIL.Installer.targets` | Auto-imported MSBuild targets file |
| `lib/net48/SIL.Installer.dll` | Empty placeholder DLL (satisfies NuGet NU5017; not referenced by your project) |

## Usage

### 1. Reference the package

Add a `PackageReference` to your `.wixproj`:

```xml
<PackageReference Include="SIL.Installer" Version="..." />
```

The `SIL.Installer.targets` file is imported automatically. It:
- Adds `Analytics.wxs` to the WiX compiler's `Compile` items.
- Adds `AnalyticsNavigation.wxs` to `Compile` items (opt out with `SilAnalyticsIncludeNavigation=false`).

### 2. Define required preprocessor variables

In your installer's `.wxs` file (or via `DefineConstants` in the `.wixproj`):

```xml
<?define ProductName = "YourProduct" ?>
<?define ProductAnalyticsRegistryKey = "Software\SIL\YourProduct\Analytics" ?>
```

### 3. Reference the component group

In your product's `Feature` element:

```xml
<Feature Id="ProductFeature" ...>
    <ComponentGroupRef Id="SilAnalyticsComponents" />
    <!-- ... other ComponentRef elements ... -->
</Feature>
```

### 4. Choose a compatible WiX UI set

`AnalyticsNavigation.wxs` is included automatically and wires `LicenseAgreementDlg →
[PrivacyDlg →] VerifyReadyDlg`. It requires a UI set that routes through
`LicenseAgreementDlg`; `WixUI_Minimal` is not supported.

### 5. Customize dialog navigation (optional)

The built-in navigation intentionally bypasses any intermediate dialog the UI set normally
shows between `LicenseAgreementDlg` and `VerifyReadyDlg` (e.g. `CustomizeDlg` in
`WixUI_FeatureTree`, `InstallDirDlg` in `WixUI_InstallDir`). If you need those dialogs to
appear, or if you are wiring navigation yourself for any other reason, disable the built-in
navigation and add your own `<Publish>` elements connecting to `PrivacyDlg`:

```xml
<!-- in your .wixproj -->
<SilAnalyticsIncludeNavigation>false</SilAnalyticsIncludeNavigation>
```

## Build Requirements

- **WiX Toolset v3.14** must be installed on the build machine.
  - Default path: `%ProgramFiles(x86)%\WiX Toolset v3.14\SDK\`
- CI: install via `choco install wixtoolset --version=3.14.1 -y`
