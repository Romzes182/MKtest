// Visual Studio Installer Projects post-build event (Windows Script Host).
// Framework and modern .NET assets can have identical file versions but
// different bytes. Remove the old product inside the transaction BEFORE copying
// the new payload, so Windows Installer cannot retain incompatible assemblies.
try {
    if (WScript.Arguments.length !== 1) throw new Error("Expected the built MSI path.");
    var installer = new ActiveXObject("WindowsInstaller.Installer");
    var database = installer.OpenDatabase(WScript.Arguments(0), 1); // TRANSACT

    function scalar(query) {
        var view = database.OpenView(query);
        view.Execute();
        var record = view.Fetch();
        var value = record ? String(record.StringData(1)) : null;
        view.Close();
        return value;
    }

    var framework = scalar("SELECT `Value` FROM `Property` WHERE `Property` = 'VSDFrameworkVersion'");
    if (!framework || framework.replace(/\s/g, "") !== "v4.8")
        throw new Error("The MSI must require .NET Framework 4.8.");

    var conditions = database.OpenView("SELECT `Condition` FROM `LaunchCondition`");
    conditions.Execute();
    var condition;
    var hasOsCondition = false;
    while ((condition = conditions.Fetch()) !== null) {
        var text = String(condition.StringData(1));
        if (text === "Installed OR (VersionNT64 AND ((VersionNT > 601) OR (VersionNT = 601 AND ServicePackLevel >= 1)))")
            hasOsCondition = true;
    }
    conditions.Close();
    if (!hasOsCondition) throw new Error("The MSI must require Windows 7 SP1 x64 or later.");

    var initialize = Number(scalar("SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = 'InstallInitialize'"));
    var previous = scalar("SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = 'RemoveExistingProducts'");
    if (!initialize || previous === null) throw new Error("Required installer actions are missing.");
    var sequence = initialize + 1;
    var occupant = scalar("SELECT `Action` FROM `InstallExecuteSequence` WHERE `Sequence` = " + sequence);
    if (occupant !== null && occupant !== "RemoveExistingProducts")
        throw new Error("The early upgrade sequence is already occupied by " + occupant);

    var update = database.OpenView("UPDATE `InstallExecuteSequence` SET `Sequence` = " + sequence +
        " WHERE `Action` = 'RemoveExistingProducts'");
    update.Execute();
    update.Close();
    database.Commit();
    WScript.Echo("Verified Windows 7 SP1 / .NET Framework 4.8 requirements; RemoveExistingProducts sequence: " + sequence);
} catch (error) {
    WScript.Echo("MSI finalization failed: " + error.message);
    WScript.Quit(1);
}
