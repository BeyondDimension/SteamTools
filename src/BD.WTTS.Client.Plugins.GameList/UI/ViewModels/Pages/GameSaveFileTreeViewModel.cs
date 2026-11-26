using Avalonia;
using BD.WTTS.GameList.Saves;
using static BD.WTTS.UI.ViewModels.GameSavesPageViewModel;

namespace BD.WTTS.UI.ViewModels;

public sealed class GameSaveFileTreeViewModel(bool hasFiles, bool isScreenshot, Func<IReadOnlyList<GameSaveTreeNode>> build) : ReactiveObject
{
    Branch? root;
    bool isExpanded;
    bool isLoading;

    public bool HasFiles => hasFiles;
    public bool IsLoading => isLoading;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
                return;

            this.RaiseAndSetIfChanged(ref isExpanded, value);
            this.RaisePropertyChanged(nameof(ExpansionLabel));
            if (value && root == null && !isLoading)
                _ = LoadAsync();
        }
    }

    public string ExpansionLabel => IsExpanded ? Strings.GameList_Saves_Collapse : Strings.GameList_Saves_Expand;
    public IReadOnlyList<GameSaveTreeRowViewModel> Rows { get; private set; } = [];

    async Task LoadAsync()
    {
        this.RaiseAndSetIfChanged(ref isLoading, true, nameof(IsLoading));
        try
        {
            var nodes = await Task.Run(build);
            root = new(nodes) { IsExpanded = true };
            RefreshRows();
        }
        catch (Exception ex)
        {
            IsExpanded = false;
            ShowError(Strings.GameList_Saves_FileTreeFailed, ex);
        }
        finally
        {
            this.RaiseAndSetIfChanged(ref isLoading, false, nameof(IsLoading));
        }
    }

    void RefreshRows()
    {
        var rows = new List<GameSaveTreeRowViewModel>();
        root?.AppendRows(rows, depth: 0, isScreenshot, RefreshRows);
        Rows = rows;
        this.RaisePropertyChanged(nameof(Rows));
    }

    sealed class Branch(IReadOnlyList<GameSaveTreeNode> nodes)
    {
        const int pageSize = 100;
        int visibleCount = Math.Min(pageSize, nodes.Count);
        readonly Dictionary<int, Branch> children = [];
        public bool IsExpanded { get; set; }

        public void AppendRows(List<GameSaveTreeRowViewModel> rows, int depth, bool isScreenshot, Action refresh)
        {
            if (!IsExpanded)
                return;

            for (var index = 0; index < visibleCount; index++)
            {
                var node = nodes[index];
                if (!node.IsDirectory)
                {
                    rows.Add(new(node, depth, isScreenshot));
                    continue;
                }

                if (!children.TryGetValue(index, out var child))
                    children.Add(index, child = new(node.Children));
                rows.Add(new(node, depth, false, child.IsExpanded, () =>
                {
                    child.IsExpanded = !child.IsExpanded;
                    refresh();
                }));
                child.AppendRows(rows, depth + 1, isScreenshot, refresh);
            }

            if (visibleCount < nodes.Count)
            {
                var nextCount = Math.Min(pageSize, nodes.Count - visibleCount);
                var message = string.Format(isScreenshot ? Strings.GameList_Saves_ShowMoreScreenshots : Strings.GameList_Saves_ShowMoreFiles,
                    nextCount);
                rows.Add(new(message, depth, () =>
                {
                    visibleCount += Math.Min(pageSize, nodes.Count - visibleCount);
                    refresh();
                }));
            }
        }
    }
}

public sealed class GameSaveTreeRowViewModel
{
    internal GameSaveTreeRowViewModel(GameSaveTreeNode node, int depth, bool showImage, bool isExpanded = false, Action? action = null)
    {
        Name = node.Name;
        FullPath = node.FullPath;
        Indent = GetIndent(depth);
        Size = FormatSize(node.SizeBytes);
        Modified = FormatDate(node.LastWriteUtc);
        IsDirectory = node.IsDirectory;
        IsExpanded = isExpanded;
        ShowImage = showImage;
        Action = action;
    }

    internal GameSaveTreeRowViewModel(string name, int depth, Action action)
    {
        Name = name;
        Indent = GetIndent(depth);
        IsMore = true;
        Action = action;
    }

    public string Name { get; }
    public string? FullPath { get; }
    public Thickness Indent { get; }
    public string? Size { get; }
    public string? Modified { get; }
    public bool IsDirectory { get; }
    public bool IsExpanded { get; }
    public bool IsMore { get; }
    public bool ShowImage { get; }
    public bool IsDocument => !IsDirectory && !ShowImage;
    public string ExpansionLabel => IsExpanded ? Strings.GameList_Saves_Collapse : Strings.GameList_Saves_Expand;
    internal Action? Action { get; }

    static Thickness GetIndent(int depth) => new(Math.Min(depth, 8) * 12, 0, 0, 0);
}
