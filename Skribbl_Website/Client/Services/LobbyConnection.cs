﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Skribbl_Website.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace Skribbl_Website.Client.Services
{
    public class LobbyConnection
    {
        public Player User { get; set; }
        public LobbyClient Lobby { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();

        private HubConnection _hubConnection;

        private void InvokeOnReceive()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler StateChanged;

        protected virtual void OnError()
        {
            ErrorOccured?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ErrorOccured;

        protected virtual void OnBan(string username)
        {
            UserBanned?.Invoke(this, username);
        }
        public event EventHandler<string> UserBanned;

        public LobbyConnection()
        {

        }

        public async Task StartConnection(Player user, Uri hubUrl, string lobbyId)
        {
            _hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            }).WithAutomaticReconnect().Build();
            User = user;

            _hubConnection.On<Message>("ReceiveNewHost", (message) =>
            {
                Messages.Add(new Message(message.MessageContent, message.Type));
                Lobby.SetHostPlayer(message.Sender);
                InvokeOnReceive();
            });

            _hubConnection.On<Message>("ReceiveMessage", (message) =>
            {
                Messages.Add(message);
                InvokeOnReceive();
            });

            _hubConnection.On<LobbyClientDto>("SetLobby", (lobbyClientDto) =>
            {
                Lobby = new LobbyClient(lobbyClientDto);
                InvokeOnReceive();
            });

            _hubConnection.On<PlayerClient>("AddPlayer", (playerToAdd) =>
            {
                if (!Lobby.Players.Any(p => p.Name == playerToAdd.Name))
                {
                    Lobby.Players.Add(playerToAdd);
                }
                InvokeOnReceive();
            });

            _hubConnection.On<Message>("RemovePlayer", (message) =>
            {
                Lobby.RemovePlayerByName(message.Sender);
                Messages.Add(message);
                InvokeOnReceive();
            });

            _hubConnection.On<LobbySettings>("ReceiveLobbySettings", (lobbySettings) =>
            {
                Console.WriteLine("in receive");
                Lobby.LobbySettings = lobbySettings;
                InvokeOnReceive();
            });

            _hubConnection.On<string>("RedirectToKickedPage", (hostName) =>
            {
                OnBan(hostName);
            });

            _hubConnection.On<int>("StartGame", (delay) =>
            {
                Lobby.StartGame();
                InvokeOnReceive();
            });

            _hubConnection.On<Message>("ReceiveNewDrawingPlayer", (message) =>
            {
                Messages.Add(message);
                Lobby.SetDrawingPlayer(message.Sender);
                InvokeOnReceive();
            });

            await _hubConnection.StartAsync();
            try
            {
                await Join(lobbyId);
            }
            catch
            {
                OnError();
            }
        }
        public void CloseConnection()
        {
            _hubConnection.StopAsync();
            //TODO: Clear UserState
        }

        public Task Send(string messageContent) =>
        _hubConnection.SendAsync("SendMessage", new Message(messageContent, Message.MessageType.Guess, User.Name));

        private Task Join(string lobbyId) =>
        _hubConnection.InvokeAsync("AddToGroup", User.Id, lobbyId);

        public Task UpdateLobbySettings()
        {
            if (UserIsHost)
            {
                return _hubConnection.SendAsync("SendLobbySettings", Lobby.LobbySettings);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task BanPlayer(string playerName)
        {
            if (UserIsHost)
            {
                return _hubConnection.SendAsync("BanPlayer", playerName);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task StartGame()
        {
            Console.WriteLine("button clicked");
            if (UserIsHost && Lobby.GetConnectedPlayersCount() >= Lobby.MinPlayers && Lobby.State == LobbyState.Preparing)
            {
                Console.WriteLine("message sent");
                return _hubConnection.SendAsync("StartGame");
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public bool UserIsHost
        {
            get
            {
                return User?.Name == Lobby.GetHostPlayer()?.Name;
            }
        }

        public bool UserIsDrawing
        {
            get
            {
                return User?.Name == Lobby.GetDrawingPlayer()?.Name;
            }
        }
    }
}
