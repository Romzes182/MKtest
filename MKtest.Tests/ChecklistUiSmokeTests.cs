using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;
using System.Threading;
using System.Windows.Forms;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistUiSmokeTests
{
    [TestMethod]
    public void MoreMenu_RemainsUsableUntilTheChecklistTabIsDisposed()
    {
        Exception? error = null;
        bool menuWasReused = false;
        bool disposedWithTab = false;

        var thread = new Thread(() =>
        {
            try
            {
                var tab = new ChecklistTabController();
                try
                {
                    var button = tab.Controls.Find("ChecklistMoreActions", true)
                        .OfType<Button>().Single();
                    var menu = button.ContextMenuStrip;
                    Assert.IsNotNull(menu, "The button should own one reusable context menu.");
                    Assert.IsFalse(menu.IsDisposed);
                    menu.Close();
                    Assert.IsFalse(menu.IsDisposed,
                        "Closing the menu must not dispose it during WinForms message handling.");
                    menuWasReused = ReferenceEquals(menu, button.ContextMenuStrip);
                }
                finally
                {
                    var menu = tab.Controls.Find("ChecklistMoreActions", true)
                        .OfType<Button>().Single().ContextMenuStrip;
                    tab.Dispose();
                    disposedWithTab = menu?.IsDisposed == true;
                }
            }
            catch (Exception ex) { error = ex; }
        }) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)));
        if (error != null) Assert.Fail(error.ToString());
        Assert.IsTrue(menuWasReused);
        Assert.IsTrue(disposedWithTab);
    }

    [TestMethod]
    public void ChecklistTab_InitializesAndLaysOutOnStaThread()
    {
        // Windows Forms controls must be created on an STA thread.
        Exception? error = null;
        int checkCount = 0;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form { Width = 1100, Height = 780 };
                using var tab = new ChecklistTabController();
                form.Controls.Add(tab);
                form.CreateControl();
                tab.CreateControl();
                form.PerformLayout();
                tab.PerformLayout();
                checkCount = tab.Controls.Count;
            }
            catch (Exception ex)
            {
                error = ex;
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)),
            "ChecklistTab construction exceeded 15 seconds.");
        if (error != null)
            Assert.Fail("Checklist tab must open without crashing: " + error);
        Assert.IsTrue(checkCount > 0);
    }
}
