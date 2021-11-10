using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TabRenamer2022;

internal static class ToolWindowViewExtensions
{
    public static string GetShortTitle(this ToolWindowView view)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));

        return (view.Title as WindowFrameTitle)?.ShortTitle;
    }

    public static void SetShortTitle(this ToolWindowView view, string title)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));

        if (view.Title is WindowFrameTitle wft)
        {
            BindingOperations.ClearBinding(wft, WindowFrameTitle.ShortTitleProperty);
            wft.ShortTitle = title;
        }
    }
}

internal static class IVsWindowFrameExtensions
{
    public static bool IsToolWindow(this IVsWindowFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));

        return (frame as WindowFrame)?.IsToolWindow ?? false;
    }

    public static ToolWindowView GetToolWindowView(this IVsWindowFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));

        return (frame as WindowFrame)?.FrameView as ToolWindowView;
    }

    public static string GetCaption(this IVsWindowFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));

        ThreadHelper.ThrowIfNotOnUIThread();
        frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out var result);
        return result as string;
    }
}

internal static class DependencyObjectExtensions
{
    public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var count = VisualTreeHelper.GetChildrenCount(obj);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child != null)
                yield return child;
        }
    }

    public static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        foreach (var child in obj.Children())
        {
            yield return child;

            foreach (var grandChild in child.Descendants())
                yield return grandChild;
        }
    }

    public static IEnumerable<T> Children<T>(this DependencyObject obj) where T : DependencyObject
        => obj.Children().OfType<T>();

    public static IEnumerable<T> Descendants<T>(this DependencyObject obj) where T : DependencyObject
        => obj.Descendants().OfType<T>();
}
