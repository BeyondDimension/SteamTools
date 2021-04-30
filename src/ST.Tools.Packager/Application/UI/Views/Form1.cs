using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ProjectPathUtil;

namespace System.Application.UI.Views
{
    /// <summary>
    /// 发布新版本打包 tar.gz/.tgz 格式，与带进度解压示例
    /// </summary>
    public partial class Form1 : Form
    {
        readonly string app_path = DirPublishWinX86;

        static string GetPath(string path)
        {
            if (Path.DirectorySeparatorChar == '\\') return path;
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = GetPath(projPath + app_path);
        }

        void OnBtnSelectPathClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Path.GetDirectoryName(openFileDialog1.FileName);
            }
        }

        string? filePath = null;

        private void OnBtnCreateClick(object sender, EventArgs e)
        {
            var dirPath = textBox1.Text;
            var parent = Directory.GetParent(dirPath)?.FullName;
            if (parent != null)
            {
                void Create()
                {
                    filePath = Path.Combine(parent, DateTime.Now.ToString("yyyyMMddHHmmssfffffff")) + FileEx.TAR_GZ;
                    TarGZipHelper.Create(filePath, dirPath, progress: ShowTarProgressMessage);
                    Invoke((Action)(() =>
                    {
                        progressBar1.Value = progressBar1.Maximum;
                        button2.Enabled = true;
                    }));
                }
                button2.Enabled = false;
                Task.Factory.StartNew(Create);
            }
            else
            {
                richTextBox1.AppendText("Fail, Parent is null");
                richTextBox1.AppendText(Environment.NewLine);
            }
        }

        void ShowTarProgressMessage(TarArchive archive, TarEntry entry, string message)
        {
            void ShowTarProgressMessage()
            {
                if (entry.TarHeader.TypeFlag != TarHeader.LF_NORMAL && entry.TarHeader.TypeFlag != TarHeader.LF_OLDNORM)
                {
                    richTextBox1.AppendText("Entry type " + (char)entry.TarHeader.TypeFlag + " found!");
                    richTextBox1.AppendText(Environment.NewLine);
                }
                richTextBox1.AppendText(entry.Name + " " + message);
                richTextBox1.AppendText(Environment.NewLine);
                string modeString = DecodeType(entry.TarHeader.TypeFlag, entry.Name.EndsWith("/")) + DecodeMode(entry.TarHeader.Mode);
                string userString = (string.IsNullOrEmpty(entry.UserName)) ? entry.UserId.ToString() : entry.UserName;
                string groupString = (string.IsNullOrEmpty(entry.GroupName)) ? entry.GroupId.ToString() : entry.GroupName;
                richTextBox1.AppendText(string.Format("{0} {1}/{2} {3,8} {4:yyyy-MM-dd HH:mm:ss}", modeString, userString, groupString, entry.Size, entry.ModTime.ToLocalTime()));
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            Invoke((Action)ShowTarProgressMessage);
        }

        static string DecodeType(int type, bool slashTerminated)
        {
            string result = "?";
            switch (type)
            {
                case TarHeader.LF_OLDNORM:       // -jr- TODO this decoding is incomplete, not all possible known values are decoded...
                case TarHeader.LF_NORMAL:
                case TarHeader.LF_LINK:
                    if (slashTerminated)
                        result = "d";
                    else
                        result = "-";
                    break;

                case TarHeader.LF_DIR:
                    result = "d";
                    break;

                case TarHeader.LF_GNU_VOLHDR:
                    result = "V";
                    break;

                case TarHeader.LF_GNU_MULTIVOL:
                    result = "M";
                    break;

                case TarHeader.LF_CONTIG:
                    result = "C";
                    break;

                case TarHeader.LF_FIFO:
                    result = "p";
                    break;

                case TarHeader.LF_SYMLINK:
                    result = "l";
                    break;

                case TarHeader.LF_CHR:
                    result = "c";
                    break;

                case TarHeader.LF_BLK:
                    result = "b";
                    break;
            }

            return result;
        }

        static string DecodeMode(int mode)
        {
            const int S_ISUID = 0x0800;
            const int S_ISGID = 0x0400;
            const int S_ISVTX = 0x0200;

            const int S_IRUSR = 0x0100;
            const int S_IWUSR = 0x0080;
            const int S_IXUSR = 0x0040;

            const int S_IRGRP = 0x0020;
            const int S_IWGRP = 0x0010;
            const int S_IXGRP = 0x0008;

            const int S_IROTH = 0x0004;
            const int S_IWOTH = 0x0002;
            const int S_IXOTH = 0x0001;

            var result = new System.Text.StringBuilder();
            result.Append((mode & S_IRUSR) != 0 ? 'r' : '-');
            result.Append((mode & S_IWUSR) != 0 ? 'w' : '-');
            result.Append((mode & S_ISUID) != 0
                    ? ((mode & S_IXUSR) != 0 ? 's' : 'S')
                    : ((mode & S_IXUSR) != 0 ? 'x' : '-'));
            result.Append((mode & S_IRGRP) != 0 ? 'r' : '-');
            result.Append((mode & S_IWGRP) != 0 ? 'w' : '-');
            result.Append((mode & S_ISGID) != 0
                    ? ((mode & S_IXGRP) != 0 ? 's' : 'S')
                    : ((mode & S_IXGRP) != 0 ? 'x' : '-'));
            result.Append((mode & S_IROTH) != 0 ? 'r' : '-');
            result.Append((mode & S_IWOTH) != 0 ? 'w' : '-');
            result.Append((mode & S_ISVTX) != 0
                    ? ((mode & S_IXOTH) != 0 ? 't' : 'T')
                    : ((mode & S_IXOTH) != 0 ? 'x' : '-'));

            return result.ToString();
        }

        private void OnBtnUnpackClick(object sender, EventArgs e)
        {
            if (filePath != null)
            {
                void Unpack()
                {
                    var dirName = Path.GetFileName(filePath).Split('.').First();
                    TarGZipHelper.Unpack(filePath,
                        Path.Combine(Path.GetDirectoryName(filePath).ThrowIsNull(nameof(filePath)), dirName),
                        ShowTarProgressMessage,
                        new Progress<float>(v =>
                        {
                            void OnReport()
                            {
                                progressBar1.Value = (int)MathF.Floor(v * 100);
                            }
                            Invoke((Action)OnReport);
                        }), maxProgress: 100f);
                    Invoke((Action)(() =>
                    {
                        button3.Enabled = true;
                    }));
                }
                button3.Enabled = false;
                progressBar1.Value = 0;
                progressBar1.Maximum = 10000;
                Task.Factory.StartNew(Unpack);
            }
        }
    }
}