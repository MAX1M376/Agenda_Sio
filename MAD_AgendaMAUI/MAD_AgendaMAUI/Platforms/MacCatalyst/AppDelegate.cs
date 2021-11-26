using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MAD_AgendaMAUI
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}