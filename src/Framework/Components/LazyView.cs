using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace StedySoft.Maui.Framework {

	#region Class LazyView
	public class LazyView<TView> : BaseLazyView where TView : View, new() {

		public override ValueTask LoadViewAsync() {
			View? view = new TView { BindingContext = this.BindingContext };
			this.Loaded = true;
			this.Content = view;
			return new ValueTask(Task.FromResult(true));
		}

	}
	#endregion

}