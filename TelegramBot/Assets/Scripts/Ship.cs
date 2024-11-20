using System.Collections;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UnityEngine;

[System.Serializable]
public class Ship
{
    private static int idCounter = 0; // Contador global de barcos.
    public int id; // Identificador unico del barco.
    public string name; // Nombre del barco.
    public ShipType type; // Tipo de barco.
    public int capacity; // Capacidad del barco.
    public float speed; // Velocidad del barco.

    public Vector2 position; // Posicion en el mundo del barco.
    public Vector2 destination; // Destino del barco.
    public ShipPlace place = ShipPlace.Port; // Lugar donde esta el barco.

    /// <summary>
    /// Construcor de barco en una ubicaion especifica.
    /// </summary>
    /// <param name="type">Tipo de barco a construir.</param>
    /// <param name="position">Posicion en la que se construira el barco</param>
    public Ship(ShipType type, Vector2 position)
    {
        id = ++idCounter;
        this.type = type;
        this.position = position;
        SetShipForType(type);
    }

    /// <summary>
    /// Construye un plano de barco 
    /// </summary>
    /// <param name="type">Tipo de barco a mostrar</param>
    private Ship(ShipType type)
    {
        this.type = type;
        SetShipForType(type);
    }

    /// <summary>
    /// Establece las estadisticas de un barco segun su tipo.
    /// </summary>
    /// <param name="shipType">Tipo de barco</param>
    private void SetShipForType(ShipType shipType)
    {
        if (shipType == ShipType.Sloop)
        {
            capacity = 10;
            speed = 0.1f;
        }
        else if (shipType == ShipType.Brigantine)
        {
            capacity = 20;
            speed = 0.2f;
        }
        else if (shipType == ShipType.Frigate)
        {
            capacity = 30;
            speed = 0.3f;
        }
        else if (shipType == ShipType.Galleon)
        {
            capacity = 40;
            speed = 0.4f;
        }
    }

    /// <summary>
    /// Obtener el plano de un barco por su tipo.
    /// </summary>
    /// <param name="type">Tipo de barco</param>
    /// <returns>Retorna un barco</returns>
    public static Ship GetShipPlaneForType(ShipType type)
    {
        return new Ship(type);
    }

