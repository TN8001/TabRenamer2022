using System.Collections.Generic;
using System.ComponentModel;
using Community.VisualStudio.Toolkit;

namespace TabRenamer2022;

internal partial class OptionsProvider
{
    public class GeneralOptions : BaseOptionPage<General> { }
}

public enum UseTabRename { Enable, Disable }

public class General : BaseOptionModel<General>
{
    [Category("Tab Renamer"), DisplayName("Apply"), Description("Apply Tab Renamer.")]
    [DefaultValue(UseTabRename.Enable)]
    public UseTabRename Apply { get; set; } = UseTabRename.Enable;

    [Category("Tab Renamer"), DisplayName("Tab Names"), Description("Rename Tab Collections.")]
    public List<TabName> TabNames { get; set; } = new()
    {
        new() { From = "ソリューション エクスプローラー", To = "ソリューション" },
        new() { From = "チーム エクスプローラー", To = "チーム" },
        new() { From = "サーバー エクスプローラー", To = "サーバー" },
        new() { From = "SQL Server オブジェクト エクスプローラー", To = "SQL Server オブジェクト" },
        new() { From = "ソース管理エクスプローラー", To = "ソース管理" },
        new() { From = "タスク ランナー エクスプローラー", To = "タスク ランナー" },
        new() { From = "テスト エクスプローラー", To = "テスト" },
        new() { From = "ライブ プロパティ エクスプローラー", To = "ライブ プロパティ" },
    };
}

public class TabName
{
    public string From { get; set; }
    public string To { get; set; }
    public override string ToString() => $"{From} -> {To}";
}
