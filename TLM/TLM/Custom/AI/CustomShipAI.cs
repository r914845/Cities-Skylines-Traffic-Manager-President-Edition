﻿using ColossalFramework;
using CSUtil.Commons;
using CSUtil.Commons.Benchmark;
using System;
using System.Collections.Generic;
using System.Text;
using TrafficManager.Custom.PathFinding;
using TrafficManager.Geometry;
using TrafficManager.Manager;
using TrafficManager.Traffic;
using TrafficManager.Traffic.Data;
using UnityEngine;

namespace TrafficManager.Custom.AI {
	class CustomShipAI : ShipAI {
		public bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays) {
#if DEBUG
			//Log._Debug($"CustomShipAI.CustomStartPathFind called for vehicle {vehicleID}");
#endif

			/// NON-STOCK CODE START ///
			ExtVehicleType vehicleType = ExtVehicleType.None;
#if BENCHMARK
			using (var bm = new Benchmark(null, "vehicleType")) {
#endif
				vehicleType = vehicleData.Info.m_vehicleAI is PassengerShipAI ? ExtVehicleType.PassengerShip : ExtVehicleType.CargoVehicle;
#if BENCHMARK
			}
#endif
			/// NON-STOCK CODE END ///

			VehicleInfo info = this.m_info;
			PathUnit.Position startPosA;
			PathUnit.Position startPosB;
			float startSqrDistA;
			float startSqrDistB;
			PathUnit.Position endPosA;
			PathUnit.Position endPosB;
			float endSqrDistA;
			float endSqrDistB;
			if (CustomPathManager.FindPathPosition(startPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, info.m_vehicleType, false, false, 64f, out startPosA, out startPosB, out startSqrDistA, out startSqrDistB) &&
				CustomPathManager.FindPathPosition(endPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, info.m_vehicleType, false, false, 64f, out endPosA, out endPosB, out endSqrDistA, out endSqrDistB)) {
				if (!startBothWays || startSqrDistA < 10f) {
					startPosB = default(PathUnit.Position);
				}
				if (!endBothWays || endSqrDistA < 10f) {
					endPosB = default(PathUnit.Position);
				}
				uint path;
				if (CustomPathManager._instance.CreatePath(vehicleType, vehicleID, ExtCitizenInstance.ExtPathType.None, out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, NetInfo.LaneType.Vehicle, info.m_vehicleType, 20000f)) {

					if (vehicleData.m_path != 0u) {
						Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
					}
					vehicleData.m_path = path;
					vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
					return true;
				}
			}
			return false;
		}

	}
}
