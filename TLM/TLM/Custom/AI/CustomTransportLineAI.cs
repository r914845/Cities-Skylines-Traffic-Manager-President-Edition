﻿using ColossalFramework;
using CSUtil.Commons;
using CSUtil.Commons.Benchmark;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TrafficManager.Custom.PathFinding;
using TrafficManager.Geometry;
using TrafficManager.State;
using TrafficManager.Traffic;
using TrafficManager.Traffic.Data;
using UnityEngine;

namespace TrafficManager.Custom.AI {
	class CustomTransportLineAI : TransportLineAI { // TODO inherit from NetAI (in order to keep the correct references to `base`)
		public static bool CustomStartPathFind(ushort segmentID, ref NetSegment data, ItemClass.Service netService, ItemClass.Service netService2, VehicleInfo.VehicleType vehicleType, bool skipQueue) {
			if (data.m_path != 0u) {
				Singleton<PathManager>.instance.ReleasePath(data.m_path);
				data.m_path = 0u;
			}
			NetManager netManager = Singleton<NetManager>.instance;
			if ((netManager.m_nodes.m_buffer[(int)data.m_startNode].m_flags & NetNode.Flags.Ambiguous) != NetNode.Flags.None) {
				for (int i = 0; i < 8; i++) {
					ushort segment = netManager.m_nodes.m_buffer[(int)data.m_startNode].GetSegment(i);
					if (segment != 0 && segment != segmentID && netManager.m_segments.m_buffer[(int)segment].m_path != 0u) {
						return true;
					}
				}
			}
			if ((netManager.m_nodes.m_buffer[(int)data.m_endNode].m_flags & NetNode.Flags.Ambiguous) != NetNode.Flags.None) {
				for (int j = 0; j < 8; j++) {
					ushort segment2 = netManager.m_nodes.m_buffer[(int)data.m_endNode].GetSegment(j);
					if (segment2 != 0 && segment2 != segmentID && netManager.m_segments.m_buffer[(int)segment2].m_path != 0u) {
						return true;
					}
				}
			}
			Vector3 position = netManager.m_nodes.m_buffer[(int)data.m_startNode].m_position;
			Vector3 position2 = netManager.m_nodes.m_buffer[(int)data.m_endNode].m_position;
#if DEBUG
			bool debug = GlobalConfig.Instance.Debug.Switches[18];
			if (debug)
				Log._Debug($"TransportLineAI.CustomStartPathFind({segmentID}, ..., {netService}, {netService2}, {vehicleType}, {skipQueue}): startNode={data.m_startNode} @ {position}, endNode={data.m_endNode} @ {position2} -- line: {netManager.m_nodes.m_buffer[(int)data.m_startNode].m_transportLine}/{netManager.m_nodes.m_buffer[(int)data.m_endNode].m_transportLine}");
#endif
			PathUnit.Position startPosA;
			PathUnit.Position startPosB;
			float startSqrDistA;
			float startSqrDistB;
			if (!CustomPathManager.FindPathPosition(position, netService, netService2, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, vehicleType, true, false, 32f, out startPosA, out startPosB, out startSqrDistA, out startSqrDistB)) {
				CustomTransportLineAI.CheckSegmentProblems(segmentID, ref data);
				return true;
			}
			PathUnit.Position endPosA;
			PathUnit.Position endPosB;
			float endSqrDistA;
			float endSqrDistB;
			if (!CustomPathManager.FindPathPosition(position2, netService, netService2, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, vehicleType, true, false, 32f, out endPosA, out endPosB, out endSqrDistA, out endSqrDistB)) {
				CustomTransportLineAI.CheckSegmentProblems(segmentID, ref data);
				return true;
			}
			if ((netManager.m_nodes.m_buffer[(int)data.m_startNode].m_flags & NetNode.Flags.Fixed) != NetNode.Flags.None) {
				startPosB = default(PathUnit.Position);
			}
			if ((netManager.m_nodes.m_buffer[(int)data.m_endNode].m_flags & NetNode.Flags.Fixed) != NetNode.Flags.None) {
				endPosB = default(PathUnit.Position);
			}
			startPosA.m_offset = 128;
			startPosB.m_offset = 128;
			endPosA.m_offset = 128;
			endPosB.m_offset = 128;
			bool stopLane = CustomTransportLineAI.GetStopLane(ref startPosA, vehicleType);
			bool stopLane2 = CustomTransportLineAI.GetStopLane(ref startPosB, vehicleType);
			bool stopLane3 = CustomTransportLineAI.GetStopLane(ref endPosA, vehicleType);
			bool stopLane4 = CustomTransportLineAI.GetStopLane(ref endPosB, vehicleType);
			if ((!stopLane && !stopLane2) || (!stopLane3 && !stopLane4)) {
				CustomTransportLineAI.CheckSegmentProblems(segmentID, ref data);
				return true;
			}
			
			ExtVehicleType extVehicleType = ExtVehicleType.Bus;
#if BENCHMARK
			using (var bm = new Benchmark(null, "extVehicleType")) {
#endif
				if ((vehicleType & (VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Metro | VehicleInfo.VehicleType.Monorail)) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.PassengerTrain;
			if ((vehicleType & VehicleInfo.VehicleType.Tram) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.Tram;
			if ((vehicleType & VehicleInfo.VehicleType.Ship) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.PassengerShip;
			if ((vehicleType & VehicleInfo.VehicleType.Plane) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.PassengerPlane;
			if ((vehicleType & VehicleInfo.VehicleType.Ferry) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.Ferry;
			if ((vehicleType & VehicleInfo.VehicleType.Blimp) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.Blimp;
			if ((vehicleType & VehicleInfo.VehicleType.CableCar) != VehicleInfo.VehicleType.None)
				extVehicleType = ExtVehicleType.CableCar;
#if BENCHMARK
			}
#endif
			//Log._Debug($"Transport line. extVehicleType={extVehicleType}");
			uint path;
			if (CustomPathManager._instance.CreatePath(extVehicleType, 0, ExtCitizenInstance.ExtPathType.None, out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, vehicleType, 20000f, false, true, true, skipQueue)) {
				if (startPosA.m_segment != 0 && startPosB.m_segment != 0) {
					netManager.m_nodes.m_buffer[data.m_startNode].m_flags |= NetNode.Flags.Ambiguous;
				} else {
					netManager.m_nodes.m_buffer[data.m_startNode].m_flags &= ~NetNode.Flags.Ambiguous;
				}
				if (endPosA.m_segment != 0 && endPosB.m_segment != 0) {
					netManager.m_nodes.m_buffer[data.m_endNode].m_flags |= NetNode.Flags.Ambiguous;
				} else {
					netManager.m_nodes.m_buffer[data.m_endNode].m_flags &= ~NetNode.Flags.Ambiguous;
				}
				data.m_path = path;
				data.m_flags |= NetSegment.Flags.WaitingPath;
#if DEBUG
				if (debug)
					Log._Debug($"TransportLineAI.CustomStartPathFind({segmentID}, ..., {netService}, {netService2}, {vehicleType}, {skipQueue}): Started calculating path {path} for extVehicleType={extVehicleType}, startPosA=[seg={startPosA.m_segment}, lane={startPosA.m_lane}, off={startPosA.m_offset}], startPosB=[seg={startPosB.m_segment}, lane={startPosB.m_lane}, off={startPosB.m_offset}], endPosA=[seg={endPosA.m_segment}, lane={endPosA.m_lane}, off={endPosA.m_offset}], endPosB=[seg={endPosB.m_segment}, lane={endPosB.m_lane}, off={endPosB.m_offset}]");
#endif
				return false;
			}
			CustomTransportLineAI.CheckSegmentProblems(segmentID, ref data);
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool GetStopLane(ref PathUnit.Position pos, VehicleInfo.VehicleType vehicleType) {
			Log.Error($"CustomTransportLineAI.GetStopLane called.");
			return false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void CheckSegmentProblems(ushort segmentID, ref NetSegment data) {
			Log.Error($"CustomTransportLineAI.CheckSegmentProblems called.");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void CheckNodeProblems(ushort nodeID, ref NetNode data) {
			Log.Error($"CustomTransportLineAI.CheckNodeProblems called.");
		}
	}
}
