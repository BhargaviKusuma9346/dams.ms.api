using Dams.ms.auth.Models;
using Dams.ms.auth.Utility;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dams.ms.auth.Repositories
{
    public class EmailRepository
    {

        public async Task<EmailTemplatesResponse> GetEmailTemplates(EmailTemplatesQuery query)
        {
            string connectionString = ConnectionString.EmpcName;
            EmailTemplatesResponse emailTemplatesResponse = new EmailTemplatesResponse();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("[dbo].[spGetEmailTemplates]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", query.Name);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader["TemplateHtml"] != DBNull.Value)
                        {
                            emailTemplatesResponse.TemplateHtml = (reader["TemplateHtml"].ToString());
                        }
                        if (reader["ReplaceableVariables"] != DBNull.Value)
                        {
                            emailTemplatesResponse.ReplaceableVariables = (reader["ReplaceableVariables"].ToString());
                        }

                    }

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    con.Close();

                }

                return emailTemplatesResponse;
            }
        }
    }
}
