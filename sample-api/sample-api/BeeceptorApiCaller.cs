using Microsoft.AspNetCore.Mvc;
using Refit;

namespace sample_api
{
    public interface IBeeceptorApiCaller
    {
        [Get("/data")]
        Task<HttpResponseMessage> GetData();
    }
}
