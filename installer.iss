#define AppVersion "1.0.0"

[Setup]
AppId={{6B1E7C52-3C1A-4F58-9D0B-5A2F4C8E9A11}
AppName=Stable Volume
AppVersion={#AppVersion}
AppPublisher=Dcode9
DefaultDirName={autopf}\Stable Volume
DefaultGroupName=Stable Volume
PrivilegesRequired=lowest
OutputDir=installer-out
OutputBaseFilename=StableVolume-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\StableVolume.exe

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{autoprograms}\Stable Volume"; Filename: "{app}\StableVolume.exe"
Name: "{autodesktop}\Stable Volume"; Filename: "{app}\StableVolume.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked

[Run]
Filename: "{app}\StableVolume.exe"; Description: "Launch Stable Volume"; Flags: nowait postinstall skipifsilent
