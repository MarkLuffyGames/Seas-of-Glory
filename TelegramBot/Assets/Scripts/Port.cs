using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

[System.Serializable]
public class Port : Building
{
    public string portName;
    public Vector2 position;
    public List<ShipType> BoatsForSale;
    public List<Ship> Boats;

    public Port(Vector2 position, List<ShipType> boatsForSale)
    {
        this.position = position;
        BoatsForSale = boatsForSale;
        Boats = new List<Ship>();
    }

    /// <summary>
    /// Muestra un mensaje con los barcos dispinibles para comprar.
    /// </summary>
    /// <param name="id">Chat ID del jugador.</param>
    public static void BuyShipButton(Player player)
    {
        var ships = GetPort(player.locationIsland.city.buildings).BoatsForSale;
        var message = "Boats for sale\n";
        var count = 1;

        foreach (var ship in ships)
        {
            message += $"\n{count}. {ship}\n" +
                $"Capacity {Ship.GetShipPlaneForType(ship).capacity}\n";
            count++;
        }
        player.action = PlayerAction.BuyingBoat;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Que barco te gustaria comprar?", new ReplyKeyboardMarkup(new KeyboardButton[] { "Cancel" })
        {
            ResizeKeyboard = true
        });
        TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, Keyboard.GenerateInlineKeyboardBoatsForSale(ships));

    }

    public static Port GetPort(List<Building> buildings)
    {
        foreach(var build in buildings)
        {
            if(build is Port)
            {
                return (Port)build;
            }
        }

        return null;
    }

    /// <summary>
    /// Realiza la compra de un barco por un jugador.
    /// </summary>
    /// <param name="id">Chat ID del jugador.</param>
    /// <param name="type">Tipo de barco que compra el jugador.</param>
    public static void BuyShip(Player player, ShipType type)
    {
        var purchasedBoats = new Ship(type, player.locationIsland.position);
        GameData.Instance.AddShip(purchasedBoats);
        player.OwnedBoats.Add(purchasedBoats);
        GetPort(player.locationIsland.city.buildings).Boats.Add(purchasedBoats);
        player.action = PlayerAction.None;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, $"Has comprado un {purchasedBoats.type}.", Keyboard.GetKeyboard(player));

    }
    public static void CancelPurchase(Player player)
    {
        player.action = PlayerAction.None;
        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, $"Compra cancelada.", Keyboard.GetKeyboard(player));

    }

    public static void GetJobs(Player player, List<City> cities)
    {
        var temporalList = new List<City>(cities);
        temporalList.Sort((city1, city2) =>
        {
            float distancia1 = Vector2.Distance(player.locationIsland.city.position, city1.position);
            float distancia2 = Vector2.Distance(player.locationIsland.city.position, city2.position);
            return distancia1.CompareTo(distancia2);
        });

        if (player.place == PlayerPlace.Port)
        {
            var jobs = new List<Mission>();
            string message = "Jobs:\n";
            int missionCount = 0;
            foreach (var city in temporalList)
            {
                if(city != player.locationIsland.city)
                {
                    foreach (var order in city.myOrders)
                    {
                        if(order is Delivery &&
                            !order.planning &&
                            player.locationIsland.city.producesGrains)
                        {
                            jobs.Add(order);
                            message += $"\n{++missionCount}- 📦 de 🌾 a {city.name} 📍/c{city.position.x}x{city.position.y}" +
                                $"\n📏{(int)Vector2.Distance(player.locationIsland.position, city.position)} ⏳{Ship.CalculateArrivalTime(player.locationIsland.position, order.missionLocation, 0.1f)}\n";
                        }
                    }
                }
            }
            

            TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, Keyboard.GenerateInlineKeyboardJobs(jobs));

        }
    }
}
