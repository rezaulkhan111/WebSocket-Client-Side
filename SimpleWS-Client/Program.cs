using System.Net.WebSockets;
using System.Text;

var ws = new ClientWebSocket();

Console.WriteLine("Connecting to server");
try
{
    await ws.ConnectAsync(new Uri("ws://192.168.0.110:8085/sendMessage?name=ConsoleApp"), CancellationToken.None);
}
catch (Exception aaa)
{
    Console.WriteLine(aaa.Message);
}

Console.WriteLine("");

var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024 * 4];
    while (true)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine(message);
    }
});

var sendTask = Task.Run(async () =>
{
    Console.WriteLine("Enter terget name:");
    string reciverName = Console.ReadLine();
    Console.WriteLine($"Terget name: {reciverName}");
    string reciverMessage;
    while (true)
    {
        Console.Write("Message: ");
        reciverMessage = Console.ReadLine();

        string messageToSend = reciverName + ":" + reciverMessage;
        if (reciverMessage == "exit")
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);

            break;
        }
        var bytes = Encoding.UTF8.GetBytes(messageToSend);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
});

await Task.WhenAny(sendTask, receiveTask);

try
{
    if (ws.State != WebSocketState.Closed)
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
}
catch (Exception aaa)
{
    Console.WriteLine(aaa.Message);
}

await Task.WhenAll(sendTask, receiveTask);