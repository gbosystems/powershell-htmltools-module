using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace HtmlTools.PowerShell {

	internal static class ChromeOptionsFactory {

		public static ChromeOptions CreateDefaultOptions() {
			var value = new ChromeOptions() {
				PageLoadStrategy = PageLoadStrategy.Normal				
			};

			value.AddArgument("--headless=new");
			value.AddArgument("--disable-gpu");
			value.AddArgument("--disable-extensions");
			value.AddArgument("--disable-dev-shm-usage");
			value.AddArgument("--no-sandbox");

			return value;
		}

	}

}
