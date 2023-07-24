using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using UWPPoint = Windows.Foundation.Point;
using UWPSize = Windows.Foundation.Size;

namespace StedySoft.Maui.Framework.Controls {

	#region Class ListViewBaseExtensions
	internal static class ListViewBaseExtensions {

		#region Declarations
		private static UWPPoint Zero = new(0, 0);
		#endregion

		#region Private Methods
		private static bool IsVertical(ScrollViewer scrollViewer) =>
			scrollViewer.HorizontalScrollMode == Microsoft.UI.Xaml.Controls.ScrollMode.Disabled;

		private static UWPPoint AdjustToMakeVisible(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) =>
			IsVertical(scrollViewer)
				? AdjustToMakeVisibleVertical(point, itemSize, scrollViewer)
				: AdjustToMakeVisibleHorizontal(point, itemSize, scrollViewer);

		private static UWPPoint AdjustToMakeVisibleVertical(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) =>
			point.Y > (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight)
				? AdjustToEndVertical(point, itemSize, scrollViewer)
				: point.Y >= scrollViewer.VerticalOffset && point.Y < (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight - itemSize.Height)
					? new UWPPoint(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset)
					: point;

		private static UWPPoint AdjustToMakeVisibleHorizontal(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) =>
			point.X > (scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth)
				? AdjustToEndHorizontal(point, itemSize, scrollViewer)
				: point.X >= scrollViewer.HorizontalOffset && point.X < (scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth - itemSize.Width)
					? new UWPPoint(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset)
					: point;

		private static UWPPoint AdjustToEnd(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) =>
			IsVertical(scrollViewer)
				? AdjustToEndVertical(point, itemSize, scrollViewer)
				: AdjustToEndHorizontal(point, itemSize, scrollViewer);

		private static UWPPoint AdjustToEndHorizontal(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) {
			double adjustment = scrollViewer.ViewportWidth - itemSize.Width;
			return new UWPPoint(point.X - adjustment, point.Y);
		}

		private static UWPPoint AdjustToEndVertical(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) {
			double adjustment = scrollViewer.ViewportHeight - itemSize.Height;
			return new UWPPoint(point.X, point.Y - adjustment);
		}

