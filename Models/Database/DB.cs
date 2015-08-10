using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace TemGente.Models.Database
{
    public class DB : IDisposable
    {
        static readonly string connectionString = ConfigurationManager.ConnectionStrings["STRING_CONEXAO"].ConnectionString;

        public static IEnumerable<T> Get<T>(string query, Dictionary<string, object> param, CommandType ct) where T : class
        {
            try
            {
                DynamicParameters dynamicParameters = new DynamicParameters();

                if (param.Count > 0)
                    dynamicParameters = SetDynamicParameters(param);

                using (var sqlConnection = new MySqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    var data = sqlConnection.Query<T>(query, dynamicParameters, commandType: ct);

                    sqlConnection.Close();

                    return data;
                }
            }
            catch (Exception e) 
            {
                Util.LogError(e);
                throw e;
            }
        }

        public static int Save(string query, Dictionary<string, object> param)
        {
            try { 
                DynamicParameters dynamicParameters = SetDynamicParameters(param);

                using (var sqlConnection = new MySqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    var data = sqlConnection.Execute(query, dynamicParameters);

                    sqlConnection.Close();

                    return data;
                }
            }
            catch (Exception e)
            {
                Util.LogError(e);
                throw e;
            }
        }

        private static DynamicParameters SetDynamicParameters(Dictionary<string, object> param)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();

            foreach (KeyValuePair<string, object> keyPair in param)
            {
                dynamicParameters.Add(keyPair.Key, keyPair.Value);
            }
            return dynamicParameters;
        }


        public void Dispose() { }
    }
}
