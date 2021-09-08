using Android.Content;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Application.Security
{
    /// <summary>
    /// Android[多开/分身]检测
    /// <para>https://github.com/lamster2018/EasyProtector/blob/master/library/src/main/java/com/lahm/library/VirtualApkCheckUtil.java</para>
    /// </summary>
    internal static class VirtualApkCheckUtil
    {
        /// <summary>
        /// 市面上多开应用的包名列表
        /// </summary>
        static readonly string[] virtualPkgs = new[] {
            "com.bly.dkplat", //多开分身本身的包名
            "com.by.chaos", //chaos引擎
            "com.lbe.parallel", //平行空间
            "com.excelliance.dualaid", //双开助手
            "com.lody.virtual", //VirtualXposed，VirtualApp
            "com.qihoo.magic", //360分身大师
        };

        static bool? mCheckByPrivateFilePath;

        /// <summary>
        /// 检测APP私有路径，某些多开软件会包含多开软件的包名
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CheckByPrivateFilePath(Context context)
        {
            if (mCheckByPrivateFilePath.HasValue) return mCheckByPrivateFilePath.Value;
            var filesDirPath = context.FilesDir?.Path;
            if (filesDirPath == null) throw new NullReferenceException("FilesDir.Path");
            mCheckByPrivateFilePath = virtualPkgs
                .Any(x => filesDirPath.Contains(x, StringComparison.OrdinalIgnoreCase));
            return mCheckByPrivateFilePath.Value;
        }

        static bool? mCheckByOriginApkPackageName;

        /// <summary>
        /// 检测原始的包名，多开应用会hook处理getPackageName方法，如果在应用列表里出现了同样的包，那么认为该应用被多开了
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CheckByOriginApkPackageName(Context context)
        {
            if (mCheckByOriginApkPackageName.HasValue) return mCheckByOriginApkPackageName.Value;
            var count = 0;
            try
            {
                var packageName = context.PackageName;
                if (packageName == null) throw new NullReferenceException("context.PackageName");
                var pm = context.PackageManager;
                if (pm == null) throw new NullReferenceException("context.PackageManager");
                var pkgs = pm.GetInstalledPackages(0);
                foreach (var info in pkgs)
                {
                    if (packageName.Equals(info.PackageName, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                    if (count > 1)
                    {
                        mCheckByOriginApkPackageName = true;
                        return mCheckByOriginApkPackageName.Value;
                    }
                }
            }
            catch (Java.Lang.Throwable t)
            {
                t.PrintStackTraceWhenDebug();
            }
            mCheckByOriginApkPackageName = count > 1;
            return mCheckByOriginApkPackageName.Value;
        }

        static bool? mCheckByMultiApkPackageName;

        /// <summary>
        /// 运行被克隆的应用，该应用会加载多开应用的so库，检测已经加载的so里是否包含这些应用的包名
        /// </summary>
        /// <returns></returns>
        public static bool CheckByMultiApkPackageName()
        {
            if (mCheckByMultiApkPackageName.HasValue) return mCheckByMultiApkPackageName.Value;
            try
            {
                var reader = new Java.IO.FileReader("/proc/self/maps");
                using var bufferedReader = new Java.IO.BufferedReader(reader);
                string? line;
                while ((line = bufferedReader.ReadLine()) != null)
                {
                    var hasContains = virtualPkgs
                        .Any(x => line.Contains(x, StringComparison.OrdinalIgnoreCase));
                    if (hasContains)
                    {
                        mCheckByMultiApkPackageName = true;
                        return mCheckByMultiApkPackageName.Value;
                    }
                }
            }
            catch (Java.Lang.Throwable t)
            {
                t.PrintStackTraceWhenDebug();
            }
            mCheckByMultiApkPackageName = false;
            return mCheckByMultiApkPackageName.Value;
        }

        static class CommandUtil
        {
            static string? GetStrFromBufferInputSteam(Java.IO.BufferedInputStream bufferedInputStream)
            {
                if (bufferedInputStream == null) return string.Empty;
                var BUFFER_SIZE = 512;
                var buffer = new byte[BUFFER_SIZE];
                try
                {
                    var result = new StringBuilder();
                    while (true)
                    {
                        int read = bufferedInputStream.Read(buffer);
                        if (read > 0)
                        {
                            result.Append(new Java.Lang.String(buffer, 0, read).ToString());
                        }
                        if (read < BUFFER_SIZE)
                        {
                            break;
                        }
                    }
                    return result.ToString();
                }
                catch (Java.Lang.Throwable t)
                {
                    t.PrintStackTraceWhenDebug();
                    return null;
                }
            }

            public static string? Exec(string command)
            {
                var runtime = Java.Lang.Runtime.GetRuntime();
                if (runtime == null) throw new NullReferenceException("Java.Lang.Runtime.GetRuntime");
                try
                {
                    var command2 = new Java.Lang.String(command);
                    using var process = runtime.Exec("sh");
                    if (process != null)
                    {
                        using var bufferedOutputStream = new Java.IO.BufferedOutputStream(process.OutputStream);
                        using var bufferedInputStream = new Java.IO.BufferedInputStream(process.InputStream);
                        bufferedOutputStream.Write(command2.GetBytes());
                        bufferedOutputStream.Write('\n');
                        bufferedOutputStream.Flush();
                        bufferedOutputStream.Close();
                        process.WaitFor();
                        var outputStr = GetStrFromBufferInputSteam(bufferedInputStream);
                        return outputStr;
                    }
                }
                catch (Java.Lang.Throwable t)
                {
                    t.PrintStackTraceWhenDebug();
                }
                return null;
            }
        }

        static string? GetUidStrFormat()
        {
            var filter = CommandUtil.Exec("cat /proc/self/cgroup");
            if (string.IsNullOrWhiteSpace(filter)) return null;
#pragma warning disable CS8604 // 可能的 null 引用参数。
            var filter2 = filter.ToJavaString();
#pragma warning restore CS8604 // 可能的 null 引用参数。
            int uidStartIndex = filter2.LastIndexOf("uid");
            int uidEndIndex = filter2.LastIndexOf("/pid");
            if (uidStartIndex < 0) return null;
            if (uidEndIndex <= 0) uidEndIndex = filter2.Length();
            filter2 = filter2.Substring(uidStartIndex + 4, uidEndIndex).ToJavaString();
            try
            {
                var strUid = filter2.ReplaceAll("\n", "");
                var uid = strUid.TryParseInt32();
                if (uid.HasValue)
                {
                    return Java.Lang.String.Format("u0_a%d", uid - 10000);
                }
            }
            catch (Java.Lang.Throwable t)
            {
                t.PrintStackTraceWhenDebug();
            }
            return null;
        }

        static bool? mCheckByHasSameUid;

        /// <summary>
        /// Android系统一个app一个uid，如果同一uid下有两个进程对应的包名，在"/data/data"下有两个私有目录，则该应用被多开了
        /// </summary>
        /// <returns></returns>
        public static bool CheckByHasSameUid()
        {
            if (mCheckByHasSameUid.HasValue) return mCheckByHasSameUid.Value;
            var filter = GetUidStrFormat();
            if (string.IsNullOrWhiteSpace(filter)) return false;
            var result = CommandUtil.Exec("ps");
            if (string.IsNullOrWhiteSpace(result)) return false;
#pragma warning disable CS8602 // 取消引用可能出现的空引用。
            var lines = result.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
#pragma warning restore CS8602 // 取消引用可能出现的空引用。
            if (lines == null || lines.Length <= 0) return false;
            var exitDirCount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    var pkgStartIndex = lines[i].LastIndexOf(" ");
                    var processName = lines[i].JavaSubstring(
                        pkgStartIndex <= 0 ? 0 : pkgStartIndex + 1, lines[i].Length);
                    using var dataFile = new Java.IO.File($"/data/data/{processName}");
                    if (dataFile.Exists())
                    {
                        exitDirCount++;
                    }
                }
                if (exitDirCount > 1)
                {
                    mCheckByHasSameUid = true;
                    return mCheckByHasSameUid.Value;
                }
            }
            mCheckByHasSameUid = exitDirCount > 1;
            return mCheckByHasSameUid.Value;
        }

        static volatile Android.Net.LocalServerSocket? localServerSocket;

        public static bool CheckByCreateLocalServerSocket(string uniqueMsg)
        {
            if (localServerSocket != null) return false;
            try
            {
                localServerSocket = new Android.Net.LocalServerSocket(uniqueMsg);
                return false;
            }
            catch (Java.IO.IOException ex)
            {
                ex.PrintStackTraceWhenDebug();
                return true;
            }
        }

        static bool? mCheckByCreateLocalServerSocket;

        public static bool CheckByCreateLocalServerSocket(Context context)
        {
            if (mCheckByCreateLocalServerSocket.HasValue) return mCheckByCreateLocalServerSocket.Value;
            var packageName = context.PackageName;
            if (packageName == null) throw new NullReferenceException("context.PackageName");
            mCheckByCreateLocalServerSocket = CheckByCreateLocalServerSocket(packageName);
            return mCheckByCreateLocalServerSocket.Value;
        }

        const char O = 'Y';
        const char X = 'N';

        static char ToChar(bool b) => b ? O : X;

        static bool Check(IEnumerable<Func<bool>> checkfuns)
        {
            foreach (var item in checkfuns)
            {
                var result = item?.Invoke() ?? false;
                if (result) return true;
            }
            return false;
        }

        static bool Check(params Func<bool>[] checkfuns) => Check(checkfuns.AsEnumerable());

        static bool Check(Context context, params Func<Context, bool>[] checkfuns)
            => Check(checkfuns.Select<Func<Context, bool>, Func<bool>>(
                x => () => x?.Invoke(context) ?? false));

        public static bool Check(Context context)
        {
            return
                    Check(context, CheckByPrivateFilePath, CheckByOriginApkPackageName, CheckByCreateLocalServerSocket) ||
                    Check(CheckByHasSameUid, CheckByMultiApkPackageName);
        }

        public static void GetCheckResult(Context context, StringBuilder b)
        {
            b.Append(ToChar(CheckByPrivateFilePath(context)));
            b.Append(ToChar(CheckByOriginApkPackageName(context)));
            b.Append(ToChar(CheckByHasSameUid()));
            b.Append(ToChar(CheckByMultiApkPackageName()));
            b.Append(ToChar(CheckByCreateLocalServerSocket(context)));
        }

        //public static string GetCheckResult(Context context)
        //{
        //    var chars = new char[] {
        //            ToChar(CheckByPrivateFilePath(context)),
        //            ToChar(CheckByOriginApkPackageName(context)),
        //            ToChar(CheckByHasSameUid()),
        //            ToChar(CheckByMultiApkPackageName()),
        //            ToChar(CheckByCreateLocalServerSocket(context)),
        //        };
        //    return new string(chars);
        //}
    }
}