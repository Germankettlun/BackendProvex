using Microsoft.Data.SqlClient;
using ProvexApi.Helper;


namespace ProvexApi.Services.SDT
{

    public class FacilitieServices
    {
        private readonly IConfiguration _configuration;

        public FacilitieServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }


    }
}
