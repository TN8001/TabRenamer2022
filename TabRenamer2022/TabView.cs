using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell.Interop;

namespace TabRenamer2022;

public class TabView
{
    private static readonly Dictionary<IVsWindowFrame, TabView> cache = new();
    private static IReadOnlyDictionary<string, string> names;

    private ToolWindowView twView => frame.GetToolWindowView();
    private readonly IVsWindowFrame frame;
    public readonly string originalName;
    private string newName;


    private TabView(IVsWindowFrame frame)
    {
        this.frame = frame ?? throw new ArgumentNullException(nameof(frame));

        var defaultName = frame.GetCaption();
        var title = frame.GetToolWindowView().GetShortTitle();

        var name = string.IsNullOrEmpty(defaultName) ? title : defaultName;
        var index = name.IndexOf(" - ");
        originalName = index < 0 ? name : name.Substring(0, index);

        if (names.ContainsKey(originalName))
            SetName(names[originalName]);
    }

    public static void AddOrUpdate(IVsWindowFrame frame)
    {
        if (frame == null) return;

        if (cache.ContainsKey(frame))
        {
            var tv = cache[frame];
            if (names == null) return;

            if (names.ContainsKey(tv.originalName))
                tv.SetName(names[tv.originalName]);
        }
        else
        {
            try
            {
                cache[frame] = new TabView(frame);
            }
            catch { /* NOP */ }
        }
    }

    public static void Remove(IVsWindowFrame frame)
    {
        if (frame == null) return;

        if (cache.ContainsKey(frame))
            cache.Remove(frame);
    }

    public static void Rename(IReadOnlyDictionary<string, string> names)
    {
        if (names == null) return;

        TabView.names = names;
        foreach (var kv in cache)
            kv.Value.Rename(kv.Key);
    }

    private void Rename(IVsWindowFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));

        //Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        if (names.ContainsKey(originalName))
            SetName(names[originalName]);
        else
            SetName(originalName);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        Debug.WriteLine($"SetName:{name}");

        twView.SetShortTitle(name);
        newName = name;
    }

    public override string ToString() => $"{{{newName}}} [{originalName}]";
}
