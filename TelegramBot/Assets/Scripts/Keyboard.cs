using System.Collections;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using UnityEngine;

public class Keyboard 
{
    // Teclado que se muestra cuando el jugador esta en una ciudad.
    public static ReplyKeyboardMarkup replyKeyboardCity = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "Facilities", "Missions", "My Ships" },
        new KeyboardButton[] { "Crew", "Inventory", "Dashboard" }
    })
    {
        ResizeKeyboard = true
    };
    //Teclado que se muestra cuando el jugador esta en un puerto.
    public static ReplyKeyboardMarkup replyKeyboardPort = new ReplyKeyboardMarkup(new[]
    {
         new KeyboardButton[] { "Board" },
        new KeyboardButton[] { "Buy Ship", "Sell Ship", "Repair" },
        new KeyboardButton[] { "Jobs", "More", "Exit" }
    })
    {
        ResizeKeyboard = true
    };
    //Teclado que se muestra cuando el jugador esta en el barco.
    public static ReplyKeyboardMarkup replyKeyboardShip = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "Facilities", "Missions", "My Ships" },
        new KeyboardButton[] { "Crew", "Inventory", "Dashboard" },
        new KeyboardButton[] { "Course", "Inventory", "Disembark" }
    })
    {
        ResizeKeyboard = true
    };
    //Teclado que se muestra cuando el jugador esta esteblaciendo el curso del barco.
    public static ReplyKeyboardMarkup replyKeyboardShipCourse = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "Weigh Anchor", "Lower Anchor", "Back" }
    })
    {
        ResizeKeyboard = true
    };

    //InlineKeyboard.

    /// <summary>
    /// Genera un teclado inline de las instalaciones disponibles para el jugador.
    /// </summary>
    /// <param name="buildings">Recibe las construcciones de una ciudad o barco.</param>
    /// <returns>Retorna un teclado inline con las instalciones disponibles.</returns>
    public static InlineKeyboardMarkup GenerateInlineKeyboardFacilities(List<Building> buildings)
    {
        var inlineKeyboardButtons = new List<InlineKeyboardButton>();
        var buttonCount = 0;

        foreach (var build in buildings)
        {
            if (build is Port)
            {
                var button = InlineKeyboardButton.WithCallbackData((buttonCount + 1).ToString(), "GoToPort");
                inlineKeyboardButtons.Add(button);
                buttonCount++;
            }
        }
        
        
        return new InlineKeyboardMarkup(inlineKeyboardButtons);
    }
    /// <summary>
    /// Genera un teclado inline con un boton para cada barco en venta disponible.
    /// </summary>
    /// <param name="boats">Recibe una lista de barcos.</param>
    /// <returns>Regresa el teclado inline con los barcos disponibles.</returns>
    public static InlineKeyboardMarkup GenerateInlineKeyboardBoatsForSale(List<ShipType> boats)
    {
        var inlineKeyboardButtons = new List<InlineKeyboardButton>();
        var buttonCount = 0;
        foreach (var ship in boats)
        {
            var button = InlineKeyboardButton.WithCallbackData((buttonCount + 1).ToString(), "BuyBoat" + boats[buttonCount].ToString());
            inlineKeyboardButtons.Add(button);
            buttonCount++;
        }

        return new InlineKeyboardMarkup(inlineKeyboardButtons);
    }
    /// <summary>
    /// Genera un teclado inline con un boton para cada barco del jugador disponible para abordar.
    /// </summary>
    /// <param name="boats">Recibe una lista de barcos</param>
    /// <returns>Retorna un teclado inline.</returns>
    public static InlineKeyboardMarkup GenerateInlineKeyboardMyBoat(List<Ship> boats)
    {
        var inlineKeyboardButtons = new List<InlineKeyboardButton>();
        var buttonCount = 0;
        foreach (var ship in boats)
        {
            var button = InlineKeyboardButton.WithCallbackData((buttonCount + 1).ToString(), "MyBoat" + buttonCount);
            inlineKeyboardButtons.Add(button);
            buttonCount++;
        }

        return new InlineKeyboardMarkup(inlineKeyboardButtons);
    }

    public static InlineKeyboardMarkup GenerateInlineKeyboardJobs(List<Mission> jobs)
    {
        var inlineKeyboardButtons = new List<InlineKeyboardButton>();
        var buttonCount = 0;
        foreach (var job in jobs)
        {
            var button = InlineKeyboardButton.WithCallbackData((buttonCount + 1).ToString(), "GetJob" + job.missionLocation + job.missionType);
            inlineKeyboardButtons.Add(button);
            buttonCount++;
        }

        return new InlineKeyboardMarkup(inlineKeyboardButtons);
    }

    /// <summary>
    /// Comprueba la posicion del jugador y retorna el teclado correspondiente.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static ReplyKeyboardMarkup GetKeyboard(Player player)
    {
        if (player.place == PlayerPlace.City)
        {
            return Keyboard.replyKeyboardCity;
        }
        else if (player.place == PlayerPlace.Port)
        {
            return Keyboard.replyKeyboardPort;
        }
        else if (player.place == PlayerPlace.Ship)
        {
            return Keyboard.replyKeyboardShip;
        }

        return null;

    }
}