		private static async Task AdjustToEndAsync(ListViewBase list, ScrollViewer scrollViewer, object targetItem) {
			UWPPoint point = new(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
			if (list.ContainerFromItem(targetItem) is not UIElement targetContainer) { return; }
			point = AdjustToEnd(point, targetContainer.DesiredSize, scrollViewer);
			await JumpToOffsetAsync(scrollViewer, point.X, point.Y);
		}

		private static UWPPoint AdjustToCenter(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) =>
			IsVertical(scrollViewer)
				? AdjustToCenterVertical(point, itemSize, scrollViewer)
				: AdjustToCenterHorizontal(point, itemSize, scrollViewer);

		private static UWPPoint AdjustToCenterHorizontal(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) {
			double adjustment = (scrollViewer.ViewportWidth / 2) - (itemSize.Width / 2);
			return new UWPPoint(point.X - adjustment, point.Y);
		}

		private static UWPPoint AdjustToCenterVertical(UWPPoint point, UWPSize itemSize, ScrollViewer scrollViewer) {
			double adjustment = (scrollViewer.ViewportHeight / 2) - (itemSize.Height / 2);
			return new UWPPoint(point.X, point.Y - adjustment);
		}

		private static async Task AdjustToCenterAsync(ListViewBase list, ScrollViewer scrollViewer, object targetItem) {
			UWPPoint point = new(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
			if (list.ContainerFromItem(targetItem) is not UIElement targetContainer) { return; }
			point = AdjustToCenter(point, targetContainer.DesiredSize, scrollViewer);
			await JumpToOffsetAsync(scrollViewer, point.X, point.Y);
		}

		private static async Task JumpToOffsetAsync(ScrollViewer scrollViewer, double targetHorizontalOffset, double targetVerticalOffset) {
			TaskCompletionSource<object> tcs = new();
			void ViewChanged(object s, ScrollViewerViewChangedEventArgs e) =>
				_ = tcs.TrySetResult(null);
			try {
				if (scrollViewer.HorizontalOffset == targetHorizontalOffset
					&& scrollViewer.VerticalOffset == targetVerticalOffset) {
					_ = tcs.TrySetResult(null);
				}
				scrollViewer.ViewChanged += ViewChanged;
				_ = scrollViewer.ChangeView(targetHorizontalOffset, targetVerticalOffset, null, true);
				_ = await tcs.Task;
			}
			finally {
				scrollViewer.ViewChanged -= ViewChanged;
			}
		}

		private static async Task<UWPPoint> GetApproximateTargetAsync(ListViewBase list, ScrollViewer scrollViewer, object targetItem) {
			if (scrollViewer == null) { return new UWPPoint(0, 0); }
			double horizontalOffset = scrollViewer.HorizontalOffset;
			double verticalOffset = scrollViewer.VerticalOffset;
			await JumpToItemAsync(list, targetItem, ScrollToPosition.Start);
			if (list.ContainerFromItem(targetItem) is not UIElement targetContainer) { return new UWPPoint(0, 0); }
			Microsoft.UI.Xaml.Media.GeneralTransform transform = targetContainer.TransformToVisual(scrollViewer.Content as UIElement);
			await JumpToOffsetAsync(scrollViewer, horizontalOffset, verticalOffset);
			return transform.TransformPoint(Zero);
		}

		private static void JumpToIndex(ListViewBase list, int index, ScrollToPosition scrollToPosition) {
			ScrollViewer scrollViewer = list.GetFirstDescendant<ScrollViewer>();
			DependencyObject con = list.ContainerFromIndex(index);
			if (con is UIElement uIElement) {
				double width = uIElement.DesiredSize.Width;
				double current = scrollViewer.HorizontalOffset;
				double delta = (index * width) - current;
				_ = scrollViewer.ChangeView(current + delta, scrollViewer.VerticalOffset, scrollViewer.ZoomFactor, true);
			}
		}

		private static async Task<bool> ScrollToItemAsync(ListViewBase list, object targetItem, ScrollViewer scrollViewer, ScrollToPosition scrollToPosition) {
			UIElement targetContainer = list.ContainerFromItem(targetItem) as UIElement;
			if (targetContainer != null) {
				await ScrollToTargetContainerAsync(targetContainer, scrollViewer, scrollToPosition);
				return true;
			}
			return false;
		}

		private static async Task AnimateToOffsetAsync(ScrollViewer scrollViewer, double targetHorizontalOffset, double targetVerticalOffset,
			Func<Task<bool>> interruptCheck = null) {
			TaskCompletionSource<object> tcs = new();
			async void ViewChanged(object s, ScrollViewerViewChangedEventArgs e) {
				if (tcs.Task.IsCompleted) { return; }
				if (e.IsIntermediate) {
					if (interruptCheck == null) { return; }
					if (await interruptCheck()) {
						_ = scrollViewer.ChangeView(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset, 1.0f, true);
						_ = tcs.TrySetResult(null);
					}
				}
				else {
					_ = tcs.TrySetResult(null);
				}
			}
			try {
				scrollViewer.ViewChanged += ViewChanged;
				_ = scrollViewer.ChangeView(targetHorizontalOffset, targetVerticalOffset, null, false);
				_ = await tcs.Task;
			}
			finally {
				scrollViewer.ViewChanged -= ViewChanged;
			}
		}

		private static async Task ScrollToTargetContainerAsync(UIElement targetContainer, ScrollViewer scrollViewer, ScrollToPosition scrollToPosition) {
			Microsoft.UI.Xaml.Media.GeneralTransform transform = targetContainer.TransformToVisual(scrollViewer.Content as UIElement);
			UWPPoint? position = transform?.TransformPoint(Zero);
			if (!position.HasValue) { return; }
			UWPPoint offset = position.Value;
			UWPSize itemSize = targetContainer.DesiredSize;
			switch (scrollToPosition) {
				case ScrollToPosition.Start:
					break;
				case ScrollToPosition.MakeVisible:
					offset = AdjustToMakeVisible(offset, itemSize, scrollViewer);
					break;
				case ScrollToPosition.Center:
					offset = AdjustToCenter(offset, itemSize, scrollViewer);
					break;
				case ScrollToPosition.End:
					offset = AdjustToEnd(offset, itemSize, scrollViewer);
					break;
			}
			await AnimateToOffsetAsync(scrollViewer, offset.X, offset.Y);
		}
		#endregion

		#region Public Methods
		public static async Task JumpToItemAsync(this ListViewBase list, object targetItem, ScrollToPosition scrollToPosition) {
			ScrollViewer scrollViewer = list.GetFirstDescendant<ScrollViewer>();
			TaskCompletionSource<object> tcs = new();
			Func<Task> adjust = null;
			async void ViewChanged(object s, ScrollViewerViewChangedEventArgs e) {
				if (e.IsIntermediate) { return; }
				scrollViewer.ViewChanged -= ViewChanged;
				if (adjust != null) { await adjust(); }
				_ = tcs.TrySetResult(null);
			}
			try {
				scrollViewer.ViewChanged += ViewChanged;
				switch (scrollToPosition) {
					case ScrollToPosition.MakeVisible:
						list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Default);
						break;
					case ScrollToPosition.Start:
						list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Leading);
						break;
					case ScrollToPosition.Center:
						list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Leading);
						adjust = () => AdjustToCenterAsync(list, scrollViewer, targetItem);
						break;
					case ScrollToPosition.End:
						list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Leading);
						adjust = () => AdjustToEndAsync(list, scrollViewer, targetItem);
						break;
				}
				_ = await tcs.Task;
			}
			finally {
				scrollViewer.ViewChanged -= ViewChanged;
			}
		}

		public static async Task AnimateToItemAsync(this ListViewBase list, object targetItem, ScrollToPosition scrollToPosition) {
			ScrollViewer scrollViewer = list.GetFirstDescendant<ScrollViewer>();
			if (scrollViewer == null) { return; }
			if (await ScrollToItemAsync(list, targetItem, scrollViewer, scrollToPosition)) { return; }
			UWPPoint targetPoint = await GetApproximateTargetAsync(list, scrollViewer, targetItem);
			await AnimateToOffsetAsync(scrollViewer, targetPoint.X, targetPoint.Y,
				async () => await ScrollToItemAsync(list, targetItem, scrollViewer, scrollToPosition));
		}

		public static async Task<bool> ScrollToItemAsync(this ListViewBase list, int index, ScrollViewer scrollViewer, ScrollToPosition scrollToPosition) {
			UIElement targetContainer = list.ContainerFromIndex(index) as UIElement;
			if (targetContainer != null) {
				await ScrollToTargetContainerAsync(targetContainer, scrollViewer, scrollToPosition);
				return true;
			}
			return false;
		}
		#endregion

	}
	#endregion

}