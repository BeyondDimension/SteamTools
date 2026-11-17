// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static class ScriptMapper
{
    public static Script ToEntity(ScriptDTO source) => new()
    {
        Id = source.LocalId,
        Pid = source.Id,
        Order = (int)source.Order,
        Name = source.Name,
        Author = source.AuthorName!,
        Version = source.Version,
        FilePath = source.FilePath,
        CachePath = source.CachePath,
        SourceLink = source.SourceLink,
        DownloadLink = source.DownloadLink,
        UpdateLink = source.UpdateLink,
        Description = source.Describe!,
        MatchDomainNames = source.MatchDomainNames,
        Enable = !source.Disable,
        Icon = source.IconUrl,
        ExcludeDomainNames = source.ExcludeDomainNames,
        RequiredJs = source.RequiredJs,
        IsBuild = source.IsCompile,
    };

    public static ScriptDTO ToDto(Script source) => new()
    {
        LocalId = source.Id,
        Id = source.Pid,
        Order = source.Order,
        Name = source.Name,
        AuthorName = source.Author,
        Version = source.Version,
        FilePath = source.FilePath,
        CachePath = source.CachePath,
        SourceLink = source.SourceLink,
        DownloadLink = source.DownloadLink,
        UpdateLink = source.UpdateLink,
        Describe = source.Description,
        MatchDomainNames = source.MatchDomainNames,
        Disable = !source.Enable,
        IconUrl = source.Icon,
        ExcludeDomainNames = source.ExcludeDomainNames!,
        RequiredJs = source.RequiredJs,
        IsCompile = source.IsBuild,
    };
}