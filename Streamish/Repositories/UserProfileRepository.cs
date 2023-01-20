using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Streamish.Models;
using Streamish.Utils;

namespace Streamish.Repositories
{

    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        public UserProfileRepository(IConfiguration configuration) : base(configuration) { }

        public List<UserProfile> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT Id, Name, Email, ImageUrl, DateCreated
                            FROM UserProfile
                        ORDER BY DateCreated
                    ";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var profiles = new List<UserProfile>();
                        while (reader.Read())
                        {
                            profiles.Add(new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "Id"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated"),

                            });
                        }

                        return profiles;
                    }
                }
            }
        }

        public UserProfile GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT Name, Email, ImageUrl, DateCreated
                            FROM UserProfile
                           WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        UserProfile profile = null;
                        if (reader.Read())
                        {
                            profile = new UserProfile()
                            {
                                Id = id,
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated"),


                            };
                        }

                        return profile;
                    }
                }
            }
        }

        public UserProfile GetUserProfileByIdWithVideoList(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT up.Name, up.Email, up.ImageUrl, up.DateCreated,
                                               v.Id AS VideoId, v.Title, v.Description,
                                               v.Url, v.DateCreated AS VideoDateCreated

                                        FROM UserProfile up

                                        LEFT JOIN Video v ON v.UserProfileId = up.Id
                                        WHERE up.Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        UserProfile userProfile = null;
                        while (reader.Read())
                        {
                            if (userProfile == null)
                            {
                                userProfile = new UserProfile
                                {
                                    Id = id,
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                    DateCreated = DbUtils.GetDateTime(reader, "DateCreated"),
                                    UserVideos = new List<Video>()
                                };

                            }
                            if (DbUtils.IsNotDbNull(reader, "VideoId"))
                            {
                                userProfile.UserVideos.Add(new Video
                                {
                                    Id = DbUtils.GetInt(reader, "VideoId"),
                                    Title = DbUtils.GetString(reader, "Title"),
                                    Description = DbUtils.GetString(reader, "Description"),
                                    Url = DbUtils.GetString(reader, "Url"),
                                    DateCreated = DbUtils.GetDateTime(reader, "VideoDateCreated"),
                                    UserProfileId = id
                                });
                            }
                        }
                        return userProfile;
                    }
                }
            }
        }






        public void Add(UserProfile profile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO UserProfile (Name, Email, ImageUrl, DateCreated)
                        OUTPUT INSERTED.ID
                        VALUES (@Name, @Email, @ImageUrl, @DateCreated)";

                    DbUtils.AddParameter(cmd, "@Title", profile.Name);
                    DbUtils.AddParameter(cmd, "@Description", profile.Email);
                    DbUtils.AddParameter(cmd, "@DateCreated", profile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@Url", profile.DateCreated);


                    profile.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(UserProfile profile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE UserProfile
                           SET Name = @Name,
                               Email = @Email,
                               Url = @Url,
                               DateCreated = @DateCreated
                               
                         WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Name", profile.Name);
                    DbUtils.AddParameter(cmd, "@Description", profile.Email);
                    DbUtils.AddParameter(cmd, "@ImageUrl", profile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@DateCreated", profile.DateCreated);
                    DbUtils.AddParameter(cmd, "@Id", profile.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM UserProfile WHERE Id = @Id";
                    DbUtils.AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}