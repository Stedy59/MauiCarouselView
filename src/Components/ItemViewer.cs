using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

using StedySoft.Maui.Framework;

namespace StedySoft.CarouselView.Components {

	#region Class MauiTabView
	[Preserve(AllMembers = true)]
	[ContentProperty(nameof(ViewerItems))]
	public class ItemViewer : ContentView, IDisposable {

		#region Declarations
		private Grid _mainContainer { get; set; }
		private Grid _itemContentContainer { get; set; }
		private Grid _itemContent { get; set; }
		private Microsoft.Maui.Controls.CarouselView _contentContainer { get; set; }

		private bool _updatingIndex { get; set; }
		private bool _scrollToInternal { get; set; }
		private int _scrollToIndex { get; set; } = -1;
		#endregion

		#region Constructor
		public ItemViewer() =>
			this.ControlTemplate =
				Application.Current.Resources.TryGetTemplate("ItemViewerTemplate");

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.ViewerItems.CollectionChanged -= this.OnViewerItemsCollectionChanged;
				if (this._contentContainer != null) {
					this._contentContainer.PositionChanged -= this.OnContentContainerPositionChanged;
					this._contentContainer.Scrolled -= this.OnContentContainerScrolled;
					this._contentContainer.ScrollToRequested -= this.OnContentContainerScrollToRequested;
				}
			}
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Bindable Properties
		public static readonly BindableProperty ViewerItemsProperty =
			BindableProperty.Create(
				nameof(ItemViewer.ViewerItems),
				typeof(ObservableCollection<ItemViewerItem>),
				typeof(ItemViewer),
				defaultValueCreator: _ => new ObservableCollection<ItemViewerItem>());

		public ObservableCollection<ItemViewerItem> ViewerItems {
			get => (ObservableCollection<ItemViewerItem>)this.GetValue(ViewerItemsProperty);
			set => this.SetValue(ViewerItemsProperty, value);
		}

		public static readonly BindableProperty SelectedIndexProperty =
			BindableProperty.Create(
				nameof(ItemViewer.SelectedIndex),
				typeof(int),
				typeof(ItemViewer),
				-1,
				BindingMode.TwoWay,
				propertyChanged: async (b, o, n) => {
					if ((int)n >= 0 && (int)n != (int)o) {
						await b.As<ItemViewer>()?._updateSelectedIndexAsync((int)n);
					}
				});

		public int SelectedIndex {
			get => (int)this.GetValue(SelectedIndexProperty);
			set => this.SetValue(SelectedIndexProperty, value);
		}

		public static readonly BindableProperty ItemContainerHeightProperty =
			BindableProperty.Create(
				nameof(ItemViewer.ItemContainerHeight),
				typeof(double),
				typeof(ItemViewer),
				30d);

		public double ItemContainerHeight {
			get => (double)this.GetValue(ItemContainerHeightProperty);
			set => this.SetValue(ItemContainerHeightProperty, value);

		}

		public static readonly BindableProperty IndicatorColorProperty =
			BindableProperty.Create(
				nameof(ItemViewer.IndicatorColor),
				typeof(Color),
				typeof(ItemViewer),
				Microsoft.Maui.Graphics.Colors.Blue);

		public Color IndicatorColor {
			get => (Color)this.GetValue(IndicatorColorProperty);
			set => this.SetValue(IndicatorColorProperty, value);
		}

		public static readonly BindableProperty IndicatorHeightProperty =
			BindableProperty.Create(
				nameof(ItemViewer.IndicatorHeight),
				typeof(double),
				typeof(ItemViewer),
				2d);

		public double IndicatorHeight {
			get => (double)this.GetValue(IndicatorHeightProperty);
			set => this.SetValue(IndicatorHeightProperty, value);
		}

		public static readonly BindableProperty IndicatorWidthProperty =
			BindableProperty.Create(
				nameof(ItemViewer.IndicatorWidth),
				typeof(double),
				typeof(ItemViewer),
				default(double));

		public double IndicatorWidth {
			get => (double)this.GetValue(IndicatorWidthProperty);
			set => this.SetValue(IndicatorWidthProperty, value);
		}

		public static readonly BindableProperty IndicatorTranslationXProperty =
			BindableProperty.Create(
				nameof(ItemViewer.IndicatorTranslationX),
				typeof(double),
				typeof(ItemViewer),
				2d);

