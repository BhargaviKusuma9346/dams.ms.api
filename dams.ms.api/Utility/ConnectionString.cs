using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Utility
{
    public static class ConnectionString
    {
        private static string cName = "Data Source=183.82.117.19; Initial Catalog=IDM;User ID=sa;Password=dev1@env";
        private static string employerName = "Data Source=183.82.117.19; Initial Catalog=Employer;User ID=sa;Password=dev1@env";
        private static string empcName = "server=192.168.0.184;database=YodaDAMS;user id=sa;password=dev1@env";

        public static string CName
        {
            get => cName;
        }
        public static string EmployerName
        {
            get => employerName;
        }
        public static string EmpcName
        {
            get => empcName;
        }
    }
}
