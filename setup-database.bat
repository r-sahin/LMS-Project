@echo off
REM LMS Project - Database Setup Script (Windows)

echo =========================================
echo LMS Project - Veritabani Kurulum Script
echo =========================================
echo.

REM API klasörüne git
cd src\LMS-Project.API

echo 1. Migration olusturuluyor...
dotnet ef migrations add InitialCreate --project ..\LMS-Project.Infrastructure --startup-project . --context ApplicationDbContext

if %ERRORLEVEL% EQU 0 (
    echo Migration basariyla olusturuldu!
    echo.

    echo 2. Veritabani guncelleniyor...
    dotnet ef database update --project ..\LMS-Project.Infrastructure --startup-project . --context ApplicationDbContext

    if %ERRORLEVEL% EQU 0 (
        echo Veritabani basariyla olusturuldu!
        echo.
        echo =========================================
        echo Kurulum tamamlandi!
        echo =========================================
        echo.
        echo Uygulamayi baslatmak icin:
        echo dotnet run
        echo.
    ) else (
        echo Veritabani guncellenirken hata olustu!
        pause
        exit /b 1
    )
) else (
    echo Migration olusturulurken hata olustu!
    pause
    exit /b 1
)

pause
