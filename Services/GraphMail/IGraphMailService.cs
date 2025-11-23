using System.Collections.Generic;
using System.Threading.Tasks;
using ProvexApi.Models.GraphMail;

namespace ProvexApi.Services.GraphMail
{
    public interface IGraphMailService
    {
        Task<List<GraphMailResult>> GetMailWeightByYearAndSenderAsync(string mailbox, ProcessingOptions options = null);
    }
}