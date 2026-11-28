@echo off
rem YENI SISTEM - programi acar (Windows PowerShell 5.1, ekran icin STA)
powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -File "%~dp0Program.ps1"
if errorlevel 1 pause
