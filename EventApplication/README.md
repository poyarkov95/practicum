# yandex practicum

## Требования
- [.NET 9.0+ SDK](https://dotnet.microsoft.com/download)
- Любой удобный редактор кода (Visual Studio, VS Code, Rider и т.д.)

---

## Запуск проекта

1. **Перейдите в директорию проекта**:
   
   cd <имя_папки_проекта>   

2. **Восстановите зависимости**:
   dotnet restore

3. **Соберите проект**:
   dotnet build

4. **Запустите проект**:
   dotnet run

5. **Откройте Swagger UI** в браузере:
   http://localhost:5105/swagger/index.html

## Конфигурация
- Порт по умолчанию: `5105` (можно изменить в `Program.cs` или `launchSettings.json`).
- Swagger автоматически настраивается при запуске.
