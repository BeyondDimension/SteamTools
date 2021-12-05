//using System;
//using System.Collections.Generic;
//using System.Text;
//using Android.Widget;

//// ReSharper disable once CheckNamespace
//namespace System.Text
//{
//    public sealed class ConsoleBuilderA : IConsoleBuilder, IStringBuilder
//    {
//        readonly TextView textView;

//        public int MaxLine { get; set; }

//        public ConsoleBuilderA(TextView textView)
//        {
//            this.textView = textView;
//        }

//        void Append(string? value)
//        {
//            if (!string.IsNullOrEmpty(value))
//            {
//                textView.Append(value);
//            }
//        }

//        IConsoleBuilder IStringBuilder<IConsoleBuilder>.Append(string? value)
//        {
//            Append(value);
//            return this;
//        }

//        IStringBuilder IStringBuilder<IStringBuilder>.Append(string? value)
//        {
//            Append(value);
//            return this;
//        }

//        int IConsoleBuilder.LineCount => textView.LineCount;
//    }
//}
