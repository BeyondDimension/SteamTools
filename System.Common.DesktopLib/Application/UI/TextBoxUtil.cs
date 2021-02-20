//using Avalonia;
//using Avalonia.Controls;
//using AvaloniaEdit;
//using System.Reflection;

//namespace System.Application.UI
//{
//    /// <summary>
//    /// 文本框工具类
//    /// </summary>
//    public static class TextBoxUtil
//    {
//        #region TextBox Console

//        public interface IConsoleView
//        {
//            TextEditor TbConsole { get; }
//        }

//        public static void WriteLine(this IConsoleView view, string s, bool scrollToEnd = true)
//        {
//            view.TbConsole.AppendText(s);
//            view.TbConsole.AppendText(Environment.NewLine);
//            if (scrollToEnd) view.TbConsole.ScrollToEnd();
//        }

//        public static void Log(this IConsoleView view, string tag, string s, bool scrollToEnd = true)
//        {
//            view.TbConsole.AppendText(DateTime.Now.ToString(DateTimeFormat.Debug2));
//            view.TbConsole.AppendText(" ");
//            view.TbConsole.AppendText(tag);
//            view.TbConsole.AppendText(": ");
//            view.TbConsole.AppendText(s);
//            view.TbConsole.AppendText(Environment.NewLine);
//            if (scrollToEnd) view.TbConsole.ScrollToEnd();
//        }

//        public static void InitConsole(this IConsoleView view, string projectName)
//        {
//            void WriteLine(string s) => view.WriteLine(s, false);
//            var version = Assembly.GetCallingAssembly().GetName().Version?.ToString();
//            UnityConsoleOutputHead.Write(WriteLine, projectName, version);
//            view.TbConsole.ScrollToEnd();
//        }

//        #endregion

//        /// <summary>
//        /// https://github.com/AvaloniaUI/Avalonia/issues/418#issuecomment-201916167
//        /// </summary>
//        /// <param name="textBox"></param>
//        /// <param name="listener"></param>
//        public static void AddTextChangedListener(this TextBox textBox, Action<string> listener)
//            => textBox.GetObservable(TextBox.TextProperty).Subscribe(listener);

//        public static void AddTextChangedListener(this TextEditor textEditor, Action<string> listener)
//            => textEditor.TextChanged += (_, _) => listener(textEditor.Text);
//    }
//}