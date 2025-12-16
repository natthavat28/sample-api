using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.IO;

namespace sample_api.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly ISampleExternalApiCaller apiCaller;
        private readonly IBeeceptorApiCaller beeceptorApiCaller;

        public TestController(ISampleExternalApiCaller apiCaller, IBeeceptorApiCaller beeceptorApiCaller)
        {
            this.apiCaller = apiCaller;
            this.beeceptorApiCaller = beeceptorApiCaller;
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

        [HttpGet("action-4")]
        public async Task<IActionResult> Action4()
        {
            try
            {
                var res = await beeceptorApiCaller.GetData();
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

        [HttpGet("action-5")]
        public async Task<IActionResult> Action5()
        {
            try
            {
                var res1 = await apiCaller.GetObjectById(1);
                res1.EnsureSuccessStatusCode();

                var res2 = await beeceptorApiCaller.GetData();
                res2.EnsureSuccessStatusCode();

                var content1 = await res1.Content.ReadAsStringAsync();
                var jsonObj1 = System.Text.Json.JsonSerializer.Deserialize<object>(content1);
                var content2 = await res2.Content.ReadAsStringAsync();
                var jsonObj2 = System.Text.Json.JsonSerializer.Deserialize<object>(content2);

                return Ok(new { resp1 = jsonObj1, resp2 = jsonObj2 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("action-6")]
        public async Task<IActionResult> Action6()
        {
            try
            {
                // Read the raw request body
                using var reader = new StreamReader(Request.Body);
                var content = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return BadRequest("Request body is empty");
                }

                // Deserialize to an object so the response is returned as JSON
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
