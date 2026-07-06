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
    }
}
