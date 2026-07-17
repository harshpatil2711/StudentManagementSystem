using Microsoft.Practices.EnterpriseLibrary.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BusinessLayer1.Models;

namespace BusinessLayer1.DAL
{
    public class AuthDAL
    {
        private Database db;

        public AuthDAL()
        {
            db = DatabaseFactory.CreateDatabase();
        }

        public string RegisterUser(SignUpViewModel model, string passwordHash)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_RegisterUser");

                db.AddInParameter(cmd, "@FirstName", DbType.String, model.FirstName);
                db.AddInParameter(cmd, "@LastName", DbType.String, model.LastName);
                db.AddInParameter(cmd, "@Email", DbType.String, model.Email);
                db.AddInParameter(cmd, "@PhoneNumber", DbType.String,
                    string.IsNullOrEmpty(model.PhoneNumber) ? (object)DBNull.Value : model.PhoneNumber);
                db.AddInParameter(cmd, "@Username", DbType.String, model.Username);
                db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);
                db.AddInParameter(cmd, "@RoleId", DbType.Int32, model.RoleId);
                db.AddInParameter(cmd, "@CreatedBy", DbType.String, model.Username);

                db.AddOutParameter(cmd, "@Message", DbType.String, 200);

                db.ExecuteNonQuery(cmd);
                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error registering user {Username}", model.Username);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public UserSession AuthenticateUser(string username, out string passwordHash)
        {
            passwordHash = null;
            UserSession user = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_AuthenticateUser");
                db.AddInParameter(cmd, "@Username", DbType.String, username);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        passwordHash = reader["PasswordHash"].ToString();
                        user = new UserSession
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            RoleName = reader["RoleName"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error authenticating user {Username}", username);
            }
            return user;
        }

        public Dictionary<int, string> GetRoles()
        {
            Dictionary<int, string> roles = new Dictionary<int, string>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetRoles");
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["RoleId"]);
                        string name = reader["RoleName"].ToString();
                        roles.Add(id, name);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading roles");
            }
            return roles;
        }

        public string UpdateLastLogin(int userId)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_UpdateLastLogin");
                db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
                db.AddOutParameter(cmd, "@Message", DbType.String, 100);
                db.ExecuteNonQuery(cmd);
                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating last login for user {UserId}", userId);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public void SaveRefreshToken(int userId, string tokenHash, string deviceName, string ipAddress)
        {
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_SaveRefreshToken");
                db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
                db.AddInParameter(cmd, "@TokenHash", DbType.String, tokenHash);
                db.AddInParameter(cmd, "@DeviceName", DbType.String, string.IsNullOrEmpty(deviceName) ? "" : deviceName);
                db.AddInParameter(cmd, "@IpAddress", DbType.String, string.IsNullOrEmpty(ipAddress) ? "" : ipAddress);
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving refresh token for user {UserId}", userId);
            }
        }

        public RefreshToken GetRefreshToken(string tokenHash)
        {
            RefreshToken token = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetRefreshToken");
                db.AddInParameter(cmd, "@TokenHash", DbType.String, tokenHash);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        token = new RefreshToken
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            TokenHash = reader["TokenHash"].ToString(),
                            ExpiresAt = Convert.ToDateTime(reader["ExpiresAt"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            RevokedAt = reader["RevokedAt"] != DBNull.Value ? Convert.ToDateTime(reader["RevokedAt"]) : (DateTime?)null,
                            DeviceName = reader["DeviceName"].ToString(),
                            IpAddress = reader["IpAddress"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting refresh token");
            }
            return token;
        }

        public void RevokeRefreshToken(int id)
        {
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_RevokeRefreshToken");
                db.AddInParameter(cmd, "@Id", DbType.Int32, id);
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error revoking refresh token {Id}", id);
            }
        }

        public void RevokeAllUserTokens(int userId)
        {
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_RevokeAllUserTokens");
                db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error revoking all tokens for user {UserId}", userId);
            }
        }

        public UserSession GetUserById(int userId)
        {
            UserSession user = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetUserById");
                db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        user = new UserSession
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            RoleName = reader["RoleName"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user by ID {UserId}", userId);
            }
            return user;
        }
    }
}
