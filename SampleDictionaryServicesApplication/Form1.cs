using System;
using System.Windows.Forms;
using Palaso.Services;

namespace SampleDictionaryServicesApplication
{
	public partial class Form1 : Form
	{
		private readonly IServiceAppSingletonHelper _serviceAppSingletonHelper;
		delegate void BringToFrontRequestCallback(object sender, EventArgs args);
		public Form1(IServiceAppSingletonHelper serviceAppSingletonHelper)
		{
			_serviceAppSingletonHelper = serviceAppSingletonHelper;
			_serviceAppSingletonHelper.BringToFrontRequest += On_BringToFrontRequest;

			InitializeComponent();
		}

		 void On_BringToFrontRequest(object sender, EventArgs args)
		{
			if (InvokeRequired)
			{

				Invoke(new BringToFrontRequestCallback(On_BringToFrontRequest),sender,args);
			}
			else
			{
				this.Activate();
			}
		}
	}
}