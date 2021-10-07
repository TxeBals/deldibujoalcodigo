using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Dibujo2Code.Core.Services;
using Dibujo2Code.Core.Helpers;

namespace Dibujo2Code.Api
{
    public static class ObjectDetection
    {
        [FunctionName("ObjectDetection")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            IActionResult funcResult = null;

            log.LogInformation("Recuperamos la imagen....");

            var data = await req.ReadFormAsync();
            var file  = req.Form.Files[0];

            byte[] content;

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                content = ms.ToArray();
            }
            

            if (content == null)
                funcResult = new BadRequestObjectResult("No se ha la imagen para ser procesada...");
            else
                log.LogInformation($"La imagen recibida es de {content.Length} bytes");

            var correlationID = Guid.NewGuid().ToString();


            var objectDetector = new ObjectDetectionAppService();
            var result = await objectDetector.GetPredictionAsync(content);

            if (result != null)
            {
                await objectDetector.SaveResults(result, correlationID);
                await objectDetector.SaveResults(content, correlationID, "original.png");
                await objectDetector.SaveResults(content.DrawRectangle(result), correlationID, "predicted.png");
                byte[] jsonContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                await objectDetector.SaveResults(jsonContent, correlationID, "results.json");
                var groupBox = await objectDetector.CreateGroupBoxAsync(result);
                await objectDetector.SaveResults(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupBox)), correlationID, "groups.json");
            }

            funcResult = new OkObjectResult(new { Id = correlationID });
            return funcResult;
        }
    }
}
