using Microsoft.Build.Framework;
using Packaging.Targets.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packaging.Targets
{
    class ArchiveBuilder2 : ArchiveBuilder
    {
        public new void AddFile(string entry, string relativePath, string prefix, List<ArchiveEntry> value, ITaskItem[] metadata)
        {
            base.AddFile(entry, relativePath, prefix, value, metadata);
        }
    }
}
