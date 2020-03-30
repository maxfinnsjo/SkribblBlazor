﻿using Skribbl_Website.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace LobbyTests
{
    public class PlayerDtoTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(100)]
        public void AddScore_Correct(int scoreToAdd)
        {
            var player = new PlayerDto("player");

            player.AddScore(scoreToAdd);

            Assert.Equal(scoreToAdd, player.Score);
        }

        [Fact]
        public void AddScore_NegativeValue_ThrowError()
        {
            var player = new PlayerDto("player");

            Assert.Throws<ArgumentOutOfRangeException>(() => player.AddScore(-1));       
        }

        [Fact]
        public void SetIsHost_ConnectedPlayer()
        {
            var player = new PlayerDto("player");
            player.IsConnected = true;

            player.IsHost = true;

            Assert.True(player.IsHost);
        }

        [Fact]
        public void SetIsHost_NotConnectedPlayer_ThrowsError()
        {
            var player = new PlayerDto("player");
            player.IsConnected = false;
            Assert.Throws<Exception>(() => player.IsHost = true);
        }

        [Fact]
        public void SetIsDrawing_NotConnectedPlayer_ThrowsError()
        {
            var player = new PlayerDto("player");
            player.IsConnected = false;
            Assert.Throws<Exception>(() => player.IsDrawing = true);
        }
    }
}