using Newtonsoft.Json;

namespace HtmlTools.PowerShell {

	internal static class PsEnvironment {

		private static readonly object mLock = new object();

		public static JsonSerializer JsonSerializer { get; private set; }

		private static bool mIsInitialized;

		static PsEnvironment() {

			mIsInitialized = false;
			JsonSerializer = JsonSerializer.CreateDefault();
		}

		public static void SafeInitialize() {

			if (mIsInitialized) {
				return;
			}

			lock (mLock) {
				if (mIsInitialized) {
					return;
				}

				Initialize();
				mIsInitialized = true;
			}
		}

		private static void Initialize() {

			/* Do nothing */
		}

	}

}
