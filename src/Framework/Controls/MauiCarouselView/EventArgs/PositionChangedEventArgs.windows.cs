using System;

namespace StedySoft.Maui.Framework.Controls {

	#region Class PositionChangedEventArgs
	public class PositionChangedEventArgs : EventArgs {

		public int CurrentPosition { get; set; }

		public int PreviousPosition { get; set; }

	}
	#endregion

}