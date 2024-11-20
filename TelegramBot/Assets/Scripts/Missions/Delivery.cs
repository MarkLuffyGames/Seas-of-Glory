using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Delivery : Mission
{

    public string message;
    public long carrierShipID;

    public Delivery()
    {

    }
    public Delivery(MissionType missionType, Vector2 missionLocation)
    {
        this.missionType = missionType;
        this.missionLocation = missionLocation;

        var city = GameData.Instance.GetIsland(missionLocation.ToString()).city;

        message = $"Mission: 📦 de {GetIconByMissionType(missionType)}" +
                   $"Destino: {city.name} 📍/c{city.position.x}x{city.position.y} ";
    }

    private string GetIconByMissionType(MissionType missionType)
    {
        if(missionType == MissionType.GrainDelivery)
        {
            return "🌾";
        }
        else if(missionType == MissionType.GrainDelivery)
        {
            return "";
        }

        return null;
    }
}
