using System.IO;
using System.Management.Automation;
using System.Threading;

namespace HtmlTools.PowerShell.Cmdlet {

	public abstract class CmdletBase : PSCmdlet {

		protected CancellationTokenSource CancellationTokenSource { get; private set; }

		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		protected CmdletBase() { 

			PsEnvironment.SafeInitialize();

			CancellationTokenSource = new CancellationTokenSource();
		}

		protected override void StopProcessing() {

			CancellationTokenSource.Cancel();

			base.StopProcessing();
		}

		protected virtual string GetModulePath() {

			return MyInvocation.MyCommand.Module.ModuleBase;
		}

		protected virtual string GetFullPath(string fullOrRelativePath) {

			if (Path.IsPathRooted(fullOrRelativePath)) {
				return Path.GetFullPath(fullOrRelativePath);
			}

			var basePath = SessionState.Path.CurrentFileSystemLocation.Path;
			var result = Path.Combine(basePath, fullOrRelativePath);

			return Path.GetFullPath(result);
		}

	}

}
