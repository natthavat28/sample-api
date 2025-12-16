using Microsoft.AspNetCore.Mvc;

namespace sample_api.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly ISampleExternalApiCaller apiCaller;

        public TestController(ISampleExternalApiCaller apiCaller)
        {
            this.apiCaller = apiCaller;
        }

        [HttpGet("action-1")]
        public async Task<IActionResult> Action1()
        {
            return Ok();
        }

        [HttpGet("action-2")]
        public async Task<IActionResult> Action2()
        {
            try
            {
                var res = await apiCaller.ListAllObject();
                res.EnsureSuccessStatusCode();

                var content = await res.Content.ReadAsStringAsync();
                var jsonObj = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                return Ok(jsonObj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("action-3/{itemId}")]
        public async Task<IActionResult> Action3(int itemId)
        {
            try
            {
                var res = await apiCaller.GetObjectById(itemId);
                res.EnsureSuccessStatusCode();

                var content = await res.Content.ReadAsStringAsync();
                var jsonObj = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                return Ok(jsonObj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
