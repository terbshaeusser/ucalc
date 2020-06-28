[Setup]
AppName=MietRechner
AppVersion=2.0
WizardStyle=modern
DefaultDirName={autopf}\MietRechner
DefaultGroupName=MietRechner
UninstallDisplayIcon={app}\ucalc.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=MietRechner Setup
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "../ucalc/bin/Release/netcoreapp3.1/win-x64/publish/**"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\MietRechner"; Filename: "{app}\ucalc.exe"
Name: "{commondesktop}\MietRechner"; Filename: "{app}\ucalc.exe"; IconFilename: "{app}\ucalc.exe"; Tasks: desktopicon