		public double IndicatorTranslationX {
			get => (double)this.GetValue(IndicatorTranslationXProperty);
			set => this.SetValue(IndicatorTranslationXProperty, value);
		}

		public static readonly BindableProperty ItemContentHeightProperty =
			BindableProperty.Create(
				nameof(ItemViewer.ItemContentHeight),
				typeof(double),
				typeof(ItemViewer),
				0d);

		public double ItemContentHeight {
			get => (double)this.GetValue(ItemContentHeightProperty);
			set => this.SetValue(ItemContentHeightProperty, value);
		}

		public static readonly BindableProperty ItemContentWidthProperty =
			BindableProperty.Create(
				nameof(ItemViewer.ItemContentWidth),
				typeof(double),
				typeof(ItemViewer),
				0d);

		public double ItemContentWidth {
			get => (double)this.GetValue(ItemContentWidthProperty);
			set => this.SetValue(ItemContentWidthProperty, value);
		}
		#endregion

		#region Protected Overrides
		protected override void OnApplyTemplate() {
			base.OnApplyTemplate();
			this._mainContainer = this.GetTemplateChild("mainContainer").As<Grid>();
			this._itemContentContainer ??= this._mainContainer?.FindByName<Grid>("itemContentContainer");
			this._itemContent ??= this._itemContentContainer?.FindByName<Grid>("itemContent");
			this._contentContainer ??= this._mainContainer?.FindByName<Microsoft.Maui.Controls.CarouselView>("contentContainer");
			this.ViewerItems.CollectionChanged += this.OnViewerItemsCollectionChanged;
			this._contentContainer.PositionChanged += this.OnContentContainerPositionChanged;
			this._contentContainer.Scrolled += this.OnContentContainerScrolled;
			this._contentContainer.ScrollToRequested += this.OnContentContainerScrollToRequested;
			this._contentContainer.ItemsSource = this.ViewerItems.Where(t => t.Content is not null);
			this._contentContainer.ItemTemplate =
				new DataTemplate(() =>
					new ContentView()
						.Bind(ContentProperty, "Content")
						.Bind(View.HeightRequestProperty, path: "ItemContentHeight", source: this)
						.Bind(View.WidthRequestProperty, path: "ItemContentWidth", source: this));
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint) {
			if (this._contentContainer?.ItemsSource is not null) {
				double h = heightConstraint != double.PositiveInfinity ? heightConstraint : this.HeightRequest;
				this.IndicatorWidth = widthConstraint / this.ViewerItems.Count;
				this.ItemContentHeight = h - this.ViewerItems[0].HeightRequest;
				this.ItemContentWidth = widthConstraint;
			}
			return base.MeasureOverride(widthConstraint, heightConstraint);
		}

		protected override void OnBindingContextChanged() {
			base.OnBindingContextChanged();
			if (this.ViewerItems is null || this.ViewerItems.Count == 0) { return; }
			foreach (ItemViewerItem item in this.ViewerItems) {
				this._updateItemBindingContext(item);
			}
		}

		protected override async void OnPropertyChanged([CallerMemberName] string propertyName = "") {
			base.OnPropertyChanged(propertyName);
			switch (propertyName) {
				case nameof(this.IndicatorWidth):
					await this._adjustSelectedIndexAsync(this.SelectedIndex);
					break;
			}
		}
		#endregion

		#region Private Methods

		#region ViewerItem Methods
		private void _addItem(ItemViewerItem item, int index = -1) {
			this._addItemTapCommand(item);
			this._addItemToContentGrid(item, index);
		}

		private void _addItemTapCommand(ItemViewerItem item) =>
			item.TappedCommand =
				new Command(async (s) => {
					if (s is not View view) { return; }
					await this._updateSelectedIndexAsync(this._itemContent.Children.IndexOf(view));
				});

