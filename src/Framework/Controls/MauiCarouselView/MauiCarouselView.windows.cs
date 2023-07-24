using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace StedySoft.Maui.Framework.Controls {

	#region Class MauiCarouselView
	public class MauiCarouselView : CollectionView {

		#region Declarations
		public const string CurrentItemVisualState = "CurrentItem";
		public const string NextItemVisualState = "NextItem";
		public const string PreviousItemVisualState = "PreviousItem";
		public const string DefaultItemVisualState = "DefaultItem";

		public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
		public event EventHandler<PositionChangedEventArgs> PositionChanged;
		#endregion

		#region Constructor
		public MauiCarouselView() {
			this.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) {
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
			this.SelectionMode = SelectionMode.None;
			this.Position = 0;
		}
		#endregion

		#region Bindable Properties
		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.CurrentItem),
				typeof(object),
				typeof(MauiCarouselView),
				default,
				BindingMode.TwoWay,
				propertyChanged: (b, o, n) => {
					MauiCarouselView MauiCarouselView = b.As<MauiCarouselView>();
					CurrentItemChangedEventArgs args = new() { NewPosition = n, OldPosition = o };
					ICommand command = MauiCarouselView.CurrentItemChangedCommand;
					object commandParameter = MauiCarouselView.CurrentItemChangedCommandParameter;
					if (command?.CanExecute(commandParameter) == true) { command.Execute(commandParameter); }
					MauiCarouselView.CurrentItemChanged?.Invoke(MauiCarouselView, args);
					MauiCarouselView.OnCurrentItemChanged(args);
				});

		public object CurrentItem {
			get => this.GetValue(MauiCarouselView.CurrentItemProperty);
			set => this.SetValue(MauiCarouselView.CurrentItemProperty, value);
		}

		public static readonly BindableProperty CurrentItemChangedCommandProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.CurrentItemChangedCommand),
				typeof(ICommand),
				typeof(MauiCarouselView));

		public ICommand CurrentItemChangedCommand {
			get => (ICommand)this.GetValue(MauiCarouselView.CurrentItemChangedCommandProperty);
			set => this.SetValue(MauiCarouselView.CurrentItemChangedCommandProperty, value);
		}

		public static readonly BindableProperty CurrentItemChangedCommandParameterProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.CurrentItemChangedCommandParameter),
				typeof(object),
				typeof(MauiCarouselView));

		public object CurrentItemChangedCommandParameter {
			get => this.GetValue(MauiCarouselView.CurrentItemChangedCommandParameterProperty);
			set => this.SetValue(MauiCarouselView.CurrentItemChangedCommandParameterProperty, value);
		}

		public static readonly BindableProperty IsBounceEnabledProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.IsBounceEnabled),
				typeof(bool),
				typeof(MauiCarouselView),
				true);

		public bool IsBounceEnabled {
			get => (bool)this.GetValue(MauiCarouselView.IsBounceEnabledProperty);
			set => this.SetValue(MauiCarouselView.IsBounceEnabledProperty, value);
		}

		private static readonly BindablePropertyKey IsDraggingPropertyKey =
			BindableProperty.CreateReadOnly(
				nameof(MauiCarouselView.IsDragging),
				typeof(bool),
				typeof(MauiCarouselView),
				false);

		public static readonly BindableProperty IsDraggingProperty = IsDraggingPropertyKey.BindableProperty;

		public bool IsDragging =>
			(bool)this.GetValue(MauiCarouselView.IsDraggingProperty);

		public static readonly BindableProperty IsScrollAnimatedProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.IsScrollAnimated),
				typeof(bool),
				typeof(MauiCarouselView),
				true);

		public bool IsScrollAnimated {
			get => (bool)this.GetValue(MauiCarouselView.IsScrollAnimatedProperty);
			set => this.SetValue(MauiCarouselView.IsScrollAnimatedProperty, value);
		}

		public static readonly BindableProperty IsSwipeEnabledProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.IsSwipeEnabled),
				typeof(bool),
				typeof(MauiCarouselView),
				true);

		public bool IsSwipeEnabled {
			get => (bool)this.GetValue(MauiCarouselView.IsSwipeEnabledProperty);
			set => this.SetValue(MauiCarouselView.IsSwipeEnabledProperty, value);
		}

		public static readonly BindableProperty LoopProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.Loop),
				typeof(bool),
				typeof(MauiCarouselView),
				true,
				BindingMode.OneTime);

		public bool Loop {
			get => (bool)this.GetValue(MauiCarouselView.LoopProperty);
			set => this.SetValue(MauiCarouselView.LoopProperty, value);
		}

		public static readonly BindableProperty PeekAreaInsetsProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.PeekAreaInsets),
				typeof(Thickness),
				typeof(MauiCarouselView),
				default(Thickness));

		public Thickness PeekAreaInsets {
			get => (Thickness)this.GetValue(MauiCarouselView.PeekAreaInsetsProperty);
			set => this.SetValue(MauiCarouselView.PeekAreaInsetsProperty, value);
		}

		public static readonly BindableProperty PositionProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.Position),
				typeof(int),
				typeof(MauiCarouselView),
				default(int), BindingMode.TwoWay,
				propertyChanged: (b, o, n) => {
					MauiCarouselView MauiCarouselView = b.As<MauiCarouselView>();
					PositionChangedEventArgs args = new() { PreviousPosition = (int)o, CurrentPosition = (int)n };
					ICommand command = MauiCarouselView.PositionChangedCommand;
					object commandParameter = MauiCarouselView.PositionChangedCommandParameter;
					if (command?.CanExecute(commandParameter) == true) { command.Execute(commandParameter); }
					MauiCarouselView.PositionChanged?.Invoke(MauiCarouselView, args);
					MauiCarouselView.OnPositionChanged(args);
				});

		public int Position {
			get => (int)this.GetValue(MauiCarouselView.PositionProperty);
			set => this.SetValue(MauiCarouselView.PositionProperty, value);
		}

		public static readonly BindableProperty PositionChangedCommandProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.PositionChangedCommand),
				typeof(ICommand),
				typeof(MauiCarouselView));

		public ICommand PositionChangedCommand {
			get => (ICommand)this.GetValue(MauiCarouselView.PositionChangedCommandProperty);
			set => this.SetValue(MauiCarouselView.PositionChangedCommandProperty, value);
		}

		public static readonly BindableProperty PositionChangedCommandParameterProperty =
			BindableProperty.Create(
				nameof(MauiCarouselView.PositionChangedCommandParameter),
				typeof(object),
				typeof(MauiCarouselView));

		public object PositionChangedCommandParameter {
			get => this.GetValue(MauiCarouselView.PositionChangedCommandParameterProperty);
			set => this.SetValue(MauiCarouselView.PositionChangedCommandParameterProperty, value);
		}

		private static readonly BindablePropertyKey VisibleViewsPropertyKey =
			BindableProperty.CreateReadOnly(
				nameof(MauiCarouselView.VisibleViews),
				typeof(ObservableCollection<View>),
				typeof(MauiCarouselView),
				null,
				defaultValueCreator: (b) => new ObservableCollection<View>());

		public static readonly BindableProperty VisibleViewsProperty = VisibleViewsPropertyKey.BindableProperty;

		public ObservableCollection<View> VisibleViews =>
			(ObservableCollection<View>)this.GetValue(MauiCarouselView.VisibleViewsProperty);
		#endregion

		#region Private Methods
		private static void _linkToIndicatorView(MauiCarouselView MauiCarouselView, IndicatorView indicatorView) {
			if (indicatorView == null) { return; }
			indicatorView.SetBinding(IndicatorView.PositionProperty, new Binding {
				Path = nameof(MauiCarouselView.Position),
				Source = MauiCarouselView
			});
			indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, new Binding {
				Path = nameof(ItemsView.ItemsSource),
				Source = MauiCarouselView
			});
		}
		#endregion

		#region Protected Virtuals
		protected virtual void OnCurrentItemChanged(EventArgs args) { }

		protected virtual void OnPositionChanged(EventArgs args) { }
		#endregion

		#region Public Properties
		[System.ComponentModel.TypeConverter(typeof(ReferenceTypeConverter))]
		public IndicatorView IndicatorView {
			set => MauiCarouselView._linkToIndicatorView(this, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsScrolling { get; set; }
		#endregion

		#region Public Methods

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsDragging(bool value) =>
			this.SetValue(IsDraggingPropertyKey, value);
		#endregion

		#region Public Virtuals
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimateCurrentItemChanges =>
			this.IsScrollAnimated;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimatePositionChanges =>
			this.IsScrollAnimated;
		#endregion

	}
	#endregion

}
