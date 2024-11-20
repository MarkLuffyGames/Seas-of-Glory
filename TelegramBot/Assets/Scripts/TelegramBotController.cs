using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UnityEngine;

public class TelegramBotController : MonoBehaviour
{
    public static TelegramBotController Instance;

    public event Action<Message> OnStart;
    public event Action<Message> OnDashboard;
    public event Action<Message> OnFacilities;
    public event Action<Message> OnBuyShipButton;
    public event Action<Message> OnSellShipButton;
    public event Action<Message> OnJobsButton;
    public event Action<Message> OnMyShipButton;
    public event Action<Message> OnBoardButton;
    public event Action<Message> OnCourseButton;
    public event Action<Message> OnExit;
    public event Action<Message> OnSetCourse;
    public event Action<Message> OnWeighAnchor;
    public event Action<Message> OnLowerAnchor;
    public event Action<Message> OnBack;
    public event Action<Message> OnMissionsButton;
    public event Action<Message> OnAddAmount;
    public event Action<Message> OnAccept;
    public event Action<Message> OnCancel;
    public event Action<Message> OnDisembark;

    public event Action<CallbackQuery> OnGoTo;
    public event Action<CallbackQuery> OnBuyShip;
    public event Action<CallbackQuery> OnBoardMyShip;
    public event Action<CallbackQuery> OnDisembarkCity;
    public event Action<CallbackQuery> OnGetJob;

    private int lastUpdateId = 0;
    public TelegramBotClient botClient;
    private CancellationTokenSource cts;

    public string botToken;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartBot()
    {
        botClient = new TelegramBotClient(botToken);
        cts = new CancellationTokenSource();
        Debug.Log("Bot de Telegram iniciado.");
        StartPolling();
    }

    public async Task StopBot()
    {
        // Cancelar el token de cancelación para detener la obtención de actualizaciones
        cts.Cancel();

        // Esperar a que todas las tareas pendientes se completen
        await Task.WhenAll(
        // Otras tareas que puedas tener en ejecución
        );

        

        Debug.Log("Bot de Telegram detenido correctamente.");
    }

    

    private async void StartPolling()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            var updates = await botClient.GetUpdatesAsync(lastUpdateId);

            if (updates.Any())
            {
                foreach (var update in updates)
                {
                    await ProcessUpdate(update);
                }
                lastUpdateId = updates.Last().Id + 1;
            }

