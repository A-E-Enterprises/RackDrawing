using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingControl
{
	public interface IYesNoCancelViewModel
	{
		bool RememberTheChoice { get; }
	}
}
