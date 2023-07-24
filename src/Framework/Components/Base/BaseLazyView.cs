using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace StedySoft.Maui.Framework {

	#region Abstract Class BaseLazyView
	[Preserve(Conditional = true)]
	public abstract class BaseLazyView : ContentView, IDisposable {

		#region Declarations
		private bool disposedValue;
		#endregion

		#region Constructor
		protected virtual void Dispose(bool disposing) {
			if (!this.disposedValue) {
				if (disposing && this.Content is IDisposable disposable) {
					disposable.Dispose();
				}
				this.disposedValue = true;
			}
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Protected Overrides
		protected override void OnBindingContextChanged() {
			if (this.Content is not null and not ActivityIndicator) {
				this.Content.BindingContext = this.BindingContext;
			}
		}
		#endregion

		#region Bindable Properties
		public static readonly BindableProperty LoadedProperty =
			BindableProperty.Create(
				nameof(BaseLazyView.Loaded),
				typeof(bool),
				typeof(BaseLazyView),
				false);

		public new bool Loaded {
			get => (bool)this.GetValue(LoadedProperty);
			set => this.SetValue(LoadedProperty, value);
		}
		#endregion

		#region Public Methods
		public abstract ValueTask LoadViewAsync();
		#endregion

	}
	#endregion

}