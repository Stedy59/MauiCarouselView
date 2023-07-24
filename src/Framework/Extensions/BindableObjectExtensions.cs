using Microsoft.Maui.Controls;

namespace StedySoft.Maui.Framework {

	#region Class BindableObjectExtensions
	public static class BindableObjectExtensions {

		public static TBindable Bind<TBindable>(
			this TBindable bindable,
			BindableProperty targetProperty,
			string path = Binding.SelfPath,
			BindingMode mode = BindingMode.Default,
			IValueConverter? converter = null,
			object? converterParameter = null,
			string? stringFormat = null,
			object? source = null,
			object? targetNullValue = null,
			object? fallbackValue = null) where TBindable : BindableObject {
			bindable.SetBinding(
				targetProperty,
				new Binding(path, mode, converter, converterParameter, stringFormat, source) {
					TargetNullValue = targetNullValue,
					FallbackValue = fallbackValue
				});

			return bindable;
		}

	}
	#endregion

}
