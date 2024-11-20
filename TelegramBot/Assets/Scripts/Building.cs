using System.Collections;
using System.Collections.Generic;
using Telegram.Bot.Types;
using UnityEngine;

[System.Serializable]
public class Building
{
    public string name;

    /// <summary>
    /// Regrasa al jugador a la ciudad donde estaba, saliendo de cualquier instalacion.
    /// </summary>
    /// <param name="message"></param>
    public static void Exit(Player player)
    {
        if (player.place == PlayerPlace.Port)
        {
            player.place = PlayerPlace.City;
            TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Estas en la ciudad.", Keyboard.GetKeyboard(player));
        }
    }

    public virtual void Produce(City city) 
    { 
        
    }
}
