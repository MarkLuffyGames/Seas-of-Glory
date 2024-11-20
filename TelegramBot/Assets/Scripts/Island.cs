using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Island
{
    public string name;
    public string id;
    public Vector2 position;
    public City city;

    public Island(string name, Vector2 position, City city = null)
    {
        id = position.ToString();
        this.name = name;
        this.position = position;
        this.city = city;
    }

    /// <summary>
    /// Comprueba si la isla existe o si tiene ciudad.
    /// </summary>
    /// <param name="island">Isala que deamos comprobar.</param>
    /// <returns>Retorna tipo de destino.</returns>
    public static Destiny TryGetIslandOrCity(Island island)
    {

        if (island != null)
        {
            if (island.city != null)
            {
                if (island.city.name != null)
                {
                    if (island.city.name != "")
                    {
                        return Destiny.City;
                    }
                }
            }
            return Destiny.Island;
        }
        else
        {
            return Destiny.Sea;
        }
    }
}
