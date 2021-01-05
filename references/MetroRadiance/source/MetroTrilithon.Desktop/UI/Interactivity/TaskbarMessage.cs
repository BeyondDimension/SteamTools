using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Livet.Messaging;

namespace MetroTrilithon.UI.Interactivity
{
	/// <summary>
	/// <see cref="TaskbarMessageAction"/> 経由で <see cref="Window.TaskbarItemInfo"/> を設定するための相互作用メッセージを表します。
	/// </summary>
	public class TaskbarMessage : InteractionMessage
	{
		#region ProgressState Dependency property

		public TaskbarItemProgressState? ProgressState
		{
			get { return (TaskbarItemProgressState?)this.GetValue(ProgressStateProperty); }
			set { this.SetValue(ProgressStateProperty, value); }
		}

		public static readonly DependencyProperty ProgressStateProperty =
			DependencyProperty.Register(nameof(ProgressState), typeof(TaskbarItemProgressState?), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		#region ProgressValue Dependency property

		public double? ProgressValue
		{
			get { return (double?)this.GetValue(ProgressValueProperty); }
			set { this.SetValue(ProgressValueProperty, value); }
		}

		public static readonly DependencyProperty ProgressValueProperty =
			DependencyProperty.Register(nameof(ProgressValue), typeof(double?), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		#region Overlay Dependency property

		public ImageSource Overlay
		{
			get { return (ImageSource)this.GetValue(OverlayProperty); }
			set { this.SetValue(OverlayProperty, value); }
		}

		public static readonly DependencyProperty OverlayProperty =
			DependencyProperty.Register(nameof(Overlay), typeof(ImageSource), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		#region Description Dependency property

		public string Description
		{
			get { return (string)this.GetValue(DescriptionProperty); }
			set { this.SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register(nameof(Description), typeof(string), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		#region ThumbnailClipMargin Dependency property

		public Thickness? ThumbnailClipMargin
		{
			get { return (Thickness?)this.GetValue(ThumbnailClipMarginProperty); }
			set { this.SetValue(ThumbnailClipMarginProperty, value); }
		}

		public static readonly DependencyProperty ThumbnailClipMarginProperty =
			DependencyProperty.Register(nameof(ThumbnailClipMargin), typeof(Thickness?), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		#region ThumbButtonInfos Dependency property

		public ThumbButtonInfoCollection ThumbButtonInfos
		{
			get { return (ThumbButtonInfoCollection)this.GetValue(ThumbButtonInfosProperty); }
			set { this.SetValue(ThumbButtonInfosProperty, value); }
		}

		public static readonly DependencyProperty ThumbButtonInfosProperty =
			DependencyProperty.Register(nameof(ThumbButtonInfos), typeof(ThumbButtonInfoCollection), typeof(TaskbarMessage), new UIPropertyMetadata(null));

		#endregion

		public TaskbarMessage() { }
		public TaskbarMessage(string messageKey) : base(messageKey) { }

		protected override Freezable CreateInstanceCore()
		{
			return new TaskbarMessage
			{
				MessageKey = this.MessageKey,
				ProgressState = this.ProgressState,
				ProgressValue = this.ProgressValue,
				Overlay = this.Overlay,
				Description = this.Description,
				ThumbnailClipMargin = this.ThumbnailClipMargin,
				ThumbButtonInfos = this.ThumbButtonInfos,
			};
		}
	}
}
