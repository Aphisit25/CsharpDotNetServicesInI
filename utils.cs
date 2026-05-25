//dotnet add package Microsoft.Extensions.Configuration
//dotnet add package Microsoft.Extensions.Configuration.FileExtensions
//dotnet add package Microsoft.Extensions.Configuration.Json
//dotnet add package Microsoft.Extensions.Configuration.Ini
//dotnet add package Microsoft.Extensions.Configuration.Binder

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Ini;


namespace WinDapper.utils
{
    public class utils
    {

        private IConfiguration _config;

        public utils(string fileName = "u1Ecf0.ini")
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile(fileName, optional: false, reloadOnChange: true)
                .Build();
        }

        // ✅ string
        public string Get(string section, string key)
        {
            return _config[$"{section}:{key}"];
        }

        // ✅ generic
        public T GetValue<T>(string section, string key)
        {
            return _config.GetValue<T>($"{section}:{key}");
        }
    }
}
