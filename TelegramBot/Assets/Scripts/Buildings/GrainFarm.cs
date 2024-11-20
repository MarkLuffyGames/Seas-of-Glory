using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrainFarm : Building
{
    public int productionLevel = 1;
    public int grainProducedPerCycle = 50;
    public int grainPrice = 5;

    public int processing;

    /// <summary>
    /// produce granos.
    /// </summary>
    /// <param name="city">ciudad a la que debe producir.</param>
    public override void Produce(City city)
    {
        city.grains += processing;
        processing = 0;

        if(city.grains < 10000)//Tambien comprobar que alla dinero suficiente.
        {
            //Restar el diner requerido.
            processing = grainProducedPerCycle * productionLevel;
        }
    }
}
