using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

using StedySoft.Maui.Framework;

namespace StedySoft.CarouselView.Components {

	#region Class ItemViewerItem
	[Preserve(AllMembers = true)]
	[ContentProperty(nameof(Content))]
	public class ItemViewerItem : Label {

		#region Constructor
		public ItemViewerItem() {
			this.TextColor = ColorManager.Current.TabTextColor;
			this.Shadow = new Shadow() {
				Opacity = 1,
				Offset = new Point(.5, .5),
				Brush = ColorManager.Current.TextShadowColor,
				Radius = 1f
			};

			this.HorizontalOptions = LayoutOptions.CenterAndExpand;
			this.HorizontalTextAlignment = TextAlignment.Center;
			this.VerticalOptions = LayoutOptions.CenterAndExpand;
			this.VerticalTextAlignment = TextAlignment.Center;

			this.BindingContext = this;
			this.TappedCommandParameter = this;

			TapGestureRecognizer tapGestureRecognizer = new();
			tapGestureRecognizer.Tapped += (s, e) => this.TappedCommand?.Execute(this.TappedCommandParameter);
			this.GestureRecognizers.Add(tapGestureRecognizer);
		}
		#endregion

		#region Bindable Properties
		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(
				nameof(ItemViewerItem.Content),
				typeof(View),
				typeof(ItemViewerItem),
				null);

		public View? Content {
			get => (View?)this.GetValue(ItemViewerItem.ContentProperty);
			private set => this.SetValue(ItemViewerItem.ContentProperty, value);
		}

		public static readonly BindableProperty IsSelectedProperty =
			BindableProperty.Create(
				nameof(ItemViewerItem.IsSelected),
				typeof(bool),
				typeof(ItemViewerItem),
				false,
				propertyChanged: (s, o, n) =>
					s.As<ItemViewerItem>().TextColor = n.CastTo<bool>()
						? ColorManager.Current.TabTextSelectedColor
						: ColorManager.Current.TabTextColor);

		public bool IsSelected {
			get => (bool)this.GetValue(ItemViewerItem.IsSelectedProperty);
			set => this.SetValue(ItemViewerItem.IsSelectedProperty, value);
		}

		public static readonly BindableProperty TappedCommandProperty =
			BindableProperty.Create(
				nameof(ItemViewerItem.TappedCommand),
				typeof(ICommand),
				typeof(ItemViewerItem),
				null);

		public ICommand TappedCommand {
			get => (ICommand)this.GetValue(ItemViewerItem.TappedCommandProperty);
			set => this.SetValue(ItemViewerItem.TappedCommandProperty, value);
		}

		public static readonly BindableProperty TappedCommandParameterProperty =
			BindableProperty.Create(
				nameof(ItemViewerItem.TappedCommandParameter),
				typeof(object),
				typeof(ItemViewerItem),
				null);

		public object TappedCommandParameter {
			get => this.GetValue(ItemViewerItem.TappedCommandParameterProperty);
			set => this.SetValue(ItemViewerItem.TappedCommandParameterProperty, value);
		}
		#endregion

	}
	#endregion

}
