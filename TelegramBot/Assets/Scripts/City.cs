using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public enum Resources { food , wood , stone , iron , cloth }

[System.Serializable]
public class City
{
    public string name;
    [SerializeReference] public List<Building> buildings = new List<Building>();
    public Vector2 position;

    //Recursos
    public int grains;
    public int incomingGrains;

    public int flour;
    public int incomingFlour;
    public int bread;

    public bool needGrains = false;
    public bool needFlour = false;
    public bool producesGrains = false;
    public bool producesFlour = false;

    public bool isAutomatic = true;


    public List<Mission> myOrders = new List<Mission>();

    public City(string name, Vector2 position)
    {
        this.name = name;
        this.position = position;
    }

    /// <summary>
    /// Genera recursos para las ciudades.
    /// </summary>
    /// <param name="city"></param>
    public void Produce()
    {
        foreach (Building building in buildings)
        {
            building.Produce(this);
        }

        RequestResources();
    }

    public void RequestResources()
    {
        if(!isAutomatic) return;

        needGrains = false;
        needFlour = false;
        producesGrains = false;
        producesFlour = false;

        foreach (Building building in buildings)
        {
            if (building is GrainFarm)
            {
                producesGrains = true;
            }
            if (building is Mill)
            {
                needGrains = true;
                producesFlour = true;
            }
        }

        if (grains + incomingGrains < 10000)
        {
            if(needGrains && !producesGrains)
            {
                bool contain = false;
                foreach(var order in myOrders)
                {
                    if(order is Delivery)
                    {
                        contain = true;
                    }
                }

                if (!contain) 
                {
                    myOrders.Add(new Delivery(MissionType.GrainDelivery, position));
                } 
            }

            if(needFlour && !producesFlour)
            {
                bool contain = false;
                foreach( var order in myOrders)
                {
                    if(order is Delivery)
                    {
                        contain = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Envia un mensaje con las instalaciones disponibles.
    /// </summary>
    /// <param name="id"></param>
    public static void SendFacilities(Player player)
    {
        var message = $"Facilities\n" +
            $"\n1. Puerto" +
            $"\n2. Taberna";

        TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, Keyboard.GenerateInlineKeyboardFacilities(player.locationIsland.city.buildings));
    }

    /// <summary>
    /// Mueve al jugdor al puerto de la ciudad correspondiente.
    /// </summary>
    /// <param name="id"></param>
    public static void GoToPort(Player player)
    {
        player.place = PlayerPlace.Port;
        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Estas en el puerto.", Keyboard.GetKeyboard(player));
    }

}
