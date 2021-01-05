using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Livet.Behaviors;

namespace MetroTrilithon.UI.Controls
{
	/// <summary>
	/// クリックされたときに、指定したメソッドを実行する <see cref="Button"/> を表します。
	/// </summary>
	public class CallMethodButton : Button
	{
		static CallMethodButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CallMethodButton), new FrameworkPropertyMetadata(typeof(CallMethodButton)));
		}

		private readonly MethodBinder _binder = new MethodBinder();
		private readonly MethodBinderWithArgument _binderWithArgument = new MethodBinderWithArgument();
		private bool _hasParameter;

		#region MethodTarget 依存関係プロパティ

		public object MethodTarget
		{
			get { return this.GetValue(MethodTargetProperty); }
			set { this.SetValue(MethodTargetProperty, value); }
		}
		public static readonly DependencyProperty MethodTargetProperty =
			DependencyProperty.Register(nameof(MethodTarget), typeof(object), typeof(CallMethodButton), new UIPropertyMetadata(null));

		#endregion

		#region MethodName 依存関係プロパティ

		public string MethodName
		{
			get { return (string)this.GetValue(MethodNameProperty); }
			set { this.SetValue(MethodNameProperty, value); }
		}
		public static readonly DependencyProperty MethodNameProperty =
			DependencyProperty.Register(nameof(MethodName), typeof(string), typeof(CallMethodButton), new UIPropertyMetadata(null));

		#endregion

		#region MethodParameter 依存関係プロパティ

		public object MethodParameter
		{
			get { return this.GetValue(MethodParameterProperty); }
			set { this.SetValue(MethodParameterProperty, value); }
		}
		public static readonly DependencyProperty MethodParameterProperty =
			DependencyProperty.Register(nameof(MethodParameter), typeof(object), typeof(CallMethodButton), new UIPropertyMetadata(null, MethodParameterPropertyChangedCallback));

		private static void MethodParameterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (CallMethodButton)d;
			source._hasParameter = true;
		}

		#endregion

		protected override void OnClick()
		{
			base.OnClick();

			if (string.IsNullOrEmpty(this.MethodName)) return;

			var target = this.MethodTarget ?? this.DataContext;
			if (target == null) return;

			if (this._hasParameter)
			{
				this._binderWithArgument.Invoke(target, this.MethodName, this.MethodParameter);
			}
			else
			{
				this._binder.Invoke(target, this.MethodName);
			}
		}
	}
}
