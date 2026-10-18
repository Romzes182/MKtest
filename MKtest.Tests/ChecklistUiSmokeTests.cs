using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;
using System.Threading;
using System.Windows.Forms;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistUiSmokeTests
{
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
        Assert.IsGreaterThan(0, checkCount);
    }
}
