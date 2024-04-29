using Dams.ms.auth.Models;
using Dams.ms.auth.Reflections;
using Dams.ms.auth.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Dams.ms.auth.Services
{
    public class EmailTemplateService
    {
        private IConfigurationRoot _objConfiguration;
        public readonly string SQLConnectionString;

        public EmailTemplateService(IConfigurationRoot objConfiguration)
        {
            _objConfiguration = objConfiguration;
            SQLConnectionString = _objConfiguration.GetConnectionString("DefaultConnection");

        }

        public async Task<EmailTemplatesResponse> GetEmailTemplates(EmailTemplatesQuery query)
        {
            // string connectionString = ConnectionString.EmpcName;
            EmailTemplatesResponse emailTemplatesResponse = new EmailTemplatesResponse();
            using (SqlConnection con = new SqlConnection(SQLConnectionString))
            {
                //try
                //{
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

                con.Close();

                return emailTemplatesResponse;
            }
        }
    }
}
