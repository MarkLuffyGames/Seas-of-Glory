using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using UnityEditor.Search;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Destiny
{
    Sea, Island, City
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameData gameData;
    [SerializeField] private TelegramBotController telegramBotController;

    public GameObject island;
    public GameObject Island1;

    public static List<Player> playersAtSea = new List<Player>();

    private void Start()
    {
        gameData.LoadShipData();
        gameData.LoadIslandData();
        if(gameData.GetIslands().Count == 0)
        {
            gameData.GenerateIslands(1000);
        }
        gameData.LoadPlayerData();

        telegramBotController.OnStart += TelegramBotController_OnStart;
        telegramBotController.OnDashboard += TelegramBotController_OnDashboard;
        telegramBotController.OnFacilities += TelegramBotController_OnFacilities;
        telegramBotController.OnGoTo += TelegramBotController_OnGoTo;
        telegramBotController.OnBuyShipButton += TelegramBotController_OnBuyShipButton;
        telegramBotController.OnBuyShip += TelegramBotController_OnBuyShip;
        telegramBotController.OnMyShipButton += TelegramBotController_OnMyShipButton;
        telegramBotController.OnExit += TelegramBotController_OnExit;
        telegramBotController.OnBoardMyShip += TelegramBotController_OnBoardMyShip;
        telegramBotController.OnBoardButton += TelegramBotController_OnBoardButton;
        telegramBotController.OnCourseButton += TelegramBotController_OnCourseButton;
        telegramBotController.OnSetCourse += TelegramBotController_OnSetCourse;
        telegramBotController.OnWeighAnchor += TelegramBotController_OnWeighAnchor;
        telegramBotController.OnLowerAnchor += TelegramBotController_OnLowerAnchor;
        telegramBotController.OnBack += TelegramBotController_OnBack;
        telegramBotController.OnDisembarkCity += TelegramBotController_OnDisembarkCity;
        telegramBotController.OnJobsButton += TelegramBotController_OnJobsButton;
        telegramBotController.OnGetJob += TelegramBotController_OnGetJob;
        telegramBotController.OnMissionsButton += TelegramBotController_OnMissionButton;
        telegramBotController.OnAddAmount += TelegramBotController_OnAddAmount;
        telegramBotController.OnAccept += TelegramBotController_OnAccept;
        telegramBotController.OnCancel += TelegramBotController_OnCancel;
        telegramBotController.OnDisembark += TelegramBotController_OnDisembark;


        int cityIsland = 0;
        foreach(KeyValuePair<string, Island> island in gameData.GetIslands())
        {
            
            var destiny = Island.TryGetIslandOrCity(island.Value);
            if (destiny == Destiny.City)
            {
                Instantiate(this.island, island.Value.position, Quaternion.identity);
                cityIsland++;
            }
            else
            {
                Instantiate(Island1, island.Value.position, Quaternion.identity);
            }
        }
        Debug.Log("Cantidad de ciudades: " + cityIsland);
        telegramBotController.StartBot();
    }

    private void TelegramBotController_OnDisembark(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);

        if(player.place == PlayerPlace.Ship && player.action == PlayerAction.None)
        {
            Ship.DestinyAction(player);
        }
    }

    private void TelegramBotController_OnCancel(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);

        if (player.action == PlayerAction.PlanningDelivery)
        {
            Mission.CancelMission(player.missionInPlannin, player);
        }
        else if(player.action == PlayerAction.BuyingBoat)
        {
            Port.CancelPurchase(player);
        }
    }

    private void TelegramBotController_OnAccept(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);

        if (player.action == PlayerAction.PlanningDelivery)
        {
            Mission.GiveThePlayerMission(player.missionInPlannin, player);
        }
    }

    private void TelegramBotController_OnAddAmount(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        int amount;
        bool success = int.TryParse(message.Text.Replace("/add", ""), out amount);
        if (success)
        {
            if (player.action == PlayerAction.PlanningDelivery)
            {
                Mission.PlanMission(player.missionInPlannin, player, -1, amount);
            }
        }

        
    }

    private void TelegramBotController_OnMissionButton(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        Player.SendPlayerMission(player);
    }

    private void TelegramBotController_OnGetJob(CallbackQuery query)
    {
        var player = gameData.GetPlayerData(query.Message.Chat.Id);
        if(player.place == PlayerPlace.Port)
        {

        }

        var data = query.Data.Replace("GetJob", "");

        int startIdx = data.IndexOf('(');
        int endIdx = data.IndexOf(')');

        if (startIdx != -1 && endIdx != -1)
        {
            // Extraer las coordenadas incluyendo los paréntesis
            string coordinates = data.Substring(startIdx, endIdx - startIdx + 1);

            // Extraer el tipo de misión
            string missionTypeString = data.Substring(endIdx + 1).Trim();

            // Convertir el tipo de misión a un valor del enumerado
            if (Enum.TryParse(missionTypeString, out MissionType missionType))
            {
                var island = gameData.GetIsland(coordinates);
                bool exist = false;
                foreach (var item in island.city.myOrders)
                {
                    if(item.missionType == missionType && !item.planning)
                    {
                        Debug.Log("La mision esta disponible.");
                        Mission.PlanMission(item, player);
                        exist = true;
                        break;
                        
                    }
                }

                if (!exist) 
                {
                    telegramBotController.SendMessageAsyncReplyKeyboardMarkup(query.Message.Chat.Id, "La mision solicitada ya no esta disponible", Keyboard.GetKeyboard(player));
                }
            }
        }
    }

    private void TelegramBotController_OnJobsButton(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        if(player.place == PlayerPlace.Port && player.action == PlayerAction.None)
        {
            Port.GetJobs(player, gameData.GetCities());
        }
        
    }

    private void TelegramBotController_OnDisembarkCity(CallbackQuery query)
    {
        Ship.DisembarkCity(query);
    }

    private void TelegramBotController_OnBack(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        if(player.action == PlayerAction.SetCourse)
        {
            Ship.Back(player);
        }
        
    }

    private void TelegramBotController_OnLowerAnchor(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        if (player.place == PlayerPlace.Ship)
        {
            Ship.LowerAnchor(message);
        }
        
    }

    private void TelegramBotController_OnWeighAnchor(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);
        if(player.place == PlayerPlace.Ship)
        {
            Ship.WeighAnchor(player);
        }
        
    }

    private void TelegramBotController_OnSetCourse(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        if (player.action == PlayerAction.SetCourse)
        {
            string text = message.Text.Replace("/c", "");

            string[] Coordinates = text.Split('x');

            int x = int.Parse(Coordinates[0]);
            int y = int.Parse(Coordinates[1]);

            Vector2 destination = new Vector2(x, y);

            Ship.SetCourse(player, destination);
        }
    }

    private void TelegramBotController_OnCourseButton(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);

        if(player.place == PlayerPlace.Ship && player.action == PlayerAction.None)
        {
            Ship.CourseButton(player);
        }
        
    }

    private void TelegramBotController_OnBoardButton(Message message)
    {
        var player = gameData.GetPlayerData(message.Chat.Id);

        if(player.place == PlayerPlace.Port && player.action == PlayerAction.None)
        {
            Player.SendAvailableShip(player);
        }
        
    }

    private void TelegramBotController_OnBoardMyShip(CallbackQuery query)
    {
        var player = gameData.GetPlayerData(query.Message.Chat.Id);

        int number;
        bool success = int.TryParse(query.Data.Replace("MyBoat", ""), out number);
        if (success)
        {
            if (player.action == PlayerAction.None && player.place == PlayerPlace.Port)
            {
                Player.BoardMyShip(player, number);
            }
            else if (player.action == PlayerAction.PlanningDelivery && player.place == PlayerPlace.Port)
            {
                Mission.PlanMission(player.missionInPlannin, player, number);
            }
        }
        else
        {
            Debug.Log("Error en la conversión.");
        }
        Debug.Log(query.Data);

    }

    private void TelegramBotController_OnExit(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        if(player.action == PlayerAction.None)
        {
            Building.Exit(player);
        }
        
    }

    private void TelegramBotController_OnMyShipButton(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        Player.SendMyShip(player);
    }

    private void TelegramBotController_OnBuyShipButton(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        if(player.place == PlayerPlace.Port && player.action == PlayerAction.None)
        {
            Port.BuyShipButton(player);
        }
        
    }

    private void TelegramBotController_OnFacilities(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        if (player.place == PlayerPlace.City && player.action == PlayerAction.None)
        {
            City.SendFacilities(player);
        }
        
    }

    private void TelegramBotController_OnDashboard(Message message)
    {
        var player = GameData.Instance.GetPlayerData(message.Chat.Id);

        Player.SendDashboard(player);
    }

    private float moveInterval = 1.0f;
    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > moveInterval)
        {
            //Mueve los barcos en el mar que no tengan el ancla echada.
            foreach (var player in playersAtSea)
            {
                if (player.locationShip.place == ShipPlace.Sailing)
                {
                    Ship.MoveBoat(player);
                }
            }

            var cities = gameData.GetCities();

            foreach (var city in cities)
            {
                city.Produce();
            }
            timer = 0;
        }
        
    }

    void OnApplicationQuit()
    {
        telegramBotController.StopBot().Wait(); // Espera hasta que el bot se detenga completamente
        gameData.SaveShipData().Wait();
        gameData.SaveIslandData().Wait();
        gameData.SavePlayerData().Wait();
    }

    private void TelegramBotController_OnBuyShip(CallbackQuery callbackquery)
    {
        var player = gameData.GetPlayerData(callbackquery.Message.Chat.Id);
        if (player == null) return;

        if(player.place == PlayerPlace.Port && player.action == PlayerAction.BuyingBoat)
        {
            if (Enum.TryParse(callbackquery.Data.Replace("BuyBoat", ""), out ShipType shipType))
            {
                Port.BuyShip(player, shipType);
            }
            else
            {
                Console.WriteLine("Error en la conversión.");
            }
        }
    }

    

    private void TelegramBotController_OnGoTo(CallbackQuery callbackquery)
    {
        var player = gameData.GetPlayerData(callbackquery.Message.Chat.Id);
        if (player == null) return;

        string message = callbackquery.Data.Replace("GoTo", "");
        if (message == "Port")
        {
            if (player.place == PlayerPlace.City && player.action == PlayerAction.None)
            {
                City.GoToPort(player);
            }
        }
    }

    

    private void TelegramBotController_OnStart(Message message)
    {
        if (gameData.PlayerExists(message.Chat.Id))
        {
            var player = gameData.GetPlayerData(message.Chat.Id);

            telegramBotController.SendMessageAsyncReplyKeyboardMarkup(message.Chat.Id, "¡Bienvenido de nuevo!", Keyboard.GetKeyboard(player));
            Player.SendDashboard(player);
        }
        else
        {
            if (string.IsNullOrEmpty(message.Chat.Username))
            {
                telegramBotController.SendMessageAsyncReplyKeyboardMarkup(message.Chat.Id, "Debe tener un nombre de usuario para jugar.", null);
                return;
            }

            Player player = new Player(message.Chat.Id, message.Chat.Username, RandomCity());

            Player.AddDestination(player, player.locationIsland);
            gameData.AddPlayer(player);

            telegramBotController.SendMessageAsyncReplyKeyboardMarkup(message.Chat.Id, "¡Bienvenido al juego!", Keyboard.GetKeyboard(player));
        }


    }

    private Island RandomCity()
    {
        var islands = gameData.GetIslandsList();
        var random = Random.Range(1,islands.Count);

        var destiny = Island.TryGetIslandOrCity(islands[random]);

        if (destiny == Destiny.City)
        {
            return islands[random];
        }
        return RandomCity();
    }
}
