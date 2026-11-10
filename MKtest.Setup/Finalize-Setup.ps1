[CmdletBinding()]
param([Parameter(Mandatory = $true)][string] $MsiPath)

$ErrorActionPreference = 'Stop'

# Use the native MSI API to avoid Automation/indexed-property coercion in build
# hosts. This script runs only on the build machine, not on the user's device.
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

public static class MKtestMsiFinalizer
{
    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiOpenDatabaseW(string path, IntPtr persist, out uint database);
    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiDatabaseOpenViewW(uint database, string query, out uint view);
    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiViewExecute(uint view, uint record);
    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiViewFetch(uint view, out uint record);
    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiRecordGetStringW(uint record, uint field, StringBuilder value, ref uint length);
    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiDatabaseCommit(uint database);
    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiCloseHandle(uint handle);

    private static void Check(uint status, string operation)
    {
        if (status != 0) throw new Win32Exception((int)status, operation + " (MSI error " + status + ")");
    }

    private static List<string> Query(uint database, string query, bool readRows)
    {
        uint view;
        Check(MsiDatabaseOpenViewW(database, query, out view), query);
        try
        {
            Check(MsiViewExecute(view, 0), query);
            var rows = new List<string>();
            if (!readRows) return rows;
            while (true)
            {
                uint record;
                uint status = MsiViewFetch(view, out record);
                if (status == 259) break; // ERROR_NO_MORE_ITEMS
                Check(status, "Fetch: " + query);
                try
                {
                    var value = new StringBuilder(32768);
                    uint length = (uint)value.Capacity;
                    Check(MsiRecordGetStringW(record, 1, value, ref length), "Read: " + query);
                    rows.Add(value.ToString());
                }
                finally { MsiCloseHandle(record); }
            }
            return rows;
        }
        finally { MsiCloseHandle(view); }
    }

    public static void FinalizePackage(string path)
    {
        uint database;
        Check(MsiOpenDatabaseW(path, new IntPtr(1), out database), "Open MSI for transaction");
        try
        {
            var framework = Query(database, "SELECT `Value` FROM `Property` WHERE `Property` = 'VSDFrameworkVersion'", true);
            if (framework.Count != 1 || framework[0].Trim() != "v4.8")
                throw new InvalidOperationException("The MSI must require .NET Framework 4.8.");
            var conditions = Query(database, "SELECT `Condition` FROM `LaunchCondition`", true);
            const string expectedOs = "Installed OR (VersionNT64 AND ((VersionNT > 601) OR (VersionNT = 601 AND ServicePackLevel >= 1)))";
            if (!conditions.Contains(expectedOs))
                throw new InvalidOperationException("Missing Windows 7 SP1 x64 condition: " + string.Join("; ", conditions));
            var initialize = Query(database, "SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = 'InstallInitialize'", true);
            var previous = Query(database, "SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = 'RemoveExistingProducts'", true);
            if (initialize.Count != 1 || previous.Count != 1)
                throw new InvalidOperationException("Required installer actions are missing.");
            int sequence = int.Parse(initialize[0]) + 1;
            var occupant = Query(database, "SELECT `Action` FROM `InstallExecuteSequence` WHERE `Sequence` = " + sequence, true);
            if (occupant.Count != 0 && occupant[0] != "RemoveExistingProducts")
                throw new InvalidOperationException("The early upgrade sequence is occupied by " + occupant[0]);

            // Framework and modern .NET DLLs can share file versions. Remove the
            // old product inside the transaction, before copying the new payload.
            Query(database, "UPDATE `InstallExecuteSequence` SET `Sequence` = " + sequence + " WHERE `Action` = 'RemoveExistingProducts'", false);
            Check(MsiDatabaseCommit(database), "Commit MSI");
            Console.WriteLine("Verified Windows 7 SP1 / .NET Framework 4.8 requirements; RemoveExistingProducts sequence: " + sequence);
        }
        finally { MsiCloseHandle(database); }
    }
}
'@

[MKtestMsiFinalizer]::FinalizePackage((Resolve-Path -LiteralPath $MsiPath).Path)
