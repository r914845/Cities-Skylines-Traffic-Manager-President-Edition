using CSUtil.Commons;
using ICities;
using TrafficManager.State;
using UnityEngine;

namespace TrafficManager {
	public class TrafficManagerMod : IUserMod {

		public static readonly string Version = "1.10.3";

		public static readonly uint GameVersion = 167895824u;
		public static readonly uint GameVersionA = 1u;
		public static readonly uint GameVersionB = 8u;
		public static readonly uint GameVersionC = 0u;
		public static readonly uint GameVersionBuild = 3u;

		public string Name => "Traffic Manager: President Edition [" + Version + "]";

		public string Description => "Manage your city's traffic";

		public void OnEnabled() {
			Log._Debug("TrafficManagerMod Enabled");
		}

		public void OnDisabled() {
			Log._Debug("TrafficManagerMod Disabled");
		}

		public void OnSettingsUI(UIHelperBase helper) {
			Options.makeSettings(helper);
		}
	}
}
