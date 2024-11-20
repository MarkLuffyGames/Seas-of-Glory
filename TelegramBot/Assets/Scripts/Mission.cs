using System.Collections;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using Unity.VisualScripting;
using UnityEngine;

public enum MissionType { GrainDelivery }

[System.Serializable]
public class Mission
{
    public MissionType missionType;
    public Vector2 missionLocation;
    public bool planning = false;
    public float timeMission;
    public int amount;
    public Mission()
    {

    }

    public static void PlanMission(Mission mission, Player player, int boatNumber = -1, int amount = 0)
    {

        if(mission.planning)
        {
            
            if (mission is Delivery)
            {
                string message;
                City cityMission = GameData.Instance.GetIsland(mission.missionLocation.ToString()).city;
                Delivery delivery = (Delivery)mission;

                message = delivery.message;
                if(boatNumber != -1) delivery.carrierShipID = player.AvailableBoats[boatNumber].id;

                if (GameData.Instance.GetShip(delivery.carrierShipID).type == ShipType.None)
                {
                    message += $"📏{(int)Vector2.Distance(player.locationIsland.position, cityMission.position)} ⏳{Ship.CalculateArrivalTime(player.locationIsland.position, mission.missionLocation, 0.1f)}";
                    message += $"\n\nElige en qué barco transportarás la mercancía";

                    Player.UpdateAvailableShip(player);

                    var count = 1;
                    foreach (var boat in player.AvailableBoats)
                    {
                        message += $"\n{count}- {boat.type} {boat.name} Capacity {boat.capacity}\n";
                        count++;
                    }

                    TelegramBotController.Instance.SendMessageAsyncInlineKeyboardMarkup(player.playerID, message, Keyboard.GenerateInlineKeyboardMyBoat(player.AvailableBoats));
                }
                else if(amount == 0)
                {
                    var carrierShip = GameData.Instance.GetShip(delivery.carrierShipID);

                    delivery.message += $"\n\nBarco de transporte:{carrierShip.type} {carrierShip.name} Capacity {carrierShip.capacity}";

                    message = delivery.message +
                        $"\n\nPrecio por unidad: 10" +
                                $"\n\nAgrega una cantidad para comenzar la mision" +
                                 $"\nMax /add{Mathf.Min(carrierShip.capacity, player.locationIsland.city.grains)}";


                    TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, message,
                        new ReplyKeyboardMarkup(new KeyboardButton[] { "Cancel" })
                        {
                            ResizeKeyboard = true
                        });
                }
                else
                {
                    delivery.amount = amount;
                    delivery.message += $"\n\nCantidad a entregar: {delivery.amount}";

                    TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, delivery.message,
                        new ReplyKeyboardMarkup(new KeyboardButton[] { "Accept" ,"Cancel" })
                        {
                            ResizeKeyboard = true
                        });
                }
            }
            
        }
        else
        {
            mission.planning = true;
            player.missionInPlannin = mission;
            player.action = PlayerAction.PlanningDelivery;

            TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Planning Mission",
                new ReplyKeyboardMarkup(new KeyboardButton[] { "Cancel" })
                {
                    ResizeKeyboard = true
                });

            PlanMission(mission, player);
        }

    }
    public static void GiveThePlayerMission(Mission mission, Player player)
    {
        var city = GameData.Instance.GetIsland(mission.missionLocation.ToString()).city;
        player.missions.Add(mission);
        mission.timeMission = Vector2.Distance(player.locationIsland.position, mission.missionLocation) / 0.1f;

        player.action = PlayerAction.None;
        if(mission.missionType == MissionType.GrainDelivery)
        {
            city.incomingGrains += mission.amount;
        }
        mission.planning = false;
        player.missionInPlannin = null;
        city.myOrders.Remove(mission);

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Mision aceptada", Keyboard.GetKeyboard(player));

    }

    public static void CancelMission(Mission mission, Player player)
    {
        var city = GameData.Instance.GetIsland(mission.missionLocation.ToString()).city;
        city.myOrders.Remove(mission);
        city.RequestResources();
        player.missionInPlannin = null;
        player.action = PlayerAction.None;

        TelegramBotController.Instance.SendMessageAsyncReplyKeyboardMarkup(player.playerID, "Mision cancelada", Keyboard.GetKeyboard(player));

    }
}
