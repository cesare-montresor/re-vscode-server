using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System.Collections.Concurrent;

// Data
public class Script
{
    public string? Path { get; set; }
    public string? Filename { get; set; }
    public string? Content { get; set; }
    public string? Status { get; set; }
    public string? Language { get; set; }
    public bool? Temporary { get; set; }

    public Script(){}
    /*Script(string Path, string? Filename, string? Content, string? Status, string? Language, bool? Temporary){
        this.Path = Path;
        this.Filename = Filename;
        this.Status = Status;
        this.Language = Language;
        this.Temporary = Temporary;
    }*/
}



// Message
public class Message{
    public readonly string messageType;
    public Message(){
        messageType = GetType().ToString();
    }
}

public class ScriptList: Message
{
    public List<Script> scriptList = new List<Script>();
    public ScriptList():base(){}
}

public class ScriptAction: Message
{
    public string Path { get; set; } = "";
    public string Action { get; set; } = "";
    public ScriptAction():base(){}
    
}



public class REVSCodeServer : WebSocketBehavior
{
    public static bool debug = true;
    public static int defaultPort = 8765;
    public int serverPort = defaultPort;

    public static readonly Timer update_timer = new Timer( PerformUpdate );


    private static ConcurrentDictionary<string, Script> scripts = new ConcurrentDictionary<string, Script>();
    private static ConcurrentDictionary<string, WebSocket> clients = new ConcurrentDictionary<string, WebSocket>();
    

    public static WebSocketServer getServer(){
        //var wssv = new WebSocketServer(8765);
        var wssv = new WebSocketServer("ws://localhost:8765");
        wssv.AddWebSocketService<REVSCodeServer>("/");
        wssv.Start();
        Console.WriteLine("WebSocket server started on ws://localhost:8765");

        return wssv;
    }


    protected static T? Deserialize<T>(string data){
        return JsonConvert.DeserializeObject<T>(data);
    }

    protected static Dictionary<string,dynamic>? Deserialize(string data){
        var obj = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(data);
        return obj;
    }

    protected static string Serialize(object obj){
        return JsonConvert.SerializeObject(obj);
    }

    public static void StartUpdate(){
        update_timer.Change(0,1000);
    }

    protected static void PerformUpdate(object? sender){
        BroadcastScriptList();
    }

    public static void StopUpdate(){
        update_timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private static void BroadcastScriptList()
    {
        var msg = new ScriptList{scriptList=scripts.Values.ToList()};
        var payload = Serialize(msg);
        Console.WriteLine("PerformUpdate:",msg.ToString());
        clients.Values.ToList().ForEach(delegate(WebSocket client){client.Send(payload);});
    }




    public REVSCodeServer(){
        // Initialize with some sample scripts if the dictionary is empty
        if (scripts.IsEmpty)
        {
            scripts.TryAdd("/scripts/script1.py", new Script { Path = "/scripts/script1.py", Filename = "script1.py", Content = "print('Hello from script1')", Status = "stop" });
            scripts.TryAdd("/scripts/script2.cs", new Script { Path = "/scripts/script2.cs", Filename = "script2.cs", Content = "Console.WriteLine(\"Hello from script2\");", Status = "stop" });
            scripts.TryAdd("/scripts/script3.uos", new Script { Path = "/scripts/script3.uos", Filename = "script3.uos", Content = "msg 'hello'", Status = "stop" });
        }
    }

    protected override void OnOpen()
    {
        clients.TryAdd(ID, Context.WebSocket);
        BroadcastScriptList();
    }

    protected override void OnClose(CloseEventArgs e)
    {
        clients.TryRemove(ID, out _);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            var payload = e.Data;
            var message = Deserialize(payload);
            if (message == null) {return;}
            var messageType = message["messageType"] as string;
            

            switch (messageType)
            {
                case nameof(ScriptAction):
                    HandleScriptAction(payload);
                    break;

                case nameof(ScriptList):
                    HandleScriptList(payload);
                    break;

                default:
                    Console.WriteLine($"Unknown message type: {messageType} for {message}");
                    break;
            }

            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }


    private void HandleScriptAction(string rawData)
    {
        var scriptAction = Deserialize<ScriptAction>(rawData);
        if (scriptAction != null)
        {
            UpdateScriptStatus(scriptAction.Path, scriptAction.Action);
            BroadcastScriptList();
        }
    }

    private void HandleScriptList(string rawData)
    {
        var scriptList = Deserialize<ScriptList>(rawData);
        if (scriptList != null)
        {
            Console.WriteLine($"Received ScriptList with {scriptList.scriptList.Count} scripts");
            // Add any processing for ScriptList messages here
        }
    }

    private void UpdateScriptStatus(string path, string action)
    {
        if (scripts.TryGetValue(path, out var script))
        {
            script.Status = action == "resume" ? "run" : action;
            Console.WriteLine($"UpdateScriptStatus: {script.Status}: {script.Path}");
            scripts[path] = script; // Update the script in the dictionary
        }
    }

    
    public static void AddOrUpdateScript(Script script)
    {
        if (script.Path==null) {return;}
        scripts[script.Path] = script;
        BroadcastScriptList();
    }

    public static void RemoveScript(string path)
    {
        if (scripts.TryRemove(path, out _))
        {
            BroadcastScriptList();
        }
    }

    
}






public class Program
{
    public static void Main(string[] args)
    {
        var server = REVSCodeServer.getServer();

        // Example of adding a new script from the server side
        Console.WriteLine("Commands hotkeys: ");
        Console.WriteLine(" A: Add a new script");
        Console.WriteLine(" R: to Remove a script");
        Console.WriteLine(" Q: Quit.");
        while (true){
            var key = Console.ReadKey(true).Key;
            
            if (key == ConsoleKey.A)
            {
                var newScript = new Script
                {
                    Path = $"/scripts/script{DateTime.Now.Ticks}",
                    Filename = $"script{DateTime.Now.Ticks}.py",
                    Content = "print('New script')",
                    Status = "stop"
                };
                REVSCodeServer.AddOrUpdateScript(newScript);
                Console.WriteLine($"Added new script: {newScript.Path}");
            }
            else if (key == ConsoleKey.R)
            {
                Console.Write("Enter the path of the script to remove: ");
                var pathToRemove = Console.ReadLine();
                REVSCodeServer.RemoveScript(pathToRemove);
                Console.WriteLine($"Removed script: {pathToRemove}");
            }
            else if (key == ConsoleKey.Q)
            {
                break;
            }
            
        }

        server.Stop();
    }
}