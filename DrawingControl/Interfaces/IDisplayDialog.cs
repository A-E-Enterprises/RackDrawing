using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingControl
{
	public interface IDisplayDialog
	{
		Task<object> YesNoCancelDialog(string strContent, out IYesNoCancelViewModel vm);

		Task<object> DisplayMessageDialog(string strMessage);
	}
}
