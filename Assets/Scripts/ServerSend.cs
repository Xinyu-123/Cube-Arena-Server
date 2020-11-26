using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
                Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
                Server.clients[i].udp.SendData(_packet);
        }
    }

    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }

    }

    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerShoot(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerShoot))
        {
            _packet.Write(_player.id);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerAnimation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerAnimation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.animator.GetBool("isRunning"));

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ServerTime(int _toClient, Packet _clientPacket)
    {
        using (Packet _packet = new Packet((int)ServerPackets.serverTime))
        {
            _packet.Write(_clientPacket.ReadLong());
            _packet.Write((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);

            SendUDPData(_toClient, _packet);
        }
            
    }

    public static void SyncClock()
    {

        int _timeLeft = (Server.endTime - DateTime.UtcNow).Seconds;

        if (_timeLeft <= 0 && Server.clockStarted)
        {
            Server.EndMatch();
        }

        int _nextGameTimeLeft = (Server.nextGameTime - DateTime.UtcNow).Seconds;

        if (_nextGameTimeLeft <= 0 && Server.nextGameClockStarted)
        {
            if (Server.CurrentPlayers >= 2)
            {
                Debug.Log("SYNC TIME BEGIN MATCH");
                Server.BeginMatch();
            }
            else
            {
                WaitingForPlayers();
            }

        }

        using (Packet _packet = new Packet((int)ServerPackets.syncClock))
        {
            _packet.Write((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);

            SendUDPDataToAll(_packet);
        }

        

    }

    /// <summary>Sends a Packet containing the endTime for the current match to all players
    /// </summary>
    public static void GameTime()
    {
        
        using (Packet _packet = new Packet((int)ServerPackets.gameTime))
        {
            _packet.Write((long)(Server.endTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);

            SendTCPDataToAll(_packet);
        }
    }

    public static void EndGame()
    {
        Debug.Log("GAME OVER");
        

        using (Packet _packet = new Packet((int)ServerPackets.endGame))
        {
            // Calculate the score of all the players and send it to all players
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(Server.clients[i].player != null)
                {

                    _packet.Write(Server.clients[i].player.id);
                    _packet.Write(Server.clients[i].player.score);
                }   
            }

            SendTCPDataToAll(_packet);

        }

        
        
    }

    public static void WaitingForPlayers()
    {
        using (Packet _packet = new Packet((int)ServerPackets.waitingForPlayers))
        {
            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHit(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHit))
        {
            SendTCPData(_player.id, _packet);
        }
    }

    public static void PlayerRespawnRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawnRotation))
        {
            _packet.Write(_player.transform.rotation);

            SendTCPData(_player.id, _packet);
        }
    }
}
