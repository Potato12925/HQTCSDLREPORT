@echo off
title 🚀 .NET + Frontend Startup
color 0A

echo ====== START BACKEND ======
cd /d %~dp0HQTCSDLREPORT.Server

start "🔧 BACKEND (.NET)" cmd /k "color 0B && echo === .NET Backend === && dotnet run --launch-profile https"

cd ..

echo ====== START FRONTEND ======
cd /d %~dp0HQTCSDLREPORT.Client

start "🌐 FRONTEND" cmd /k "color 0E && echo === Frontend === && npm run dev"

cd ..

echo.
echo ✅ Done - Backend & Frontend are running
pause