    /// <summary>
    /// Mueve la posision de un barco en el mundo segun su velocidad hasta que llega a su destino
    /// </summary>
    /// <param name="player">Jugador que mueve el barco</param>
    public static void MoveBoat(Player player)
    {
        player.locationShip.position = Vector2.MoveTowards(player.locationShip.position, player.locationShip.destination, player.locationShip.speed);

        if (player.locationShip.position == player.locationShip.destination)
        {
            TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, $"Has llegado a tu destino.", Keyboard.GetKeyboard(player));

            DestinyAction(player);
        }
    }

    /// <summary>
    /// Envia un mensaje al jugador de las acciones disponibles en el destino.
    /// </summary>
    /// <param name="player">Jugador que llega al destino.</param>
    public static void DestinyAction(Player player)
    {
        player.locationShip.place = ShipPlace.Sea;

        var island = GameData.Instance.GetIsland(player.locationShip.position.ToString());

        var destiny = Island.TryGetIslandOrCity(island);

        if (destiny == Destiny.City)
        {
            MessageDisembarkNearbyCity(island, player);

            Player.AddDestination(player, island);
        }
        else if (destiny == Destiny.Island)
        {
            MessageDisembarkNearbyIsland(island, player);

            Player.AddDestination(player, island);
        }
    }

    /// <summary>
    /// Envia un mensaje al jugador de que llego a un destino que tiene una isla con ciudad.
    /// </summary>
    /// <param name="island">Isla destino.</param>
    /// <param name="player">Jugador que llega al destino.</param>
    public static void MessageDisembarkNearbyCity(Island island, Player player)
    {
        var messsage = $"\nHay una ciudad cerca" +
                           $"\n{island.city.name}";

        TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, messsage,
                new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Disembark  -20 Gold", $"DisembarkCity{island.id}")));

    }

    /// <summary>
    /// Envia un mensaje al jugador de que llego a un destino con una isla decierta.
    /// </summary>
    /// <param name="island">Isla destino.</param>
    /// <param name="player">Jugador que llega al destino.</param>
    public static void MessageDisembarkNearbyIsland(Island island, Player player)
    {
        var messsage = $"\nHay una isla cerca";


        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, messsage, Keyboard.GetKeyboard(player));

    }

    /// <summary>
    /// Leva el ancla del barco para que el barco avanze hacia su destino.
    /// </summary>
    /// <param name="message"></param>
    public static void WeighAnchor(Player player)
    {
        if (!GameManager.playersAtSea.Contains(player))
        {
            GameManager.playersAtSea.Add(player);
            var island = GameData.Instance.GetIsland(player.locationShip.position.ToString());
            Port.GetPort(island.city.buildings).Boats.Remove(player.locationShip);
        }

        player.locationShip.place = ShipPlace.Sailing;
        player.action = PlayerAction.None;
        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID,
            $"Has levado anclas y comienza el viaje.\nTiempo estimado " +
            $"{CalculateArrivalTime(player.locationShip.position, player.locationShip.destination, player.locationShip.speed)}",
            Keyboard.GetKeyboard(player));

    }

    /// <summary>
    /// Baja el ancla del barco y detiene el barco en el mar.
    /// </summary>
    /// <param name="message"></param>
    public static void LowerAnchor(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);
        player.locationShip.place = ShipPlace.Sea;
        player.action = PlayerAction.None;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(message.Chat.Id,
            $"Echaste el ancla y el barco se detuvo.", Keyboard.GetKeyboard(player));
    }

    /// <summary>
    /// Retorna al menu principal del barco.
    /// </summary>
    /// <param name="message"></param>
    public static void Back(Player player)
    {
        player.action = PlayerAction.None;
        Player.SendDashboard(player);
    }


    /// <summary>
    /// Establece el rumbo del barco.
    /// </summary>
    /// <param name="message">Recibe el mensaje del nuevo rumbo del barco.</param>
    public static void SetCourse(Player player, Vector2 destination)
    {
        player.locationShip.destination = destination;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID,
            $"Rumbo establecido hacia /c{destination.x}x{destination.y}\n" +
            $"{CalculateArrivalTime(player.locationShip.position, player.locationShip.destination, player.locationShip.speed)}"
            , Keyboard.replyKeyboardShipCourse);
    }

    /// <summary>
    /// Entra al menu para establecer rumbo del barco.
    /// </summary>
    /// <param name="message"></param>
    public static void CourseButton(Player player)
    {

        player.action = PlayerAction.SetCourse;
        SortPlayerDestinationsByDistance(player);

        var destinationsMessage = "Destinos\n";

        foreach (var item in player.destinations)
        {
            var island = GameData.Instance.GetIsland(item.ToString());

            var destino = Island.TryGetIslandOrCity(island);
            if (destino == Destiny.City)
            {
                destinationsMessage += $"\n{island.city.name} /c{island.position.x}x{island.position.y} " +
                       $"T: {CalculateArrivalTime(player.locationShip.position, island.position, player.locationShip.speed)}";
            }
            else if (destino == Destiny.Island)
            {
                destinationsMessage += $"\nIsla Desierta /c{island.position.x}x{island.position.y}" +
                        $" T: {CalculateArrivalTime(player.locationShip.position, island.position, player.locationShip.speed)}";
            }
        }

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, destinationsMessage, Keyboard.replyKeyboardShipCourse);
    }

    /// <summary>
    /// Ordena los destinos del del jugador por distancia.
    /// </summary>
    /// <param name="player">Jugador con la lista a ordenar.</param>
    public static void SortPlayerDestinationsByDistance(Player player)
    {
        player.destinations.Sort((isla1, isla2) =>
        {
            float distancia1 = Vector2.Distance(player.locationShip.position, isla1);
            float distancia2 = Vector2.Distance(player.locationShip.position, isla2);
            return distancia1.CompareTo(distancia2);
        });
    }


    /// <summary>
    /// Calcula el tiempo de llegada de una posision a otra segun la velocidad.
    /// </summary>
    /// <param name="playerPosition">Posision inicial</param>
    /// <param name="destination">Posision del destino</param>
    /// <param name="speed">Velocidad de movimiento.</param>
    /// <returns></returns>
    public static string CalculateArrivalTime(Vector2 playerPosition, Vector2 destination, float speed)
    {
        var distance = Vector2.Distance(playerPosition, destination);
        float segundos = distance / speed;
        int horas;
        int minutos;


        if (segundos > 60 * 60)
        {
            horas = (int)segundos / (60 * 60);
            minutos = (int)segundos % (60 * 60) / 60;
            segundos = (int)segundos % (60 * 60) % 60;

            return $"{horas}h{minutos}m{segundos}s";
        }
        else if (segundos > 60)
        {
            minutos = (int)segundos / 60;
            segundos = (int)segundos % 60;

            return $"{minutos}m{segundos}s";
        }
        else
        {
            return $"{(int)segundos}s";
        }
    }

    /// <summary>
    /// Desembarca en el puerto de la ciudad.
    /// </summary>
    /// <param name="callbackQuery"></param>
    public static void DisembarkCity(CallbackQuery callbackQuery)
    {
        var island = callbackQuery.Data.Replace("DisembarkCity", "");
        var player = GameData.Instance.GetPlayerData(callbackQuery.Message.Chat.Id);
        if(player.place == PlayerPlace.Ship && player.action == PlayerAction.None)
        {
            if (island == player.locationShip.position.ToString())
            {
                //restar dinero.
                player.locationShip.place = ShipPlace.Port;
                player.place = PlayerPlace.Port;
                player.locationIsland = GameData.Instance.GetIsland(island);
                GameManager.playersAtSea.Remove(player);
                Port.GetPort(player.locationIsland.city.buildings).Boats.Add(player.locationShip);
                player.locationShip = null;

                TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(callbackQuery.Message.Chat.Id, "Estas en el puerto.", Keyboard.GetKeyboard(player));
            }
        }
        

    }
}
