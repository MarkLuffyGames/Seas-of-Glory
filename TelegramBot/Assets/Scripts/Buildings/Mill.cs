using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mill : Building
{
    public int productionLevel = 1;
    public int grainProcessedPerCycle = 40;

    public int grainsNeededPerUnitFlour = 4;
    public int flourPrice = 20;

    public int processing;
    public int warehouseSize = 10000;
    public int amountOfGrains; 

    /// <summary>
    /// Produce harina.
    /// </summary>
    /// <param name="city">ciudad a la que debe producir.</param>
    public override void Produce(City city)
    {
        city.flour += processing;
        processing = 0;

        Supply(city);

        if (city.flour < 10000)
        {
            var necessaryGrains = grainProcessedPerCycle * productionLevel;
            if (amountOfGrains >= necessaryGrains)//Tambien comprobar aqui que alla dinero suficiente.
            {
                amountOfGrains -= necessaryGrains;
                //Restar el dinero requerido.
                processing = necessaryGrains / grainsNeededPerUnitFlour;
            }
        }
    }

    public void Supply(City city)
    {
        if (city.isAutomatic)
        {
            int quantitySupply = (productionLevel * grainProcessedPerCycle) * 2;

            quantitySupply = city.grains >= quantitySupply ? quantitySupply : city.grains;

            quantitySupply = warehouseSize - amountOfGrains < quantitySupply ? warehouseSize - amountOfGrains : quantitySupply;

            city.grains -= quantitySupply;
            amountOfGrains += quantitySupply;
        }
    }
}
