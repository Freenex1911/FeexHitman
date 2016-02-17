﻿using fr34kyn01535.Uconomy;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;
using System;

namespace Freenex.FeexHitman
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new I18N.West.CP1250();
            CreateCheckSchema();
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (Uconomy.Instance.Configuration.Instance.DatabasePort == 0) Uconomy.Instance.Configuration.Instance.DatabasePort = 3306;
                connection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", Uconomy.Instance.Configuration.Instance.DatabaseAddress, Uconomy.Instance.Configuration.Instance.DatabaseName, Uconomy.Instance.Configuration.Instance.DatabaseUsername, Uconomy.Instance.Configuration.Instance.DatabasePassword, Uconomy.Instance.Configuration.Instance.DatabasePort));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }

        public bool CheckExists(CSteamID id)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                int exists = 0;
                command.CommandText = "SELECT COUNT(1) FROM `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId` = '" + id.ToString() + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) Int32.TryParse(result.ToString(), out exists);
                connection.Close();

                if (exists == 0) { return false; }
                else { return true; }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        public decimal GetBounty(CSteamID id)
        {
            decimal output = 0;
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT `bounty` FROM `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId` = '" + id.ToString() + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) Decimal.TryParse(result.ToString(), out output);
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output;
        }

        public string GetBountyList()
        {
            System.Text.StringBuilder HitmanList = new System.Text.StringBuilder();
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM (SELECT * FROM `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `lastUpdated` ORDER BY `lastUpdated` DESC LIMIT " + FeexHitman.Instance.Configuration.Instance.CommandListMaximum + ") AS tbl ORDER BY `lastUpdated` ASC";
                connection.Open();
                MySqlDataReader Reader = command.ExecuteReader();

                bool firstPlayer = true;
                while (Reader.Read())
                {
                    if (firstPlayer) { firstPlayer = false; }
                    else { HitmanList.Append(", "); }

                    HitmanList.Append(Reader.GetString(2));

                    if (GetBountyCount() > FeexHitman.Instance.Configuration.Instance.CommandListMaximum) { HitmanList.Append(", [...]"); }
                }
                
                Reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return HitmanList.ToString();
        }

        public int GetBountyCount()
        {
            int output = 0;
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "`";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) int.TryParse(result.ToString(), out output);
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output;
        }

        public void AddUpdateVictimAccount(CSteamID id, decimal bounty, string lastDisplayName)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                if (CheckExists(id))
                {
                    command.CommandText = "UPDATE `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` SET `bounty` = bounty + (" + bounty + "), `lastDisplayName` = @lastDisplayName, `lastUpdated` = NOW() WHERE `steamId` = '" + id.ToString() + "'";
                }
                else
                {
                    command.CommandText = "INSERT IGNORE INTO `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` (steamId,bounty,lastDisplayName,lastUpdated) VALUES('" + id.ToString() + "','" + bounty + "',@lastDisplayName,NOW())";
                }
                command.Parameters.AddWithValue("@lastDisplayName", lastDisplayName);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public bool RemoveVictimAccount(CSteamID id)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId`='" + id.ToString() + "'";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        public void UpdateVictimDisplayName(CSteamID id, string lastDisplayName)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` SET `lastDisplayName` = @lastDisplayName WHERE `steamId` = '" + id.ToString() + "'";
                command.Parameters.AddWithValue("@lastDisplayName", lastDisplayName);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        internal void CreateCheckSchema()
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SHOW TABLES LIKE '" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "'";
                connection.Open();
                object test = command.ExecuteScalar();

                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + FeexHitman.Instance.Configuration.Instance.DatabaseTableName + "` (`steamId` varchar(32) NOT NULL, `bounty` decimal(15,2) NOT NULL DEFAULT '25.00', `lastDisplayName` varchar(32) NOT NULL, `lastUpdated` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',PRIMARY KEY (`steamId`))";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
