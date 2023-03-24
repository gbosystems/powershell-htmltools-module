using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace HtmlTools.PowerShell.Cmdlet {

	[Cmdlet(VerbsData.ConvertTo, "PdfFromHtml", DefaultParameterSetName = "Default")]
	public class ConvertTo_PdfFromHtml : CmdletBase {

		// https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-addScriptToEvaluateOnNewDocument
		public const string AddScriptCommandName = "Page.addScriptToEvaluateOnNewDocument";
		public const string AddScriptCommandSourceArg = "source";

		// https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
		public const string PrintToPDFCommandName = "Page.printToPDF";

		// https://developer.chrome.com/articles/new-headless/#debugging
		public const string RemoteDebuggerPortArg = "--remote-debugging-port";

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = true,
			ValueFromPipeline = true)]
		public string InputPath { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = true,
			ValueFromPipeline = false)]
		public string OutputPath { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter Landscape { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter DisplayHeaderFooter { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter PrintBackground { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? Scale { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? PaperWidth { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? PaperHeight { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? MarginTop { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? MarginBottom { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? MarginLeft { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public double? MarginRight { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string PageRanges { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string HeaderTemplate { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string FooterTemplate { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter PreferCSSPageSize { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string AdditionalArguments { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string InjectJavascript { get; set; }

		[Parameter(
			ParameterSetName = "Default",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter AllowChromeDebugger { get; set; }

		protected override void ProcessRecord() {
			
			/* Get relevant file paths */
			var inputPath = GetFullPath(InputPath);
			var outputPath = GetFullPath(OutputPath);
			var modulePath = GetModulePath();

			/* Read HMTL template */
			var html = File.ReadAllText(inputPath);
			var htmlAsB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
			
			/* Create options -- add args if specified */
			var options = ChromeOptionsFactory.CreateDefaultOptions();

			if (!String.IsNullOrWhiteSpace(AdditionalArguments)) {
				options.AddArgument(AdditionalArguments);
			}

			/* If neccessary, add remote debugger argument */
			if (AllowChromeDebugger && RemoteDebuggerPortNotSpecified()) {
				options.AddArgument($"{RemoteDebuggerPortArg}=0");
			}

			/* Spin up the driver, load the page, print it to a PDF
				SEE: https://developer.chrome.com/articles/new-headless/
				SEE: https://chromedevtools.github.io/devtools-protocol/tot/Page/ */
			using (var service = ChromeDriverService.CreateDefaultService(modulePath)) {

				/* https://stackoverflow.com/q/56060886 */
				service.HideCommandPromptWindow = true;
				service.SuppressInitialDiagnosticInformation = true;

				using (var driver = new ChromeDriver(service, options)) {
	
				/* Inject Javascript - Probably needs to be in a window.onload = function() { ... } to be useful */
				if (!String.IsNullOrWhiteSpace(InjectJavascript)) {
						driver.ExecuteCdpCommand(AddScriptCommandName, new Dictionary<string, object> {
						{ AddScriptCommandSourceArg, InjectJavascript }
					});
					}

					/* Open page from dataURI */
					driver.Navigate().GoToUrl("data:text/html;base64," + htmlAsB64);

					if (AllowChromeDebugger) {
						Host.UI.Write("Paused for debugger. Press any key to continue...");
						Host.UI.ReadLine();
					}

					var printOptions = CreatePrintOptions();
					var printOutput = driver.ExecuteCdpCommand(PrintToPDFCommandName, printOptions) as Dictionary<string, object>;
					var pdf = Convert.FromBase64String(printOutput["data"] as string);

					/* Write the PDF document */
					File.WriteAllBytes(outputPath, pdf);
				}
			}
		}

		private Dictionary<string, object> CreatePrintOptions() {

			var value = new Dictionary<string, object>();

			if (Landscape.IsPresent) {
				value.Add("landscape", Landscape.ToBool());
			}

			if (DisplayHeaderFooter.IsPresent) {
				value.Add("displayHeaderFooter", DisplayHeaderFooter.ToBool());
			}

			if (PrintBackground.IsPresent) {
				value.Add("printBackground", PrintBackground.ToBool());
			}

			if (Scale.HasValue) {
				value.Add("scale", Scale.Value);
			}

			if (PaperWidth.HasValue) {
				value.Add("paperWidth", PaperWidth.Value);
			}

			if (PaperHeight.HasValue) {
				value.Add("paperHeight", PaperHeight.Value);
			}

			if (MarginTop.HasValue) {
				value.Add("marginTop", MarginTop.Value);
			}

			if (MarginBottom.HasValue) {
				value.Add("marginBottom", MarginBottom.Value);
			}

			if (MarginLeft.HasValue) {
				value.Add("marginLeft", MarginLeft.Value);
			}

			if (MarginRight.HasValue) {
				value.Add("marginRight", MarginRight.Value);
			}

			if (!String.IsNullOrWhiteSpace(PageRanges)) {
				value.Add("pageRanges", PageRanges);
			}

			if (!String.IsNullOrWhiteSpace(HeaderTemplate)) {
				value.Add("headerTemplate", HeaderTemplate);
			}

			if (!String.IsNullOrWhiteSpace(FooterTemplate)) {
				value.Add("footerTemplate", FooterTemplate);
			}

			if (PreferCSSPageSize.IsPresent) {
				value.Add("preferCSSPageSize", PreferCSSPageSize.ToBool());
			}

			return value;
		}

		private bool RemoteDebuggerPortNotSpecified() {

			if (String.IsNullOrWhiteSpace(AdditionalArguments)) {
				return true;
			}

			return !AdditionalArguments
				.Contains(RemoteDebuggerPortArg, StringComparison.InvariantCultureIgnoreCase);
		}

	}

}
