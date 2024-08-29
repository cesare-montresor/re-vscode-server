using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using ScriptManagerServer;

    class Program
    {
        private static List<Script> scripts = new List<Script>();
        private static TcpListener listener;
        public static int defaultPort = 8765;
        

        static void Main(string[] args)
        {
            InitializeScripts();
            StartServer();
        }

        static void InitializeScripts()
        {
            // Add some dummy scripts
            scripts.Add(new Script { 
                Path = "C:\\script1.py", 
                Content = "print('Hello')", 
                Status = Script.Types.Status.Stopped 
            });
            scripts.Add(new Script { 
                Path = "C:\\script2.py", 
                Content = "print('World')", 
                Status = Script.Types.Status.Stopped 
            });
        }

        static void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 8765);
            listener.Start();
            Console.WriteLine("Server started on port 8765");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(Program.HandleClient));
                clientThread.Start(client);
            }
        }

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            Console.Out.WriteLine("Program:HandleClient");

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0) break;

                // Process the received message
                ProcessMessage(buffer, bytesRead, stream);
            }

            client.Close();
        }

        static void ProcessMessage(byte[] buffer, int bytesRead, NetworkStream stream)
        {
            try
            {
                ScriptAction action = ScriptAction.Parser.ParseFrom(buffer, 0, bytesRead);
                HandleScriptAction(action);
            }
            catch (InvalidProtocolBufferException)
            {
                // If it's not a ScriptAction, assume it's a request for the script list
                SendScriptList(stream);
            }
        }

        static void HandleScriptAction(ScriptAction action)
        {
            var script = scripts.Find(s => s.Path == action.Path);
            if (script != null)
            {
                switch (action.Action)
                {
                    case ScriptAction.Types.Action.Play:
                        script.Status = Script.Types.Status.Running;
                        Console.WriteLine($"Playing script: {script.Path}");
                        break;
                    case ScriptAction.Types.Action.Stop:
                        script.Status = Script.Types.Status.Stopped;
                        Console.WriteLine($"Stopping script: {script.Path}");
                        break;
                    case ScriptAction.Types.Action.Pause:
                        script.Status = Script.Types.Status.Paused;
                        Console.WriteLine($"Pausing script: {script.Path}");
                        break;
                    case ScriptAction.Types.Action.Resume:
                        script.Status = Script.Types.Status.Running;
                        Console.WriteLine($"Resuming script: {script.Path}");
                        break;
                }
            }
        }

        static void SendScriptList(NetworkStream stream)
        {
            ScriptList scriptList = new ScriptList();
            scriptList.Scripts.AddRange(scripts);

            byte[] response = scriptList.ToByteArray();
            stream.Write(response, 0, response.Length);
        }
    }
