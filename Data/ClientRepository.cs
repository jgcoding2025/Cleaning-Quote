using Cleaning_Quote.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Cleaning_Quote.Data
{
    public sealed class ClientRepository
    {
        public List<Client> GetAll()
        {
            var list = new List<Client>();

            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT ClientId, DisplayName, AddressLine1, AddressLine2, City, State, Zip, Phone, Email, Notes, CreatedAt, UpdatedAt
FROM Clients
ORDER BY DisplayName;
";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(ReadClient(r));
                }
            }

            return list;
        }

        public List<Client> SearchByName(string search)
        {
            var list = new List<Client>();
            search = search ?? "";

            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT ClientId, DisplayName, AddressLine1, AddressLine2, City, State, Zip, Phone, Email, Notes, CreatedAt, UpdatedAt
FROM Clients
WHERE DisplayName LIKE $q
ORDER BY DisplayName;
";
                cmd.Parameters.AddWithValue("$q", $"%{search}%");

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(ReadClient(r));
                }
            }

            return list;
        }

        public void Insert(Client c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (c.ClientId == Guid.Empty) c.ClientId = Guid.NewGuid();

            c.CreatedAt = DateTime.UtcNow;
            c.UpdatedAt = DateTime.UtcNow;

            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Clients
(ClientId, DisplayName, AddressLine1, AddressLine2, City, State, Zip, Phone, Email, Notes, CreatedAt, UpdatedAt)
VALUES
($ClientId, $DisplayName, $AddressLine1, $AddressLine2, $City, $State, $Zip, $Phone, $Email, $Notes, $CreatedAt, $UpdatedAt);
";

                AddClientParameters(cmd, c);
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Client c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (c.ClientId == Guid.Empty) throw new InvalidOperationException("ClientId is required to update.");

            c.UpdatedAt = DateTime.UtcNow;

            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE Clients SET
  DisplayName = $DisplayName,
  AddressLine1 = $AddressLine1,
  AddressLine2 = $AddressLine2,
  City = $City,
  State = $State,
  Zip = $Zip,
  Phone = $Phone,
  Email = $Email,
  Notes = $Notes,
  UpdatedAt = $UpdatedAt
WHERE ClientId = $ClientId;
";
                AddClientParameters(cmd, c);
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExistsByNameAndAddress(string displayName, string address1, string city, string state, string zip)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                SELECT 1
                FROM Clients
                WHERE lower(DisplayName) = lower($name)
                  AND lower(AddressLine1) = lower($addr1)
                  AND lower(City) = lower($city)
                  AND lower(State) = lower($state)
                  AND Zip = $zip
                LIMIT 1;";
                cmd.Parameters.AddWithValue("$name", displayName ?? "");
                cmd.Parameters.AddWithValue("$addr1", address1 ?? "");
                cmd.Parameters.AddWithValue("$city", city ?? "");
                cmd.Parameters.AddWithValue("$state", state ?? "");
                cmd.Parameters.AddWithValue("$zip", zip ?? "");

                return cmd.ExecuteScalar() != null;
            }
        }

        public void Delete(Guid clientId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"DELETE FROM Clients WHERE ClientId = $id;";
                cmd.Parameters.AddWithValue("$id", clientId.ToString());
                cmd.ExecuteNonQuery();
            }
        }


        public bool Exists(Guid clientId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT 1 FROM Clients WHERE ClientId = $id LIMIT 1;";
                cmd.Parameters.AddWithValue("$id", clientId.ToString());

                var result = cmd.ExecuteScalar();
                return result != null;
            }
        }


        private static void AddClientParameters(SqliteCommand cmd, Client c)
        {
            cmd.Parameters.AddWithValue("$ClientId", c.ClientId.ToString());
            cmd.Parameters.AddWithValue("$DisplayName", c.DisplayName ?? "");
            cmd.Parameters.AddWithValue("$AddressLine1", c.AddressLine1 ?? "");
            cmd.Parameters.AddWithValue("$AddressLine2", (object)(c.AddressLine2 ?? ""));

            cmd.Parameters.AddWithValue("$City", c.City ?? "");
            cmd.Parameters.AddWithValue("$State", c.State ?? "");
            cmd.Parameters.AddWithValue("$Zip", c.Zip ?? "");

            cmd.Parameters.AddWithValue("$Phone", (object)(c.Phone ?? ""));
            cmd.Parameters.AddWithValue("$Email", (object)(c.Email ?? ""));
            cmd.Parameters.AddWithValue("$Notes", (object)(c.Notes ?? ""));

            cmd.Parameters.AddWithValue("$CreatedAt", c.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$UpdatedAt", c.UpdatedAt.ToString("o"));
        }

        private static Client ReadClient(SqliteDataReader r)
        {
            return new Client
            {
                ClientId = Guid.Parse(r.GetString(0)),
                DisplayName = r.GetString(1),
                AddressLine1 = r.GetString(2),
                AddressLine2 = r.IsDBNull(3) ? "" : r.GetString(3),
                City = r.GetString(4),
                State = r.GetString(5),
                Zip = r.GetString(6),
                Phone = r.IsDBNull(7) ? "" : r.GetString(7),
                Email = r.IsDBNull(8) ? "" : r.GetString(8),
                Notes = r.IsDBNull(9) ? "" : r.GetString(9),
                CreatedAt = DateTime.Parse(r.GetString(10)),
                UpdatedAt = DateTime.Parse(r.GetString(11))
            };
        }
    }
}
