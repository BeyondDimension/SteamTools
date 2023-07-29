using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities;

public sealed class TaskItem : ITaskItem
{
    public TaskItem(string itemSpec)
    {
        ItemSpec = itemSpec;
    }

    public TaskItem(string itemSpec, IDictionary<string, string>? metadatas) : this(itemSpec)
    {
        Metadatas = metadatas;
    }

    public string ItemSpec { get; set; }

    public IDictionary<string, string>? Metadatas { get; set; }

    internal const string FullPath = "FullPath";
    internal const string RootDir = "RootDir";
    internal const string Filename = "Filename";
    internal const string Extension = "Extension";
    internal const string RelativeDir = "RelativeDir";
    internal const string Directory = "Directory";
    internal const string RecursiveDir = "RecursiveDir";
    internal const string Identity = "Identity";
    internal const string ModifiedTime = "ModifiedTime";
    internal const string CreatedTime = "CreatedTime";
    internal const string AccessedTime = "AccessedTime";
    internal const string DefiningProjectFullPath = "DefiningProjectFullPath";
    internal const string DefiningProjectDirectory = "DefiningProjectDirectory";
    internal const string DefiningProjectName = "DefiningProjectName";
    internal const string DefiningProjectExtension = "DefiningProjectExtension";

    internal static readonly string[] All =
        {
                FullPath,
                RootDir,
                Filename,
                Extension,
                RelativeDir,
                Directory,
                RecursiveDir,    // <-- Not derivable.
                Identity,
                ModifiedTime,
                CreatedTime,
                AccessedTime,
                DefiningProjectFullPath,
                DefiningProjectDirectory,
                DefiningProjectName,
                DefiningProjectExtension
        };

    public ICollection<string> MetadataNames
    {
        get
        {
            if (Metadatas == null) return All;
            var list = new HashSet<string>(All);
            foreach (var item in Metadatas.Keys)
            {
                list.Add(item);
            }
            return list;
        }
    }

    public string GetMetadata(string metadataName)
    {
        if (Metadatas == null) return null;
        if (Metadatas.ContainsKey(metadataName))
        {
            return Metadatas[metadataName];
        }
        return null;
    }
}
