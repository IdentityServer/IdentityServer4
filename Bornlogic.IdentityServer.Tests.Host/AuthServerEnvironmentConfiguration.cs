using Bornlogic.Common.EnvironmentConfiguration.AspnetCore;
using Bornlogic.Common.EnvironmentConfiguration.AspnetCore.Attributes;

namespace Bornlogic.IdentityServer.Tests.Host
{
    public class AuthServerEnvironmentConfiguration : EnvironmentConfiguration
    {
        public AuthServerEnvironmentConfiguration(bool isDev, string contentRootPath, string jsonFileName = "appsettings.json", EnvironmentVariableTarget? environmentVariableTarget = null)
            : base(isDev, contentRootPath, jsonFileName, environmentVariableTarget)
        {
        }

        public AuthServerEnvironmentConfiguration(IWebHostEnvironment hostingEnvironment, string jsonFileName = "appsettings.json", EnvironmentVariableTarget? environmentVariableTarget = null)
            : this(hostingEnvironment.IsDevelopment(), hostingEnvironment.ContentRootPath, jsonFileName, environmentVariableTarget)
        {
        }
        
        [EnvironmentValue("AuthServerMongo")]
        public AuthServerMongoEnvironmentConfiguration AuthServerMongo { get; set; }

        public class AuthServerMongoEnvironmentConfiguration
        {
            [EnvironmentValue("ConnectionString", "Auth_Server_MongoDB_ConnectionString")]
            public string ConnectionString { get; set; }

            [EnvironmentValue("DatabaseName", "Auth_Server_MongoDB_DatabaseName")]
            public string DatabaseName { get; set; }
        }
    }
}
