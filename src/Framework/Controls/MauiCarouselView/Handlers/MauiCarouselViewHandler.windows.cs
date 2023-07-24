using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

using WRect = Windows.Foundation.Rect;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace StedySoft.Maui.Framework.Controls {

	#region Class MauiCarouselViewHandler
	internal static class MauiCarouselViewHandler {

		#region Declarations
		private static MauiCarouselView _virtualView { get; set; }
		private static ListViewBase _platformView { get; set; }

		private static ScrollViewer _scrollViewer { get; set; }

		private static double _previousHorizontalOffset { get; set; }
		private static double _previousVerticalOffset { get; set; }
		#endregion

		#region Private Methods
		private static void _setupScrollViewer() {
			LinearItemsLayout itemsLayout = MauiCarouselViewHandler._virtualView.ItemsLayout.As<LinearItemsLayout>();

			MauiCarouselViewHandler._scrollViewer.IsScrollInertiaEnabled = MauiCarouselViewHandler._virtualView.IsBounceEnabled;

			MauiCarouselViewHandler._platformView.IsSwipeEnabled = MauiCarouselViewHandler._virtualView.IsSwipeEnabled;

			MauiCarouselViewHandler._platformView.Padding = new WThickness(
				MauiCarouselViewHandler._virtualView.PeekAreaInsets.Left,
				MauiCarouselViewHandler._virtualView.PeekAreaInsets.Top,
				MauiCarouselViewHandler._virtualView.PeekAreaInsets.Right,
				MauiCarouselViewHandler._virtualView.PeekAreaInsets.Bottom);


			switch (itemsLayout.Orientation) {
				case ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(MauiCarouselViewHandler._platformView, WScrollMode.Enabled);
					ScrollViewer.SetVerticalScrollMode(MauiCarouselViewHandler._platformView, WScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(MauiCarouselViewHandler._platformView,
						MauiCarouselViewHandler._virtualView.HorizontalScrollBarVisibility.ToPlatform());
					break;
				case ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetHorizontalScrollMode(MauiCarouselViewHandler._platformView, WScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollMode(MauiCarouselViewHandler._platformView, WScrollMode.Enabled);
					ScrollViewer.SetVerticalScrollBarVisibility(MauiCarouselViewHandler._platformView,
						MauiCarouselViewHandler._virtualView.VerticalScrollBarVisibility.ToPlatform());
					break;
			}

			MauiCarouselViewHandler._scrollViewer.HorizontalSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType.Mandatory;
			MauiCarouselViewHandler._scrollViewer.HorizontalSnapPointsAlignment = itemsLayout.SnapPointsAlignment.ToPlatform();
		}

		private static ItemsViewScrolledEventArgs _createEventArgs() {
			ItemsViewScrolledEventArgs args =
				new() {
					FirstVisibleItemIndex = -1,
					LastVisibleItemIndex = -1,
					HorizontalOffset = MauiCarouselViewHandler._scrollViewer.HorizontalOffset,
					HorizontalDelta = MauiCarouselViewHandler._scrollViewer.HorizontalOffset - MauiCarouselViewHandler._previousHorizontalOffset,
					VerticalOffset = MauiCarouselViewHandler._scrollViewer.VerticalOffset,
					VerticalDelta = MauiCarouselViewHandler._scrollViewer.VerticalOffset - MauiCarouselViewHandler._previousVerticalOffset,
				};

			MauiCarouselViewHandler._previousHorizontalOffset = MauiCarouselViewHandler._scrollViewer.HorizontalOffset;
			MauiCarouselViewHandler._previousVerticalOffset = MauiCarouselViewHandler._scrollViewer.VerticalOffset;

			LinearItemsLayout itemsLayout = MauiCarouselViewHandler._virtualView.ItemsLayout.As<LinearItemsLayout>();

			int count = 0;
			foreach (ListViewItemPresenter presenter in MauiCarouselViewHandler._platformView.GetChildren<ListViewItemPresenter>()) {
				if (MauiCarouselViewHandler._isElementVisibleInContainer(
						presenter,
						MauiCarouselViewHandler._scrollViewer,
						itemsLayout.Orientation)) {
					if (args.FirstVisibleItemIndex == -1) { args.FirstVisibleItemIndex = count; }
					args.LastVisibleItemIndex = count;
				}
				count++;
			}

			double center = (args.LastVisibleItemIndex + args.FirstVisibleItemIndex) / 2.0;
			args.CenterItemIndex =
				itemsLayout.Orientation switch {
					ItemsLayoutOrientation.Vertical =>
						args.VerticalDelta > 0
							? (int)Math.Ceiling(center)
							: (int)Math.Floor(center),
					_ => args.HorizontalDelta > 0
						? (int)Math.Ceiling(center)
						: (int)Math.Floor(center)
				};

			return args;
		}

		private static bool _isElementVisibleInContainer(FrameworkElement element, FrameworkElement container, ItemsLayoutOrientation layoutOrientation) {
			if (element is null || container is null) { return false; }
			if (element?.Visibility != WVisibility.Visible) { return false; }
			WRect elementBounds = element.TransformToVisual(container).TransformBounds(new WRect(0, 0, element.ActualWidth, element.ActualHeight));
			WRect containerBounds = new(0, 0, container.ActualWidth, container.ActualHeight);
			return layoutOrientation switch {
				ItemsLayoutOrientation.Vertical => elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top,
				_ => elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left,
			};
		}
		#endregion

		#region Public Methods
		public static void AppendToMapping() =>
			CollectionViewHandler.Mapper.AppendToMapping(nameof(MauiCarouselView), (handler, view) => {
				if (MauiCarouselViewHandler._virtualView is null && view is MauiCarouselView mauiCarouselView) {
					MauiCarouselViewHandler._virtualView = mauiCarouselView;
					MauiCarouselViewHandler._virtualView.PropertyChanging += MauiCarouselViewHandler.OnVirtualViewPropertyChanging;
					MauiCarouselViewHandler._platformView = handler.PlatformView;
					MauiCarouselViewHandler._platformView.Loaded += MauiCarouselViewHandler.OnPlatformViewLoaded;
				}
			});

		#endregion

		#region Events
		private static void OnPlatformViewLoaded(object sender, RoutedEventArgs e) {
			MauiCarouselViewHandler._platformView.Loaded -= MauiCarouselViewHandler.OnPlatformViewLoaded;
			MauiCarouselViewHandler._scrollViewer = MauiCarouselViewHandler._platformView.GetFirstDescendant<ScrollViewer>();
			if (MauiCarouselViewHandler._scrollViewer is not null) {
				MauiCarouselViewHandler._setupScrollViewer();
				MauiCarouselViewHandler._scrollViewer.ViewChanging += MauiCarouselViewHandler.OnScrollViewerViewChanging;
				MauiCarouselViewHandler._scrollViewer.ViewChanged += MauiCarouselViewHandler.OnScrollViewerViewChanged;
				MauiCarouselViewHandler._virtualView.PositionChanged += async (s, e) => {
					if (e.PreviousPosition != e.CurrentPosition) {
						LinearItemsLayout itemsLayout = MauiCarouselViewHandler._virtualView.ItemsLayout.As<LinearItemsLayout>();
						_ = await MauiCarouselViewHandler._platformView.ScrollToItemAsync(
							e.CurrentPosition,
							MauiCarouselViewHandler._scrollViewer,
							itemsLayout.SnapPointsAlignment.ToScrollToPosition());
					}
				};
			}
		}

		private static void OnVirtualViewPropertyChanging(object sender, PropertyChangingEventArgs e) {
			if (e.PropertyName == "Window") {
				MauiCarouselViewHandler._virtualView.PropertyChanging -= MauiCarouselViewHandler.OnVirtualViewPropertyChanging;
				if (MauiCarouselViewHandler._scrollViewer is not null) {
					MauiCarouselViewHandler._scrollViewer.ViewChanging -= MauiCarouselViewHandler.OnScrollViewerViewChanging;
					MauiCarouselViewHandler._scrollViewer.ViewChanged -= MauiCarouselViewHandler.OnScrollViewerViewChanged;
				}
				MauiCarouselViewHandler._scrollViewer = null;
				MauiCarouselViewHandler._platformView = null;
				MauiCarouselViewHandler._virtualView = null;
			}
		}

		private static void OnScrollViewerViewChanging(object sender, ScrollViewerViewChangingEventArgs e) {
			MauiCarouselViewHandler._virtualView.SetIsDragging(e.IsInertial);
			MauiCarouselViewHandler._virtualView.IsScrolling = e.IsInertial;
		}

		private static void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
			ItemsViewScrolledEventArgs args = MauiCarouselViewHandler._createEventArgs();
			if (!e.IsIntermediate && MauiCarouselViewHandler._virtualView.IsScrolling) {
				if (!MauiCarouselViewHandler._virtualView.Position.Equals(args.CenterItemIndex)) {
					MauiCarouselViewHandler._virtualView.Position = args.CenterItemIndex;
				}
			}
			MauiCarouselViewHandler._virtualView.SetIsDragging(e.IsIntermediate);
			MauiCarouselViewHandler._virtualView.IsScrolling = e.IsIntermediate;
		}
		#endregion

	}
	#endregion

}
