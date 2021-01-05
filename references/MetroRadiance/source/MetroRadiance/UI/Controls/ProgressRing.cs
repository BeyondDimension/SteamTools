using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	public class ProgressRing : Control
	{
		static ProgressRing()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(ProgressRing),
				new FrameworkPropertyMetadata(typeof(ProgressRing)));
		}

		#region IsActive dependency property

		public bool IsActive
		{
			get { return (bool)this.GetValue(IsActiveProperty); }
			set { this.SetValue(IsActiveProperty, value); }
		}

		public static readonly DependencyProperty IsActiveProperty =
			DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ProgressRing), new PropertyMetadata(true, IsActiveChangedCallback));

		private static void IsActiveChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			((ProgressRing)d).SetActivate((bool)args.NewValue);
		}

		#endregion

		#region EllipseDiameter dependency property

		public int EllipseDiameter
		{
			get { return (int)this.GetValue(EllipseDiameterProperty); }
			set { this.SetValue(EllipseDiameterProperty, value); }
		}

		public static readonly DependencyProperty EllipseDiameterProperty =
			DependencyProperty.Register(nameof(EllipseDiameter), typeof(int), typeof(ProgressRing), new PropertyMetadata(3));

		#endregion

		#region EllipseOffset dependency property

		public Thickness EllipseOffset
		{
			get { return (Thickness)this.GetValue(EllipseOffsetProperty); }
			set { this.SetValue(EllipseOffsetProperty, value); }
		}

		public static readonly DependencyProperty EllipseOffsetProperty =
			DependencyProperty.Register(nameof(EllipseOffset), typeof(Thickness), typeof(ProgressRing), new PropertyMetadata(new Thickness(0, 7, 0, 0)));

		#endregion

		#region MaxSideLength dependency property

		public int MaxSideLength
		{
			get { return (int)this.GetValue(MaxSideLengthProperty); }
			set { this.SetValue(MaxSideLengthProperty, value); }
		}

		public static readonly DependencyProperty MaxSideLengthProperty =
			DependencyProperty.Register(nameof(MaxSideLength), typeof(int), typeof(ProgressRing), new PropertyMetadata(20));

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.SetActivate(true);
			this.SetSize(true);
		}

		private void SetActivate(bool active)
		{
			VisualStateManager.GoToState(this, active ? "Active" : "Inactive", true);
		}

		private void SetSize(bool large)
		{
			VisualStateManager.GoToState(this, large ? "Large" : "Small", true);
		}
	}
}
