using System;
using System.Threading;

namespace StedySoft.Maui.Framework {

	#region Class ActionDisposable
	internal class ActionDisposable(Action action) : IDisposable {

		private volatile Action? _action = action;

		public void Dispose() =>
			Interlocked.Exchange(ref this._action, null)?.Invoke();

	}
	#endregion

}
