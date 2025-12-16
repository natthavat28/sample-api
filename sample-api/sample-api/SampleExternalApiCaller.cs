using Refit;

namespace sample_api
{
    public interface ISampleExternalApiCaller
    {
        [Get("/objects")]
        Task<HttpResponseMessage> ListAllObject();

        [Get("/objects/{objectId}")]
        Task<HttpResponseMessage> GetObjectById(int objectId);
    }
}
