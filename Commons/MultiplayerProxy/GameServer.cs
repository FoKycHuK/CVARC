﻿using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Infrastructure;
using log4net;
using ProxyCommon;

namespace MultiplayerProxy
{
    public static class GameServer
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(GameServer));

        public static async Task StartGame(ClientWithSettings[] clientsWithSettings, string levelName)
        {
            log.Debug("StartGame call");

            var settings = CreateGameSettings(clientsWithSettings, levelName);
            var mainConnection = ConnnectToServer();
            mainConnection.WriteJson(settings);

            foreach (var client in clientsWithSettings.Select(c => c.Client))
                CreateConnectionBetweenPlayerAndServer(client);

            await GetResultAndSendToWeb(mainConnection);
        }

        private static GameSettings CreateGameSettings(ClientWithSettings[] settings, string levelName)
        {
            var controllerList = MultiplayerProxyConfigurations.LevelToControllerIds[levelName];

            return new GameSettings
            {
                LevelName = levelName,
                ActorSettings = controllerList.Select((t, i) => new ActorSettings
                {
                    ControllerId = t,
                    PlayerSettings = settings[i].Settings
                }).ToArray()
            };
        }

        private static TcpClient ConnnectToServer()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(MultiplayerProxyConfigurations.UnityEndPoint);
            return tcpClient;
        }

        private static void CreateConnectionBetweenPlayerAndServer(TcpClient client)
        {
            var server = ConnnectToServer();
            Proxy.CreateChainAndStart(client, server);
        }

        private static async Task GetResultAndSendToWeb(TcpClient mainConnection)
        {
            var result = await mainConnection.ReadJsonAsync<GameResult>();
            WebServer.SendResult(result);
            Pool.CheckGame();
        }
    }
}