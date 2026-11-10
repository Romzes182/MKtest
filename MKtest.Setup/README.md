# Установщик MKtest — Visual Studio Setup Project

`MKtest.Setup.vdproj` — проект расширения
[Microsoft Visual Studio Installer Projects](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2022InstallerProjects).
Он включён в `MKtest.sln`. Сборка создаёт `Release/MKtest.Setup.msi`.

## Что устанавливается

- MKtest для Windows x64, зависимости и встроенный .NET 9 Desktop Runtime.
  Отдельная установка .NET на целевом компьютере не требуется.
- Все файлы `DemoScenarios`, `JsonTemplates` и `Routes` с сохранением каталогов.
- Ярлыки **MKtest** на рабочем столе и в меню «Пуск». Рабочая папка ярлыков —
  папка установленного приложения, чтобы `config.json` сохранялся рядом с EXE.
- Установка для текущего пользователя в `%LOCALAPPDATA%\Programs\MKtest`.
  Используйте эту папку: приложение обновляет JSON и журналы рядом с EXE.
- Русский интерфейс мастера. MSI не подписан сертификатом издателя.

Целевая система — Windows 10/11 x64, поддерживающая .NET 9.
Удаление выполняется через «Параметры → Приложения → Установленные приложения».
Созданные приложением настройки и журналы не входят в MSI и сохраняются при удалении.
Файлы, поставляемые самим MSI (включая исходные JSON), удаляются вместе с программой.

## Сборка в Visual Studio с русским интерфейсом

1. Установите Visual Studio 2022 версии 17.12 или новее (либо Visual Studio 2026),
   рабочую нагрузку **Разработка классических приложений .NET** и .NET 9 SDK.
2. Откройте **Расширения → Управление расширениями**, найдите
   **Microsoft Visual Studio Installer Projects**, установите и перезапустите IDE.
3. Откройте `MKtest.sln`, выберите **Release | Any CPU**.
4. В «Обозревателе решений» нажмите правой кнопкой на `MKtest` → **Опубликовать**,
   выберите профиль `Setup-win-x64` и выполните публикацию.
5. Нажмите правой кнопкой на `MKtest.Setup` → **Собрать**.
6. Результат: `MKtest.Setup\Release\MKtest.Setup.msi`.

Для .NET 9 используется группа **Publish Items**, а её `PublishProfilePath`
указывает на `Properties\PublishProfiles\Setup-win-x64.pubxml` в проекте MKtest.
Это [рекомендуемый Microsoft способ упаковки современных .NET-приложений](https://learn.microsoft.com/en-us/visualstudio/deployment/installer-projects-net-core).
Профиль включает runtime и не использует trimming или single-file.

Чтобы обычные сборки приложения и тестов не требовали расширения, проект
установщика исключён из автоматической сборки конфигураций Debug/Release решения.
Его можно собрать явно через контекстное меню либо скрипт ниже.

## Сборка из PowerShell

Из корня репозитория, на Windows с полной Visual Studio и расширением:

```powershell
.\MKtest.Setup\Build-Setup.ps1
```

Скрипт публикует приложение, включает штатный `DisableOutOfProcBuild` для сборки
из командной строки и вызывает `devenv.com`. Один MSBuild или `dotnet build`
не умеет собирать `.vdproj`. Лог: `artifacts\setup-logs\devenv-build.log`.

## GitHub Actions и проверка

В GitHub откройте **Actions → MKtest Setup Project → Run workflow**, выберите
нужную ветку. Workflow запускается вручную и использует Windows runner с
установленным расширением. Он выполняет тесты, собирает MSI, устанавливает его,
сравнивает файлы с публикацией, проверяет ярлыки, запуск со встроенным runtime,
закрытие программы и удаление. MSI доступен в артефакте `MKtest-Setup-win-x64-*`.

`Test-Setup.ps1` предназначен для чистой Windows VM: он отказывается работать,
если папка MKtest или одноимённые ярлыки уже существуют. Проверка не подключается
к реальному оборудованию и не заменяет ручную проверку всех функций приложения.

## Обновление версии

Для новой выпускаемой версии увеличьте `ProductVersion` в свойствах Setup Project
(MSI сравнивает первые три числа), согласитесь на изменение `ProductCode`,
сохраните прежний `UpgradeCode`. `RemovePreviousVersions` включён;
`DetectNewerInstalledVersion` запрещает установку более старой версии поверх новой.
Первый установщик имеет версию `1.0.0`. Проверка обновления между разными версиями
потребует двух собранных пакетов.

Проект использует стандартные элементы формата `.vdproj`, сверенные с
[официальным примером dotnet/samples](https://github.com/dotnet/samples/tree/main/core/workers/windows-service-installer).
