using Aml.Engine.Resources.Catalogue;
using MAPol.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol
{
    internal class DBHandler
    {
        string connectionString = "Data Source=MAPOC.sqlite;Version=3;";

        public DBHandler()
        {           
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = "CREATE TABLE IF NOT EXISTS MTP (Id INTEGER PRIMARY KEY, Name TEXT)";              
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ModuleDisplay (
                    Id INTEGER PRIMARY KEY,
                    MTPId INTEGER,
                    Name TEXT,
                    FOREIGN KEY(MTPId) REFERENCES MTP(Id)
                );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Services (
                    Id INTEGER PRIMARY KEY,
                    MTPId INTEGER,
                    Name TEXT,
                    FOREIGN KEY(MTPId) REFERENCES MTP(Id)
                );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                createTableQuery = @"
                CREATE TABLE IF NOT EXISTS OpcUaItems (
                    Id TEXT PRIMARY KEY,
                    Access INTEGER,
                    ServerEndpoint TEXT,
                    Name TEXT,
                    Identifier TEXT,
                    OpcUaNamespace TEXT,
                    DefaultValue TEXT,
                    Value TEXT
                );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }


                connection.Close();
            }
        }

        public bool InsertMTPData(string Name)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO MTP (Name) VALUES (@Name)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", Name);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            return true;
        }

        public List<OpcUaItem> ReadOpcUaItems(List<OpcUaItem> opcUaItems)
        {
            List<OpcUaItem> opcUaItemValues = new List<OpcUaItem>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * from OpcUaItems";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader.GetString(0);
                            string value = reader.GetString(6);
                            OpcUaItem item = opcUaItems.Find(x => x.Id == id);
                            item.Value = value;
                            opcUaItemValues.Add(item);
                        }
                    }
                }
                connection.Close();
            }
            return opcUaItemValues;
        }

        public List<OpcUaItem> ReadAllOpcUaItems()
        {
            List<OpcUaItem> opcUaItemValues = new List<OpcUaItem>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * from OpcUaItems";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OpcUaItem item = new OpcUaItem();
                            item.Id = reader.GetString(0);
                            item.Access = reader.GetInt16(1);
                            item.ServerEndPoint = reader.GetString(2);
                            item.Name = reader.GetString(3);
                            //item.Identifier = reader.GetString(4);
                            //item.OpcUaNamespace = reader.GetString(5);
                            if(reader.GetString(7) == null)
                            {
                                item.Value = string.Empty;
                            }
                            else
                            {
                                item.Value = reader.GetString(7);
                            }

                            opcUaItemValues.Add(item);
                        }
                    }
                }
                connection.Close();
            }
            return opcUaItemValues;
        }

        public void InsertOpcUaItems(List<OpcUaItem> opcUaItems)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO OpcUaItems (Id, Access, ServerEndpoint, Name, Identifier, OpcUaNamespace, Value) VALUES ($Id, $Access, $ServerEndpoint, $Name, $Identifier, $OpcUaNamespace, $ValueParam)";

                    var IdParam = command.CreateParameter();
                    IdParam.ParameterName = "$Id";
                    command.Parameters.Add(IdParam);

                    var accessParam = command.CreateParameter();
                    accessParam.ParameterName = "$Access";
                    command.Parameters.Add(accessParam);

                    var serverEndpointParam = command.CreateParameter();
                    serverEndpointParam.ParameterName = "$ServerEndpoint";
                    command.Parameters.Add(serverEndpointParam);

                    var nameParam = command.CreateParameter();
                    nameParam.ParameterName = "$Name";
                    command.Parameters.Add(nameParam);

                    var identifierParam = command.CreateParameter();
                    identifierParam.ParameterName = "$Identifier";
                    command.Parameters.Add(identifierParam);

                    var opcNamespaceParam = command.CreateParameter();
                    opcNamespaceParam.ParameterName = "$OpcUaNamespace";
                    command.Parameters.Add(opcNamespaceParam);

                    var valueParam = command.CreateParameter();
                    valueParam.ParameterName = "$ValueParam";
                    command.Parameters.Add(valueParam);

                    foreach (var item in opcUaItems)
                    {
                        IdParam.Value = item.Id;
                        accessParam.Value = item.Access;
                        serverEndpointParam.Value = item.ServerEndPoint;
                        nameParam.Value= item.Name;
                        identifierParam.Value= item.Identifier;
                        opcNamespaceParam.Value = item.OpcUaNamespace;
                        valueParam.Value = "";
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                connection.Close();
            }
        }

        public void UpdateOpcUaItem(string id, string value)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var sql = "UPDATE OpcUaItems SET Value = @newValue WHERE Id = @ID";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newValue", value);
                    command.Parameters.AddWithValue("@ID", id);

                    int rowsAffected = command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
