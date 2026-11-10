using CBRE.Common.Mediator;
using System.Reflection;
using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.UI
{
	public partial class AboutDialog : DarkModeForm
	{
		public AboutDialog()
		{
			InitializeComponent();

			VersionLabel.Text = "Version " + Assembly.GetAssembly(typeof(Editor)).GetName().Version.ToString(3);

			GitHubLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, GitHubLink.Text);
			LicenseLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, LicenseLink.Text);
			ExtraLicenseLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, ExtraLicenseLink.Text);

			DescriptionLabel.Links.Add(264, 19, "http://logic-and-trick.com");
			DescriptionLabel.LinkClicked += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, e.Link.LinkData.ToString());
		}
	}
}
