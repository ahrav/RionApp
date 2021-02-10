using System;
using Xamarin.Forms;

namespace Rion
{
   public partial class MainPage
	{
		public MainPage ()
		{
			DependencyService.Get<IScreenManager>().KeepScreenOn();
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		private double _oldWidth, _oldHeight;

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (Math.Abs(width - _oldWidth) < 1 && Math.Abs(height - _oldHeight) < 1) return;
			_oldHeight = height;
			_oldWidth = width;
			if (width > height)
				Content = GetLandscapeView();
			else
				Content = GetPortraitView();
			

		}

		private View GetPortraitView()
		{
			return new PortraitContent();
		}

		private View GetLandscapeView()
		{
			return new LandscapeContent();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			// UnsubscribeToDevice();
		}

	}
}