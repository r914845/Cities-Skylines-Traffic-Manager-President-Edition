﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficManager.Manager {
	/// <summary>
	/// Helper class to ensure that events are always handled in the simulation thread
	/// </summary>
	public abstract class AbstractFeatureManager : AbstractCustomManager, IFeatureManager {
		public void OnDisableFeature() {
			Services.SimulationService.PauseSimulation(true);
			OnDisableFeatureInternal();
			Services.SimulationService.ResumeSimulation(true);
		}

		public void OnEnableFeature() {
			Services.SimulationService.PauseSimulation(true);
			OnEnableFeatureInternal();
			Services.SimulationService.ResumeSimulation(true);
		}

		/// <summary>
		/// Executes whenever the associated feature is disabled. Guaranteed to run in the simulation thread.
		/// </summary>
		protected abstract void OnDisableFeatureInternal();

		/// <summary>
		/// Executes whenever the associated feature is enabled. Guaranteed to run in the simulation thread.
		/// </summary>
		protected abstract void OnEnableFeatureInternal();
	}
}