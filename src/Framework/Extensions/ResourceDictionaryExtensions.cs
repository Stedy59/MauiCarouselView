using Microsoft.Maui.Controls;

namespace StedySoft.Maui.Framework {

	#region Class ResourceDictionaryExtensions
	public static class ResourceDictionaryExtensions {

		public static ControlTemplate TryGetTemplate(this ResourceDictionary resourceDictionary, string key) =>
			(resourceDictionary.TryGetValue(key, out object template) ? template : null).As<ControlTemplate>();

		public static Style TryGetStyle(this ResourceDictionary resourceDictionary, string styleKey) =>
			(resourceDictionary.TryGetValue(styleKey, out object style) ? style : null).As<Style>();

	}
	#endregion

}
