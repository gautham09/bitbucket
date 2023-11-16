using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Galytix.Controllers
{
    [Route("api/gwp")]
    [ApiController]
    public class CountryGwpController : ControllerBase
    {
        private readonly string filePath = "Data/gwpByCountry.csv";

        [HttpPost("avg")]
        public async Task<IActionResult> GetAverageGwpAsync([FromBody] JObject requestData)
        {
            if (requestData == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                string country = requestData["country"].ToString().ToLower();
                List<string> linesOfBusiness = requestData["lob"].ToObject<List<string>>().Select(lob => lob.ToLower()).ToList();

                // Read CSV file asynchronously
                var csvData = await System.IO.File.ReadAllLinesAsync(filePath);

                // Process data asynchronously
                var filteredData = csvData
                    .Skip(1) // Skip header
                    .Select(line => line.Split(','))
                    .Where(row => row[0].ToLower() == country && linesOfBusiness.Contains(row[1].ToLower()))
                    .Select(row => new { LineOfBusiness = row[1], Gwp = double.Parse(row[2]) });

                // Calculate average GWP asynchronously
                var averageGwp = await Task.Run(() =>
                {
                    return filteredData
                        .GroupBy(item => item.LineOfBusiness)
                        .ToDictionary(g => g.Key, g => g.Average(item => item.Gwp));
                });

                return Ok(averageGwp);
            }
            catch (Exception ex)
            {
                // Implement basic error handling
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
