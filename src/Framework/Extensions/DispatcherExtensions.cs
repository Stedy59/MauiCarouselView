using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace StedySoft.Maui.Framework {

	#region Class DispatcherExtensions
	internal static class DispatcherExtensions {

		public static IDispatcher FindDispatcher(this BindableObject? bindableObject) =>
			bindableObject is not Application && bindableObject is Element element &&
					element.FindMauiContext() is IMauiContext context && context.Services.GetService<IDispatcher>() is IDispatcher handlerDispatcher
				? handlerDispatcher
				: Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher
					? globalDispatcher
					: bindableObject is Application app && app.FindMauiContext() is IMauiContext appMauiContext &&
							appMauiContext.Services.GetService<IDispatcher>() is IDispatcher appHandlerDispatcher
						? appHandlerDispatcher
						: bindableObject is not Application && Application.Current?.Dispatcher is IDispatcher appDispatcher
							? appDispatcher
							: throw new InvalidOperationException("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.");

		public static void DispatchIfRequired(this IDispatcher? dispatcher, Action action) {
			dispatcher = EnsureDispatcher(dispatcher);
			if (dispatcher.IsDispatchRequired) {
				_ = dispatcher.Dispatch(action);
			}
			else {
				action();
			}
		}

		public static Task DispatchIfRequiredAsync(this IDispatcher? dispatcher, Action action) {
			dispatcher = EnsureDispatcher(dispatcher);
			if (dispatcher.IsDispatchRequired) {
				return dispatcher.DispatchAsync(action);
			}
			else {
				action();
				return Task.CompletedTask;
			}
		}

		public static Task DispatchIfRequiredAsync(this IDispatcher? dispatcher, Func<Task> action) {
			dispatcher = EnsureDispatcher(dispatcher);
			return dispatcher.IsDispatchRequired ? dispatcher.DispatchAsync(action) : action();
		}

		static IDispatcher EnsureDispatcher(IDispatcher? dispatcher) =>
			dispatcher is not null
				? dispatcher
				: Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher
					? globalDispatcher
					: Application.Current?.Dispatcher is IDispatcher appDispatcher
						? appDispatcher
						: throw new InvalidOperationException("The dispatcher was not found and the current application does not have a dispatcher.");

	}
	#endregion

}
