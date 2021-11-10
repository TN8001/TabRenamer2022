using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TabRenamer2022;

public class TabNameEventArgs : EventArgs
{
    public IReadOnlyDictionary<string, string> Names { get; }

    public TabNameEventArgs(IReadOnlyDictionary<string, string> names) => Names = names;
}

public class TabMonitor : IVsWindowFrameEvents
{
    private readonly List<WindowFrame> tmpFrames = new();
    private IReadOnlyDictionary<string, string> names;


    public TabMonitor(IReadOnlyDictionary<string, string> names)
    {
        this.names = names;

        Application.Current.MainWindow.ContentRendered += ContentRendered;
        TabView.Rename(names);

        void ContentRendered(object s, EventArgs e)
        {
            Application.Current.MainWindow.ContentRendered -= ContentRendered;
            Initialize();
        }
    }

    public void Rename(IReadOnlyDictionary<string, string> names)
        => TabView.Rename(this.names = names);

    public void ForceRename() => TabView.Rename(names);

    #region IVsWindowFrameEvents
    // タイミングが早すぎて値がほとんど入ってない
    public void OnFrameCreated(IVsWindowFrame frame)
    {
        Debug.WriteLine($"OnFrameCreated:{frame}");

        if (!frame.IsToolWindow()) return;

        tmpFrames.Add((WindowFrame)frame);
    }

    // Windowを開いた時・閉じた（×）時（なにがnewなのか謎）
    // これが一番無駄が少ないイベントだがBindingが取れないことが多く使いづらい
    public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible) => Debug.WriteLine($"OnFrameIsVisibleChanged:{frame}");
  
    // タブが開いた時・閉じた時？（なにがnewなのか謎）
    public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
    {
        Debug.WriteLine($"OnFrameIsOnScreenChanged:{frame}");

        TabView.AddOrUpdate(frame);
    }
    
    // Windowのアクティブ化時（一連のイベントの最後になることが多い（が違うこともある
    public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
    {
        Debug.WriteLine($"OnActiveFrameChanged:{newFrame}");

        // ソリューション エクスプローラーをタブから最初に開いた際に元に戻ってしまう
        // もう面倒なのでしつこいくらいに変更をかけるｗ
        TabView.AddOrUpdate(newFrame);
    }

    // ツールウィンドウはVS終了時のみ？
    public void OnFrameDestroyed(IVsWindowFrame frame)
    {
        Debug.WriteLine($"OnFrameDestroyed:{frame}");

        tmpFrames.Remove((WindowFrame)frame);
        TabView.Remove(frame);
    }
    #endregion

    // 起動時にすでにタブになっているViewを取得
    internal void Initialize()
    {
        var ww = GetAllToolWindowView().ToList();
        Debug.WriteLine($"Initialize  count:{ww.Count}");
        ThreadHelper.ThrowIfNotOnUIThread();

        foreach (var twView in ww)
        {
            var frame = tmpFrames.FirstOrDefault(x => x.FrameView == twView);
            TabView.AddOrUpdate(frame);
        }
    }

    private IEnumerable<ToolWindowView> GetAllToolWindowView()
    {
        var root = Application.Current.MainWindow.Descendants<AutoHideRootControl>().First();
        var l1 = root.Descendants<GroupControlTabItem>().Select(x => x.Content as ToolWindowView);
        var l2 = root.Descendants<AutoHideTabItem>().Select(x => x.Content as ToolWindowView);

        return l1.Concat(l2);
    }
}
