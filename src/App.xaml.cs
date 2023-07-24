using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

using StedySoft.Maui.Framework;

using StedySoft.CarouselView.Views;

using FontManager = StedySoft.Maui.Framework.FontManager;

namespace StedySoft.CarouselView {

	public partial class App : Application {

		public App() {
			this.InitializeComponent();
			ColorManager.Current.Colors = new Colors();
			FontManager.Current.FontFamily = FontFamilyNames.Urbanist;
			SharedSettings.Current.UseShadows = true;
			this.MainPage = new NavigationPage(new MainPage() {});
		}

		protected override Window CreateWindow(IActivationState? activationState) {
			Window window = base.CreateWindow(activationState);
			if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) {
				window.Width = 600;
			}
			window.Title = AppInfo.Name;
			return window;
		}

	}

}
