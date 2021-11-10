using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SIO_AgendaWPF.Extensions
{
    public static class ApplicationExtension
    {
		public static void ExecOnUiThread(this Application app, Action action)
		{
			var dispatcher = app.Dispatcher;
			if (dispatcher.CheckAccess())
				action();
			else
				dispatcher.BeginInvoke(action);
		}
	}
}
