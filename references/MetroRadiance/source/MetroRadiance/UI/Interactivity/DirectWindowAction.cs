using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using MetroRadiance.UI.Controls;
using MetroRadiance.Utilities;

namespace MetroRadiance.UI.Interactivity
{
	internal class DirectWindowAction : TriggerAction<FrameworkElement>
	{
		#region WindowAction 依存関係プロパティ

		public WindowAction WindowAction
		{
			get { return (WindowAction)this.GetValue(WindowActionProperty); }
			set { this.SetValue(WindowActionProperty, value); }
		}

		public static readonly DependencyProperty WindowActionProperty =
			DependencyProperty.Register("WindowAction", typeof (WindowAction), typeof (DirectWindowAction), new UIPropertyMetadata(WindowAction.Active));

		#endregion

		protected override void Invoke(object parameter)
		{
			this.WindowAction.Invoke(this.AssociatedObject);
		}
	}
}