            await Task.Delay(1000); // Espera 1 segundos antes de la siguiente solicitud
            /*try
            {
                var updates = await botClient.GetUpdatesAsync(lastUpdateId);

                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        await ProcessUpdate(update);
                    }
                    lastUpdateId = updates.Last().Id + 1;
                }

                await Task.Delay(1000); // Espera 1 segundos antes de la siguiente solicitud
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("Too Many Requests"))
            {
                int retryAfter = ex.Parameters?.RetryAfter ?? 5; // Usa el tiempo recomendado por Telegram o 5 segundos por defecto
                Debug.LogError($"Too Many Requests: retry after {retryAfter} seconds");
                await Task.Delay(retryAfter * 1000); // Espera el tiempo recomendado
            }
            catch (TaskCanceledException)
            {
                // La tarea fue cancelada, probablemente por el token de cancelación
                Debug.LogWarning("Polling canceled.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al obtener actualizaciones: {ex.Message}");
                await Task.Delay(1000); // Espera 1 segundo antes de intentar nuevamente en caso de otro error
            }*/
        }

        // Limpia recursos si es necesario
        cts.Dispose();
        Debug.Log("Dispose");
    }

    private async Task ProcessUpdate(Telegram.Bot.Types.Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackQuery(update.CallbackQuery);
        }
        else if (update.Type == UpdateType.Message)
        {
            if (update.Message != null && update.Message.Text != null)
            {
                var message = update.Message;
                var chatId = message.Chat.Id;

                Debug.Log($"Mensaje recibido: {message.Text}");

                if (message.Text == "/start")
                {
                    StartBot(message);
                }
                else if (message.Text == "/help")
                {
                    Help(message);
                }
                else if (message.Text.StartsWith("/c"))
                {
                    OnSetCourse?.Invoke(message);
                }
                else if (message.Text.StartsWith("/add"))
                {
                    OnAddAmount?.Invoke(message);
                }
                else
                {
                    
                    MethodInfo method = GetType().GetMethod(message.Text.Replace(" ", ""), BindingFlags.NonPublic | BindingFlags.Instance);

                    if (method != null)
                    {
                        method.Invoke(this, new object[] { message });
                    }
                    else
                    {
                        SendUnknownCommandMessage(chatId);
                    }
                }
            }
        }
        
    }

    private async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {

            // Eliminar el teclado inline
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: null);


            var callbackData = callbackQuery.Data;
            var chatId = callbackQuery.Message.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;

            if (callbackQuery.Data.StartsWith("MyBoat"))
            {
                OnBoardMyShip?.Invoke(callbackQuery);
            }
            else if (callbackQuery.Data.StartsWith("BuyBoat"))
            {
                OnBuyShip?.Invoke(callbackQuery);
            }
            else if (callbackQuery.Data.StartsWith("DisembarkCity"))
            {
                OnDisembarkCity?.Invoke(callbackQuery);
            }
            else if (callbackQuery.Data.StartsWith("GoTo"))
            {
                OnGoTo?.Invoke(callbackQuery);
            }
            else if (callbackQuery.Data.StartsWith("GetJob"))
            {
                OnGetJob?.Invoke(callbackQuery);
            }
            else
            {
                // Procesa la información de callbackData
                switch (callbackData)
                {
                    case "port":
                        break;
                    default:
                        await botClient.SendTextMessageAsync(chatId, $"Botón pulsado: {callbackData}");
                        break;
                }
            }
           
            // Opcional: Responder al callback query
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al manejar CallbackQuery: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void SendUnknownCommandMessage(long chatId)
    {
        botClient.SendTextMessageAsync(chatId, "Comando no reconocido. Usa /help para ver la lista de comandos.");
    }

    public async void SendMessageAsyncReplyKeyboardMarkup(long chatID, string message, ReplyKeyboardMarkup keyboard)
    {
        try
        {
            await botClient.SendTextMessageAsync(chatID, message, replyMarkup: keyboard);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al enviar mensaje: {ex.Message}");
        }
    }

    public async void SendMessageAsyncInlineKeyboardMarkup(long chatID, string message, InlineKeyboardMarkup keyboard)
    {
        try
        {
            await botClient.SendTextMessageAsync(chatID, message, replyMarkup: keyboard);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al enviar mensaje: {ex.Message}");
        }
    }

    private void StartBot(Message message)
    {
        OnStart?.Invoke(message);
    }

    private void Help(Message message)
    {
        
        var chatId = message.Chat.Id;
        botClient.SendTextMessageAsync(chatId, "Lista de comandos disponibles:\n" +
                                 "/start - Inicia el juego\n" +
                                 "/help - Muestra los comandos");
    }

    private void Dashboard(Message message)
    {
        OnDashboard?.Invoke(message);
    }

    private void Facilities(Message message)
    {
        OnFacilities?.Invoke(message);
    }

    private void MyShips(Message message)
    {
        OnMyShipButton?.Invoke(message);
    }

    private void BuyShip(Message message)
    {
        OnBuyShipButton?.Invoke(message);
    }

    private void SellShip(Message message)
    {
        OnSellShipButton?.Invoke(message);
    }

    private void Jobs(Message message)
    {
        OnJobsButton?.Invoke(message);
    }
    private void Exit(Message message)
    {
        OnExit?.Invoke(message);
    }
    private void Board(Message message)
    {
        OnBoardButton?.Invoke(message);
    }

    private void Course(Message message)
    {
        OnCourseButton?.Invoke(message);
    }

    private void WeighAnchor(Message message)
    {
        OnWeighAnchor?.Invoke(message);
    }

    private void LowerAnchor(Message message)
    {
        OnLowerAnchor?.Invoke(message);
    }

    private void Back(Message message)
    {
        OnBack?.Invoke(message);
    }
    private void Missions(Message message)
    {
        OnMissionsButton?.Invoke(message);
    }

    private void Accept(Message message)
    {
        OnAccept?.Invoke(message);
    }

    private void Cancel(Message message)
    {
        OnCancel?.Invoke(message);
    }

    private void Disembark(Message message)
    {
        OnDisembark?.Invoke(message);
    }
}