		private void _addItemToContentGrid(View item, int index = -1) {
			item.HeightRequest = this.ItemContainerHeight;
			item.VerticalOptions = LayoutOptions.Start;
			this._itemContent.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = (item is ItemViewerItem viewerItem && viewerItem.Width > 0)
					? viewerItem.Width
					: GridLength.Star
			});
			this._itemContent.Children.Add(item);
			item.SetValue(Grid.ColumnProperty, this._itemContent.Children.Count - 1);
			this._updateItemWidth(item.As<ItemViewerItem>());
		}

		private void _updateItemBindingContext(ItemViewerItem item) {
			if (item?.Content == null) { return; }
			item.Content.BindingContext ??= this.BindingContext;
		}

		private void _updateItemWidth(ItemViewerItem? item) {
			if (item == null) { return; }
			int index = this._itemContent.Children.IndexOf(item);
			ColumnDefinitionCollection columns = this._itemContent.ColumnDefinitions;
			ColumnDefinition? column = null;
			if (index < columns.Count) { column = columns[index]; }
			if (column == null) { return; }
			column.Width = item.Width > 0 ? item.Width : GridLength.Star;
		}

		private async Task _updateSelectedItemAsync(int oldPosition, int newPosition) {
			if (this.ViewerItems[newPosition]?.Content is BaseLazyView lazyView && !lazyView.Loaded) {
				await lazyView.LoadViewAsync();
			}
			if (oldPosition != -1 && this.ViewerItems[oldPosition].IsSelected) {
				this.ViewerItems[oldPosition].IsSelected = false;
			}
			this.ViewerItems[newPosition].IsSelected = true;
		}
		#endregion

		#region SelectedIndex Methods
		private async Task _adjustSelectedIndexAsync(int selectedIndex) {
			switch (selectedIndex) {
				case -1:
					await this._updateSelectedItemAsync(selectedIndex, 0);
					this.IndicatorTranslationX = 0;
					this.SelectedIndex = 0;
					break;
				default:
					this._contentContainer.ScrollTo(selectedIndex, animate: false);
					this.IndicatorTranslationX = this.IndicatorWidth * selectedIndex;
					break;
			}
		}

		private async Task _updateSelectedIndexAsync(int newPosition, bool execScrollTo = true) {
			if (newPosition < 0 || newPosition == this.SelectedIndex || this._updatingIndex) { return; }
			try {
				this._updatingIndex = true;
				int oldPosition = this.SelectedIndex;
				if (this.ViewerItems.Count > 0) {
					this._scrollToInternal = execScrollTo;
					await this._updateSelectedItemAsync(oldPosition, newPosition);
					this.SelectedIndex = newPosition;
					if (execScrollTo) {
						await Task.Run(() => {
							this._contentContainer.Dispatcher.DispatchIfRequired(
								() => this._contentContainer.ScrollTo(
									this.ViewerItems[newPosition],
									position: ScrollToPosition.Center,
									animate: true));
							return Task.CompletedTask;
						});
					}
				}
			}
			finally {
				this._updatingIndex = false;
			}
		}
		#endregion

		#endregion

		#region Events
		private void OnViewerItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach (ItemViewerItem item in e.NewItems) {
						this._itemContent.BatchBegin();
						this.BatchBegin();
						this._addItem(item, this.ViewerItems.IndexOf(item));
						this.BatchCommit();
						this._itemContent.BatchCommit();
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (ItemViewerItem item in e.OldItems) {
						this._itemContent.BatchBegin();
						this.BatchBegin();
						_ = this._itemContent.Children.Remove(item);
						this.BatchCommit();
						this._itemContent.BatchCommit();
					}
					break;
				default:
					throw new NotSupportedException();
			}
		}
		private async void OnContentContainerPositionChanged(object sender, PositionChangedEventArgs e) {
			switch (this._scrollToInternal) {
				case true:
					if (e.CurrentPosition == this._scrollToIndex) {
						this._scrollToIndex = -1;
						this._scrollToInternal = false;
					}
					break;
				case false:
					if (e.PreviousPosition != e.CurrentPosition) {
						await this._updateSelectedIndexAsync(e.CurrentPosition, false);
					}
					break;
			}
		}

		private void OnContentContainerScrolled(object? sender, ItemsViewScrolledEventArgs e) {
			if (this._contentContainer.IsDragging) {
				this._scrollToIndex = -1;
				this._scrollToInternal = false;
			}
			this.IndicatorTranslationX += e.HorizontalDelta / (this.ViewerItems.Count > 0 ? this.ViewerItems.Count : 1);
		}

		private void OnContentContainerScrollToRequested(object sender, ScrollToRequestEventArgs e) =>
			this._scrollToIndex = this._scrollToInternal ? e.Index : -1;
		#endregion

	}
	#endregion

}
