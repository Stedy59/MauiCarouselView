using System.Collections.Generic;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace StedySoft.Maui.Framework {

	#region Class ViewExtensions
	public static partial class ViewExtensions {

		#region Public Methods
		public static StackBase AddChild(this StackBase stack, IView child) {
			stack.Children.Add(child);
			return stack;
		}

		internal static IMauiContext? FindMauiContext(this Element element, bool fallbackToAppMauiContext = false) {
			if (element is IElement fe && fe.Handler?.MauiContext != null) { return fe.Handler.MauiContext; }
			foreach (Element parent in element.GetParentsPath()) {
				if (parent is IElement parentView && parentView.Handler?.MauiContext != null) {
					return parentView.Handler.MauiContext;
				}
			}
			return fallbackToAppMauiContext ? Application.Current?.FindMauiContext() : default;
		}

		internal static IEnumerable<Element> GetParentsPath(this Element self) {
			Element current = self;
			while (!IsApplicationOrNull(current.RealParent)) {
				current = current.RealParent;
				yield return current;
			}
		}

		internal static bool IsApplicationOrNull(this object? element) =>
			element is null or IApplication;

		internal static bool IsApplicationOrWindowOrNull(this object? element) =>
			element is null or IApplication or IWindow;

		public static IPlatformViewHandler ToHandler(this IView view, IMauiContext context) =>
			ElementExtensions.ToHandler(view, context).CastTo<IPlatformViewHandler>();

		#endregion

	}
	#endregion

}
