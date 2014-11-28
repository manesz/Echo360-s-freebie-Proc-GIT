using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Freebie.Models;

namespace Freebie.Libs
{
    public static class CallSP
    {
        public static string[] SP_Insert_Interact_Profile(int account_id)
        {

            //return new string[2] { "0", "-"};
            var db = new EchoContext();
            string conn_str = db.Database.Connection.ConnectionString;
            using (SqlConnection conn = new SqlConnection(conn_str))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string storeProceduce = @"[echo].[dbo].[SP_Insert_Interact_Profile]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storeProceduce;
                    cmd.Parameters.Add(new SqlParameter("Account_ID", account_id));

                    var r = cmd.ExecuteReader();
                    string[] returnResult = new string[2];
                    while (r.Read())
                    {
                        returnResult[0] = r.GetString(0);
                        returnResult[1] = r.GetString(1);
                    }
                    return returnResult;
                }
            }
        }
    }
}