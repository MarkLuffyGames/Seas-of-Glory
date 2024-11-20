using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;


public enum PlayerPlace { Ship, Island, City, Port}
public enum PlayerAction { None, SetCourse, PlanningDelivery, BuyingBoat }
public enum ShipPlace { Sea, Port, Sailing }
public enum ShipType { None, Sloop, Brigantine, Frigate, Galleon }
public class GameData : MonoBehaviour
{
    public static GameData Instance;

    private string playersDataFilePath; 
    private string islandsDataFilePath;
    private string shipsDataFilePath;
    [SerializeField] private List<Player> playersDataList;
    private Dictionary<long, Player> playersData;
    [SerializeField] private List<Island> islandsList;
    private Dictionary<string, Island> islands;
    [SerializeField] private List<Ship> shipList;
    private Dictionary<long, Ship> shipsDictionary;

    [SerializeField] private List<City> cityList = new List<City>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        playersDataFilePath = Path.Combine(Application.persistentDataPath, "playersData.json");
        islandsDataFilePath = Path.Combine(Application.persistentDataPath, "islandsData.json");
        shipsDataFilePath = Path.Combine(Application.persistentDataPath, "shipData.json");
        playersData = new Dictionary<long, Player>();
        islands = new Dictionary<string, Island>();
        shipsDictionary = new Dictionary<long, Ship>();
    }

    /// <summary>
    /// Guarda los datos de todos los barcos en un fichero JSON.
    /// </summary>
    /// <returns></returns>
    public Task SaveShipData()
    {
        try
        {
            // Configurar la configuración de serialización para ignorar referencias circulares
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Guardar datos de jugadores.
            string shipJson = JsonConvert.SerializeObject(shipsDictionary, Formatting.Indented, settings);
            File.WriteAllText(shipsDataFilePath, shipJson);

            Debug.Log("Datos de los barcos guardados.");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al guardar datos de los barcos: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Carga los datos de todos los barcos desde un fichero JSON si este existe.
    /// </summary>
    public void LoadShipData()
    {
        if (File.Exists(shipsDataFilePath))
        {
            try
            {
                string shipJson = File.ReadAllText(shipsDataFilePath);
                shipsDictionary = JsonConvert.DeserializeObject<Dictionary<long, Ship>>(shipJson) ?? new Dictionary<long, Ship>();

                foreach (KeyValuePair<long, Ship> entry in shipsDictionary)
                {
                    Ship shipData = entry.Value;
                    shipList.Add(shipData);
                }

                Debug.Log($"Datos de los barcos cargados. Cantidad de barcos: {shipsDictionary.Count}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error al cargar datos de los barcos: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el archivo de datos de los barcos.");
        }
    }

    /// <summary>
    /// Guarda los datos de todos los jugadores en fichero JSON.
    /// </summary>
    /// <returns></returns>
    public Task SavePlayerData()
    {
        try
        {
            // Configurar la configuración de serialización para ignorar referencias circulares
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Guardar datos de jugadores.
            string playersJson = JsonConvert.SerializeObject(playersData, Formatting.Indented, settings);
            File.WriteAllText(playersDataFilePath, playersJson);

            Debug.Log("Datos de jugadores guardados.");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al guardar datos de jugadores: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Carga los datos de los jugadores desde un fichero JSON si este existe.
    /// </summary>
    public void LoadPlayerData()
    {
        // Cargar datos de jugadores
        if (File.Exists(playersDataFilePath))
        {
            try
            {
                string playersJson = File.ReadAllText(playersDataFilePath);
                playersData = JsonConvert.DeserializeObject<Dictionary<long, Player>>(playersJson) ?? new Dictionary<long, Player>();

                foreach (KeyValuePair<long, Player> entry in playersData)
                {
                    Player playerData = entry.Value;
                    playerData.locationIsland = playerData.place != PlayerPlace.Ship ? GetIsland(playerData.locationIsland.id) : null;
                    playerData.locationShip = playerData.place == PlayerPlace.Ship ? GetShip(playerData.locationShip.id) : null;
                    if(playerData.locationShip != null)
                    {
                        if(playerData.locationShip.place != ShipPlace.Port)
                        {
                            GameManager.playersAtSea.Add(playerData);
                        }
                    }
                    for (int i = 0; i < playerData.OwnedBoats.Count; i++)
                    {
                        playerData.OwnedBoats[i] = GetShip(playerData.OwnedBoats[i].id);
                    }
                    if(playerData.missionInPlannin != null)
                    {
                        if(playerData.missionInPlannin.planning)
                        {
                            var city = GameData.Instance.GetIsland(playerData.missionInPlannin.missionLocation.ToString()).city;

                            foreach (var order in city.myOrders)
                            {
                                if(order.missionType == playerData.missionInPlannin.missionType)
                                {
                                    playerData.missionInPlannin = order;
                                }
                            }
                        }
                    }
                    playersDataList.Add(playerData);
                }

                Debug.Log($"Datos de los jugadores cargados. Cantidad de jugadores: {playersData.Count}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error al cargar datos de jugadores: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el archivo de datos de jugadores.");
        }
    }

    /// <summary>
    /// Guarda los datos de las islas en un fichero JSON.
    /// </summary>
    /// <returns></returns>
    public Task SaveIslandData()
    {
        try
        {
            // Configurar la configuración de serialización para ignorar referencias circulares
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };

            // Guardar datos de islas
            string islandsJson = JsonConvert.SerializeObject(islands, Formatting.Indented, settings);
            File.WriteAllText(islandsDataFilePath, islandsJson);

            Debug.Log("Datos de islas guardados.");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al guardar datos de islas: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Carga los datos de las islas desde un fichero JSON si existen, en caso que no existan llama al metodo para generarlas.
    /// </summary>
    public void LoadIslandData()
    {
        // Cargar datos de islas
        if (File.Exists(islandsDataFilePath))
        {
            try
            {
                string islandsJson = File.ReadAllText(islandsDataFilePath);
                // Configurar la configuración de deserialización para manejar tipos derivados
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(islandsJson, settings) ?? new Dictionary<string, Island>();
           
                foreach (KeyValuePair<string, Island> entry in islands)
                {
                    Island island = entry.Value;

                    var destiny = Island.TryGetIslandOrCity(island);

                    if(destiny == Destiny.City)
                    {
                        cityList.Add(island.city);

                        var port = Port.GetPort(island.city.buildings);
                        if(port.Boats != null)
                        {
                            for (int i = 0; i < port.Boats.Count; i++)
                            {
                                port.Boats[i] = GetShip(port.Boats[i].id);
                            }
                        }
                    }
                    
                    islandsList.Add(island);
                }

                Debug.Log("Datos de las islas cargados. Cantidad de islas: " + islands.Count);
            }
            catch(IOException ex)
            {
                Debug.LogError($"Error al cargar datos de islas: {ex.Message}");
            }
            
        }
        else
        {
            Debug.LogWarning("No se encontró el archivo de datos de islas.");
        }
    }

    /// <summary>
    /// Genera aleatoriamente islas en el mundo.
    /// </summary>
    /// <param name="numberOfIslands"></param>
    public void GenerateIslands(int numberOfIslands)
    {
        for (int i = 0; i < numberOfIslands; i++)
        {
            City city = null;
            var random = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
            if (Random.Range(0, 10) == 0) // 10% de probabilidad de que la isla tenga una ciudad
            {
                city = new City("Ciudad " + i, random);
                // Generar edificios para la ciudad
                GenerateBuildingsForCity(city);
            }
            Island newIsland = new Island("Isla " + (i + 1), random, city);
            islandsList.Add(newIsland);
            islands.Add(newIsland.id, newIsland);

            var destiny = Island.TryGetIslandOrCity(newIsland);

            if (destiny == Destiny.City)
            {
                cityList.Add(newIsland.city);
            }
        }

        Debug.Log("Islas generadas.");
        SaveIslandData(); // Guardar los datos después de añadir las islas
    }

    /// <summary>
    /// Genera construcciones para las ciudades.
    /// </summary>
    /// <param name="city"></param>
    private void GenerateBuildingsForCity(City city)
    {
        int numberOfBuildings = Random.Range(1, 2); // Número aleatorio de edificios por ciudad
        
        Port port = new Port(city.position, new List<ShipType>());
        GenerateShip(port);
        city.buildings.Add(port);
        city.buildings.Add(new GrainFarm());
        city.buildings.Add(new Mill());
        city.buildings.Add(new Bakery());

    }

    /// <summary>
    /// Agraga tipos de barcos vendibles a un puerto.
    /// </summary>
    /// <param name="port"></param>
    public void GenerateShip(Port port)
    {   
        port.BoatsForSale.Add(ShipType.Sloop);
        port.BoatsForSale.Add(ShipType.Brigantine);
        port.BoatsForSale.Add(ShipType.Frigate);
        port.BoatsForSale.Add(ShipType.Galleon);
    }

    /// <summary>
    /// Retorna un diccionario con todas las islas.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Island> GetIslands()
    {
        return islands;
    }

    /// <summary>
    /// Retorna una lista con todas las islas.
    /// </summary>
    /// <returns></returns>
    public List<Island> GetIslandsList()
    {
        return islandsList;
    }

    /// <summary>
    /// Agraga un nuevo jugador al mundo.
    /// </summary>
    /// <param name="player"></param>
    public void AddPlayer(Player player)
    {
        playersData.Add(player.playerID, player);
        playersDataList.Add(player);
        SavePlayerData(); // Guardar los datos después de añadir un jugador
    }

    /// <summary>
    /// Comprueba si el jugador existe.
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public bool PlayerExists(long userID)
    {
        return playersData.ContainsKey(userID);
    }

    /// <summary>
    /// Retorna un diccionario con todos los jugadores.
    /// </summary>
    /// <returns></returns>
    public Dictionary<long, Player> GetPlayers()
    {
        return playersData;
    }

    /// <summary>
    /// Retorna un jugador segun si ID.
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public Player GetPlayerData(long playerID)
    {
        if (playersData.TryGetValue(playerID, out Player playerData))
        {
            return playerData;
        }
        else
        {
            Debug.LogWarning($"Player with ID {playerID} not found.");
            return null;
        }
    }

    /// <summary>
    /// Retorna una isla por su ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Island GetIsland(string id)
    {
        if (islands.TryGetValue(id, out Island island))
        {
            return island;
        }
        else
        {
            Debug.LogWarning($"Island with ID {id} not found.");
            return null;
        }
    }

    /// <summary>
    /// Retorna el barco con el ID pasado por parametro.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Ship GetShip(long id)
    {
        if (shipsDictionary.TryGetValue(id, out Ship ship))
        {
            return ship;
        }
        else
        {
            Debug.LogWarning($"Ship with ID {id} not found.");
            return null;
        }
    }

    /// <summary>
    /// Agreaga un barco a la lista global de barcos.
    /// </summary>
    /// <param name="ship"></param>
    public void AddShip(Ship ship)
    {
        shipsDictionary.Add(ship.id, ship);
        shipList.Add(ship);
    }

    /// <summary>
    /// Retorna todas las ciudades.
    /// </summary>
    /// <returns></returns>
    public List<City> GetCities()
    {
        return cityList;
    }
}
