using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

public class Bakery : Building
{
    public int productionLevel = 1;
    public int flourProcessedPerCycle = 5;

    public int breadPerUnitFlour = 5;
    public int breadPrice = 4;

    public int processing;
    public int warehouseSize = 10000;
    public int amountOfFlour;

    /// <summary>
    /// Produce pan.
    /// </summary>
    /// <param name="city">ciudad a la que debe producir.</param>
    public override void Produce(City city)
    {
        city.bread += processing;
        processing = 0;

        Supply(city);

        if (city.bread < 10000)
        {
            var necessaryflour = flourProcessedPerCycle * productionLevel;
            if (amountOfFlour >= necessaryflour)//Tambien comprobar aqui que alla dinero suficiente.
            {
                amountOfFlour -= necessaryflour;
                //Restar el dinero requerido.
                processing = necessaryflour * breadPerUnitFlour;
            }
        }
    }

    public void Supply(City city)
    {
        if (city.isAutomatic)
        {
            int quantitySupply = (productionLevel * flourProcessedPerCycle) * 2;

            quantitySupply = city.flour >= quantitySupply ? quantitySupply : city.flour;

            quantitySupply = warehouseSize - amountOfFlour < quantitySupply ? warehouseSize - amountOfFlour : quantitySupply;

            city.flour -= quantitySupply;
            amountOfFlour += quantitySupply;
        }
    }
}
