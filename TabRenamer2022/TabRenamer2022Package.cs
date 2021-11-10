using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TabRenamer2022;

[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "TabRenamer2022", "General", 0, 0, true)]
[ProvideProfile(typeof(OptionsProvider.GeneralOptions), "TabRenamer2022", "General", 0, 0, true)]
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[Guid(PackageGuids.TabRenamer2022String)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class TabRenamer2022Package : ToolkitPackage
{
    private TabMonitor monitor;
    private IVsUIShell7 uiShell7;
    private uint wfeCookie;
    private DTEEvents dteEvents;
    private General general;

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        Debug.WriteLine("InitializeAsync");

        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        General.Saved += OnSettingsSaved;

        general = await General.GetLiveInstanceAsync();

        var dict = general.Apply == UseTabRename.Disable
            ? new()
            : general.TabNames
                     .GroupBy(x => x.From)
                     .Select(x => x.Last())
                     .ToDictionary(x => x.From, x => x.To);

        monitor = new TabMonitor(dict);

        uiShell7 = await GetServiceAsync(typeof(SVsUIShell)) as IVsUIShell7;
        Assumes.Present(uiShell7);
        wfeCookie = uiShell7.AdviseWindowFrameEvents(monitor);

        var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
        Assumes.Present(dte);
        dteEvents = dte.Events.DTEEvents;
        dteEvents.ModeChanged += DteEvents_ModeChanged;

        Debug.WriteLine("InitializeAsync !");
    }

    private void DteEvents_ModeChanged(vsIDEMode LastMode)
    {
        Debug.WriteLine("DteEvents_ModeChanged");

        monitor.ForceRename();
    }

    private void OnSettingsSaved(General obj)
    {
        Debug.WriteLine("OnSettingsSaved");

        var dict = general.Apply == UseTabRename.Disable
            ? new()
            : general.TabNames
                     .GroupBy(x => x.From)
                     .Select(x => x.Last())
                     .ToDictionary(x => x.From, x => x.To);

        monitor.Rename(dict);
    }

    protected override void Dispose(bool disposing)
    {
        Debug.WriteLine("Dispose");

        base.Dispose(disposing);

        ThreadHelper.ThrowIfNotOnUIThread();

        if (disposing && uiShell7 != null)
        {
            General.Saved -= OnSettingsSaved;
            uiShell7.UnadviseWindowFrameEvents(wfeCookie);
            dteEvents.ModeChanged -= DteEvents_ModeChanged;
        }
    }
}
