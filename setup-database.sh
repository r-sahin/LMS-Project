#!/bin/bash

# LMS Project - Database Setup Script

echo "========================================="
echo "LMS Project - Veritabanı Kurulum Script"
echo "========================================="
echo ""

# API klasörüne git
cd src/LMS-Project.API

echo "1. Migration oluşturuluyor..."
dotnet ef migrations add InitialCreate --project ../LMS-Project.Infrastructure --startup-project . --context ApplicationDbContext

if [ $? -eq 0 ]; then
    echo "✅ Migration başarıyla oluşturuldu!"
    echo ""

    echo "2. Veritabanı güncelleniyor..."
    dotnet ef database update --project ../LMS-Project.Infrastructure --startup-project . --context ApplicationDbContext

    if [ $? -eq 0 ]; then
        echo "✅ Veritabanı başarıyla oluşturuldu!"
        echo ""
        echo "========================================="
        echo "Kurulum tamamlandı!"
        echo "========================================="
        echo ""
        echo "Uygulamayı başlatmak için:"
        echo "dotnet run"
        echo ""
    else
        echo "❌ Veritabanı güncellenirken hata oluştu!"
        exit 1
    fi
else
    echo "❌ Migration oluşturulurken hata oluştu!"
    exit 1
fi
