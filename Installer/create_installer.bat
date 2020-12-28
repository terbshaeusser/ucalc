@echo off

IF EXIST setup RMDIR /S /Q setup
IF EXIST "MietRechner Setup.exe" DEL "MietRechner Setup.exe"

dotnet publish ..\ucalc.sln --runtime win-x64 --configuration Release --output setup --self-contained true

"C:\Program Files (x86)\Inno Setup 6\iscc.exe" MietRechner.iss
