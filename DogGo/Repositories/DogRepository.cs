using DogGo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogGo.Repositories
{
    public class DogRepository : IDogRepository
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public DogRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public List<Dog> GetAllDogs()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id, d.[Name], d.Breed,  d.OwnerId, d.Notes, d.ImageURL
                        FROM Dog d
                        
                    ";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dog> dogs = new List<Dog>();
                    while (reader.Read())
                    {

                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
                            //ImageURL = reader.GetString(reader.GetOrdinal("ImageURL"))
                        };

                        //Check if optional columns are null
                            if (reader.IsDBNull(reader.GetOrdinal("ImageUrl")) == false)
                        {
                            dog.ImageURL = reader.GetString(reader.GetOrdinal("ImageUrl"));
                        }
                        
                    dogs.Add(dog);
                }
                reader.Close();
                return dogs;
            }
        }
    }

    public Dog GetDogById(int id)
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                        SELECT Id, [Name], Breed,  OwnerId, Notes, ImageURL
                        FROM Dog
                        WHERE Id = @id";

                cmd.Parameters.AddWithValue("@id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Dog dog = new Dog()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Breed = reader.GetString(reader.GetOrdinal("Breed")),
                        OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
                        //ImageURL = reader.GetString(reader.GetOrdinal("ImageURL"))
                    };

                        //Check if optional columns are null
                        if (reader.IsDBNull(reader.GetOrdinal("ImageUrl")) == false)
                        {
                            dog.ImageURL = reader.GetString(reader.GetOrdinal("ImageUrl"));
                        }

                        reader.Close();
                    return dog;
                }

                reader.Close();
                return null;
            }
        }
    }

        public List<Dog> GetDogsByOwnerId(int ownerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT Id, Name, Breed, Notes, ImageUrl, OwnerId 
                FROM Dog
                WHERE OwnerId = @ownerId
            ";
                    cmd.Parameters.AddWithValue("@ownerId", ownerId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dog> dogs = new List<Dog>();
                    while (reader.Read())
                    {
                        Dog dog = new Dog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                        };
                        // Check if optional columns are null
                        if (reader.IsDBNull(reader.GetOrdinal("Notes")) == false)
                        {
                            dog.Notes = reader.GetString(reader.GetOrdinal("Notes"));
                        }
                        if (reader.IsDBNull(reader.GetOrdinal("ImageURL")) == false)
                        {
                            dog.ImageURL = reader.GetString(reader.GetOrdinal("ImageURL"));
                        }
                        dogs.Add(dog);
                    }
                    reader.Close();
                    return dogs;
                }
            }
        }



        public void AddDog(Dog dog)
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Dog ([Name], Breed, Notes, ImageUrl, OwnerId )
                    OUTPUT INSERTED.ID
                    VALUES (@name,@breed,@notes,@imageurl, @ownerid);
                ";

                cmd.Parameters.AddWithValue("@name", dog.Name);
                cmd.Parameters.AddWithValue("@breed", dog.Breed);
                cmd.Parameters.AddWithValue("@ownerId", dog.OwnerId);
               
                

                    if(dog.Notes == null)
                    {
                        cmd.Parameters.AddWithValue("@notes", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@notes", dog.Notes);
                    }


                    if (dog.ImageURL == null)
                    {
                        cmd.Parameters.AddWithValue("@imageurl", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@imageURL", dog.ImageURL);
                    }

                    int id = (int)cmd.ExecuteScalar();

                dog.Id = id;
            }
        }
    }

    public void UpdateDog(Dog dog)
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                            UPDATE Dog
                            SET 
                                [Name] = @name, 
                                Email = @email, 
                                Address = @address, 
                                Phone = @phone, 
                                NeighborhoodId = @neighborhoodId
                            WHERE Id = @id";

                cmd.Parameters.AddWithValue("@name", dog.Name);
                cmd.Parameters.AddWithValue("@breed", dog.Breed);
                cmd.Parameters.AddWithValue("@ownerId", dog.OwnerId);
                cmd.Parameters.AddWithValue("@notes", dog.Notes);
                cmd.Parameters.AddWithValue("@imageURL", dog.ImageURL);
                cmd.Parameters.AddWithValue("@id", dog.Id);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void DeleteDog(int dogId)
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                            DELETE FROM Dog
                            WHERE Id = @id
                        ";

                cmd.Parameters.AddWithValue("@id", dogId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
}
