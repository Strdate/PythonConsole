namespace PythonConsole
{
    internal static class InstanceUtil
    {
        public static string GetLineName(this InstanceID instanceId)
        {
            var lineId = instanceId.TransportLine;
            return lineId != 0
                ? TransportManager.instance.GetLineName(lineId) ?? "N/A"
                : "N/A";
        }

        public static string GetNetworkAssetName(this InstanceID instanceId)
        {
            var segmentId = instanceId.NetSegment;
            if (segmentId > 0) {
                return NetManager.instance.m_segments.m_buffer[segmentId].Info?.name ?? "N/A";
            }

            var nodeId = instanceId.NetNode;
            if (nodeId > 0) {
                return NetManager.instance.m_nodes.m_buffer[nodeId].Info?.name ?? "N/A";
            }

            return "N/A";
        }

        public static string GetParkName(this InstanceID instanceId)
        {
            var parkId = instanceId.Park;
            return parkId != 0
                ? DistrictManager.instance.GetParkName(parkId) ?? "N/A"
                : "N/A";
        }

        public static string GetDistrictName(this InstanceID instanceId)
        {
            var districtId = instanceId.District;
            return districtId != 0
                ? DistrictManager.instance.GetDistrictName(districtId) ?? "N/A"
                : "N/A";
        }

        public static string GetBuildingAssetName(this InstanceID instanceId)
        {
            var buildingId = instanceId.Building;
            return buildingId != 0
                ? BuildingManager.instance.m_buildings.m_buffer[buildingId].Info?.name ?? "N/A"
                : "N/A";
        }

        public static string GetVehicleAssetName(this InstanceID instanceId)
        {
            var vehicleId = instanceId.Vehicle;
            if (vehicleId != 0) {
                return VehicleManager.instance.m_vehicles.m_buffer[vehicleId].Info?.name ?? "N/A";
            }

            vehicleId = instanceId.ParkedVehicle;
            if (vehicleId != 0) {
                return VehicleManager.instance.m_parkedVehicles.m_buffer[vehicleId].Info?.name ?? "N/A";
            }

            return "N/A";
        }

        public static string GetPropAssetName(this InstanceID instanceId)
        {
            var id = instanceId.Prop;
            if (id != 0) {
                return PropManager.instance.m_props.m_buffer[id].Info?.name ?? "N/A";
            }

            return "N/A";
        }

        public static string GetTreeAssetName(this InstanceID instanceId)
        {
            var id = instanceId.Tree;
            if (id != 0) {
                return TreeManager.instance.m_trees.m_buffer[id].Info?.name ?? "N/A";
            }

            return "N/A";
        }

        public static string GetCitizenAssetName(this InstanceID instanceId)
        {
            var citizenInstanceId = GetCitizenInstanceId(instanceId);
            if (citizenInstanceId != 0) {
                return CitizenManager.instance.m_instances.m_buffer[citizenInstanceId].Info?.name ?? "N/A";
            }

            return "N/A";
        }

        public static ushort GetCitizenInstanceId(this InstanceID instanceId)
        {
            var result = instanceId.CitizenInstance;
            if (result == 0) {
                var citizenId = instanceId.Citizen;
                if (citizenId != 0) {
                    result = CitizenManager.instance.m_citizens.m_buffer[citizenId].m_instance;
                }
            }

            return result;
        }

        public static uint GetCitizenId(this InstanceID instanceId)
        {
            var result = instanceId.Citizen;
            if (result == 0) {
                var citizenInstanceId = instanceId.CitizenInstance;
                if (citizenInstanceId != 0) {
                    result = CitizenManager.instance.m_instances.m_buffer[citizenInstanceId].m_citizen;
                }
            }

            return result;
        }
    }
}