using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RackDrawingApp_LicenseDebugger
{
	// Implements INotifyPropertyChanged interface
	public class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> projection)
		{
			var memberExpression = (MemberExpression)projection.Body;
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
		}

	};
}
