using System;

namespace StedySoft.Maui.Framework.Controls {

	#region Class CurrentItemChangedEventArgs
	public class CurrentItemChangedEventArgs : EventArgs {

		public object NewPosition { get; set; }

		public object OldPosition { get; set; }

	}
	#endregion

}