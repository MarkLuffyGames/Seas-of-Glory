using System.Collections;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Player
{
    

    public long playerID;
    public string playerName;
    public PlayerPlace place = PlayerPlace.City;
    public PlayerAction action = PlayerAction.None;

    public Island locationIsland = null;
    public Ship locationShip = null;

    public List<Ship> OwnedBoats = new List<Ship>();
    public List<Ship> AvailableBoats = new List<Ship>();

    public List<Vector2> destinations = new List<Vector2>();

    public Mission missionInPlannin;
    public List<Mission> missions = new List<Mission>();

    public Player(long Id, string name, Island location)
    {
        playerID = Id;
        playerName = name;
        locationIsland = location;
    }

    /// <summary>
    /// Envia un mensje con los datos del jugador.
    /// </summary>
    /// <param name="id">Chat ID del jugador.</param>
    public static void SendDashboard(Player player)
    {

        if (player != null)
        {
            string message = $"{player.playerName}\n" +
            $"Ubicacion: {GetLocationMessage(player)}";

            TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, message, Keyboard.GetKeyboard(player));

        }
    }

    /// <summary>
    /// Confecciona un menssaje con la ubicacion del jugador y la distancia hasta el destino o el tiempo dellegada estimado si esta moviendose el barco.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    static string GetLocationMessage(Player player)
    {
        if (player.place == PlayerPlace.Ship)
        {

            var distance = Vector2.Distance(player.locationShip.position, player.locationShip.destination);
            if (player.locationShip.place == ShipPlace.Sailing)
            {
                return $"Ship {(int)player.locationShip.position.x}x{(int)player.locationShip.position.y}" +
                $"\nTiempo estimado de llegada {Ship.CalculateArrivalTime(player.locationShip.position, player.locationShip.destination, player.locationShip.speed)}";
            }
            else
            {
                return $"Ship {(int)player.locationShip.position.x}x{(int)player.locationShip.position.y}" +
                $"\nDistancia hasta el destino {(int)distance}";
            }
        }
        else if (player.place == PlayerPlace.Island)
        {
            return $"Island {(int)player.locationIsland.position.x}x{(int)player.locationIsland.position.y}";
        }
        else
        {
            return $"{player.locationIsland.city.name} {(int)player.locationIsland.position.x}x{(int)player.locationIsland.position.y}";
        }
    }

    /// <summary>
    /// Agrega un destino a la listo de destinos del jugador.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="island"></param>
    public static void AddDestination(Player player, Island island)
    {
        if (!player.destinations.Contains(island.position))
        {
            player.destinations.Add(island.position);
        }
    }

    
    /// <summary>
    /// Envia un mensaje con los barcos en propiedad del jugador.
    /// </summary>
    /// <param name="id"></param>
    public static void SendMyShip(Player player)
    {
        var message = "My Ship\n";
        var count = 1;
        foreach (var boat in player.OwnedBoats)
        {
            message += $"\n{count}. {boat.type} {boat.name} \n" +
                $"Capacity {boat.capacity}\n" +
                $"Speed {boat.speed}" +
                $"Location {boat.position}";
            count++;
        }
        if (count == 1) message += "\nNone";
        TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, null);
    }


    /// <summary>
    /// Aborda el barco seleccionado por el jugador.
    /// </summary>
    /// <param name="callbackQuery"></param>
    public static void BoardMyShip(Player player, int boatNumber)
    {
        player.place = PlayerPlace.Ship;
        player.locationShip = player.AvailableBoats[boatNumber];
        player.locationIsland = null;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Estas a bordo del barco.", Keyboard.GetKeyboard(player));

    }

    /// <summary>
    /// Envia un mensaje con los barcos disponibles para abordar.
    /// </summary>
    /// <param name="id"></param>
    public static void SendAvailableShip(Player player)
    {
        var message = "My Ship\n";
        var count = 1;

        UpdateAvailableShip(player);

        foreach (var boat in player.AvailableBoats)
        {
            message += $"\n{count}- {boat.type} {boat.name} \n" +
                $"Capacity {boat.capacity}\n";
            count++;
        }
        if (count == 1) message += "\nNone";
        TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, Keyboard.GenerateInlineKeyboardMyBoat(player.AvailableBoats));
    }

    /// <summary>
    /// Actualiza la lista de barcos disponibles del jugador.
    /// </summary>
    /// <param name="player"></param>
    public static void UpdateAvailableShip(Player player)
    {
        player.AvailableBoats.Clear();
        foreach (var ship in player.OwnedBoats)
        {
            if (ship.position == player.locationIsland.position)
            {
                player.AvailableBoats.Add(ship);
            }
        }
    }

    public static void SendPlayerMission(Player player)
    {
        string message = "Missions:\n";
        int missionCount = 0;
        foreach (var mission in player.missions)
        {
            var island = GameData.Instance.GetIsland(mission.missionLocation.ToString());
            message += $"\n{++missionCount}- 📦 de 🌾 a {island.city.name} 📍/c{island.city.position.x}x{island.city.position.y}" +
                                $"\n📏{(int)Vector2.Distance(player.place == PlayerPlace.Ship ? player.locationShip.position : player.locationIsland.position, island.city.position)} ⏳{mission.timeMission}\n";

        }

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, message, Keyboard.GetKeyboard(player));
    }
}
