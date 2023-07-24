using Microsoft.Maui.Hosting;

using StedySoft.Maui.Framework.Controls;

namespace StedySoft.CarouselView.Framework.Hosting {

	public static partial class AppBuilderExtensions {

		internal static MauiAppBuilder ConfigureControls(this MauiAppBuilder builder) {
			MauiCarouselViewHandler.AppendToMapping();
			return builder;
		}

	}

}
