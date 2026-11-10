# Установщик MKtest — Visual Studio Setup Project

`MKtest.Setup.vdproj` — проект расширения
[Microsoft Visual Studio Installer Projects](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2022InstallerProjects).
Он включён в `MKtest.sln`. Версия 1.0.1 предназначена для **Windows 7 SP1 x64
и более новых Windows с .NET Framework 4.8**.

Инструкция для устройства: [INSTALL-WINDOWS7.md](INSTALL-WINDOWS7.md).

## Состав и требования

- MKtest, управляемые DLL и все файлы `DemoScenarios`, `JsonTemplates`, `Routes`.
- Системный **.NET Framework 4.8** устанавливается отдельно. Встроенного .NET 9
  больше нет. `setup.exe` проверяет Framework и предлагает загрузить его с сайта
  Microsoft. При отсутствии Интернета сначала установите Framework автономно.
- MSI проверяет Windows 7 SP1 x64 или новее и наличие .NET Framework 4.8.
  Framework 4.8.1 на Windows 7 установить нельзя.
- Ярлыки на рабочем столе и в меню «Пуск» с рабочей папкой приложения.
- Установка для текущего пользователя в `%LOCALAPPDATA%\Programs\MKtest`.
  Папка должна быть доступна для записи: приложение хранит рядом с EXE настройки,
  JSON и журналы. Установка самого Framework требует прав администратора.
- Русский интерфейс мастера. MSI и setup.exe не подписаны сертификатом издателя.

Удаление: «Панель управления → Программы и компоненты → MKtest».
Созданные приложением `config.json` и журналы сохраняются. Файлы, поставляемые MSI,
включая шаблоны JSON и маршруты, удаляются или заменяются при обновлении.

## Сборка в Visual Studio с русским интерфейсом

Разработка и сборка выполняются на современной Windows, а не на Windows 7.

1. Установите Visual Studio 2022 версии 17.12+ либо Visual Studio 2026,
   рабочую нагрузку **Разработка классических приложений .NET**, .NET 9 SDK
   (компилятор C# 13) и .NET Framework 4.8 Developer Pack.
2. В **Расширения → Управление расширениями** установите
   **Microsoft Visual Studio Installer Projects** и перезапустите IDE.
3. Откройте `MKtest.sln`, выберите **Setup | Any CPU**. Приложение собирается x64.
4. У проекта `MKtest` выберите **Опубликовать**, профиль `Setup-win-x64`.
5. У проекта `MKtest.Setup` выберите **Собрать**.
6. Результат в `MKtest.Setup\Release`: `MKtest.Setup.msi` и `setup.exe`.
   Передавайте оба файла вместе с `INSTALL-WINDOWS7.md`.

SDK-проект сохраняет группу **Publish Items**: она включает выходные файлы,
зависимости, `MKtest.exe.config` с перенаправлениями версий сборок и данные.
`PublishProfilePath` указывает на `Properties\PublishProfiles\Setup-win-x64.pubxml`.
Конфигурация `Setup` автоматически подключает профиль. Debug/Release не требуют
расширения: установщик исключён из их автоматической сборки.

## Сборка из PowerShell

На Windows с полной Visual Studio и расширением, из корня репозитория:

```powershell
.\MKtest.Setup\Build-Setup.ps1
```

Скрипт очищает только генерируемую публикацию, публикует через MSBuild выбранной
Visual Studio, включает штатный `DisableOutOfProcBuild` и вызывает `devenv.com`.
Один `dotnet build` не собирает `.vdproj`.
Лог: `artifacts\setup-logs\devenv-build.log`.

Событие после сборки `.vdproj` вызывает `Finalize-Setup.ps1`: проверяет требования
MSI и переносит `RemoveExistingProducts` сразу после `InstallInitialize`, внутрь
транзакции. Это необходимо при переходе с .NET 9: библиотеки для разных платформ
могут иметь одинаковые версии, и позднее удаление сохраняло несовместимые DLL.
Событие выполняется и при сборке из Visual Studio, и через скрипт.

## Проверка

**Actions → MKtest Setup Project → Run workflow** собирает и запускает тесты
на .NET Framework 4.8, собирает установщик, проверяет установленные файлы,
ярлыки, запуск с системным x64 CLR, закрытие и удаление с сохранением config.json.
Результат: артефакт `MKtest-Setup-Windows7-x64-*`.

Необязательный параметр workflow `previous_run_id` позволяет указать запуск
с прежним артефактом `MKtest-Setup-win-x64-*`. Тогда дополнительно проверяются
обновление 1.0.0 → 1.0.1, сохранение config.json, замена всех DLL и удаление .NET 9.

`Test-Setup.ps1` предназначен для чистой Windows VM и отказывается работать
поверх существующей установки. Он запускается через PowerShell 7 в CI.
Этот скрипт не нужен для установки программы на Windows 7.

CI использует Windows Server 2022 с уже установленным Framework. Это не проверка
на Windows 7 и не проверка установки Framework через setup.exe. На целевом
устройстве отдельно проверяются запуск, интерфейс, SSH, протоколы, сценарии,
печать и работа с реальным оборудованием.

## Обновления MSI

У версии 1.0.1 новый `ProductCode`, увеличенный `ProductVersion` и прежний
`UpgradeCode`. `RemovePreviousVersions` включён. Для следующих выпусков
увеличивайте первые три числа версии и меняйте ProductCode, сохраняя UpgradeCode.
`DetectNewerInstalledVersion` запрещает откат поверх более новой версии.
Перед обновлением закрывайте приложение и сохраняйте изменённые шаблоны и маршруты.

Формат `.vdproj` основан на стандартных элементах Installer Projects и
[официальном примере dotnet/samples](https://github.com/dotnet/samples/tree/main/core/workers/windows-service-installer).
