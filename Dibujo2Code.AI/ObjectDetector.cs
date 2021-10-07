using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dibujo2Code.AI
{
    public class ObjectDetector : CustomVisionClient
    {

        public ObjectDetector()
            : base(ConfigurationManager.AppSettings["trainingKey"],
                   ConfigurationManager.AppSettings["trainingEndPoint"],
                   ConfigurationManager.AppSettings["predictionKey"],
                   ConfigurationManager.AppSettings["predictionEndPoint"],
                   ConfigurationManager.AppSettings["projectName"],
                   ConfigurationManager.AppSettings["projectModelName"])
        {

        }

        public ObjectDetector(string trainingKey, string trainingEndPoint, string predictionKey, string predictionEndPoint, string projectName, string projectModelName)
            : base(trainingKey, trainingEndPoint, predictionKey, predictionEndPoint, projectName, projectModelName)
        {

        }

        public async Task<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction> GetDetectedObjects(byte[] image)
        {

            using (var endpoint = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(this._predictionApiKey))
            {
                Endpoint = this._predictionEndPoint
            })
            {
                using(var ms = new MemoryStream(image))
                {
                    return await endpoint.DetectImageAsync(this._project.Id, this._projectModelName, ms);
                }
            }            
        }

        public async Task<List<String>> GetText(byte[] image)
        {
            var list = new List<String>();
            try
            {
                using (var ms = new MemoryStream(image))
                {
                    var operation = await _visionClient.CreateHandwritingRecognitionOperationAsync(ms);
                    var result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);

                    while (result.Status != Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Succeeded)
                    {
                        if (result.Status == Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Failed)
                            return new List<string>(new string[] { "La predicción de texto ha fallado" });

                        await Task.Delay(Convert.ToInt32(ConfigurationManager.AppSettings["ComputerVisionDelay"]));

                        result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                    }
                    list = result.RecognitionResult.Lines.SelectMany(l => l.Words?.Select(w => w.Text)).ToList();
                }
            }
            catch (ClientException ex)
            {
                list.Add($"La predicción de texto a fallado: {ex.Error.Message}. Id: {ex.Error.Code}.");
            }
            return list;
        }

        public async Task<HandwritingTextLine[]> GetTextRecognition(byte[] image)
        {
            try
            {
                using (var ms = new MemoryStream(image))
                {
                    var operation = await _visionClient.CreateHandwritingRecognitionOperationAsync(ms);
                    var result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);

                    while (result.Status != Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Succeeded)
                    {
                        if (result.Status == Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Failed)
                        {
                            return null;
                        }

                        await Task.Delay(Convert.ToInt32(ConfigurationManager.AppSettings["ComputerVisionDelay"]));
                        result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                    }

                    return result.RecognitionResult.Lines;

                }
            }
            catch (ClientException ex)
            {
                return null;
            }
        }
    }
}
