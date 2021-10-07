using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace UploadImagesProgram
{
    class Program
    {

        static string trainingKey = "6bb39a269eab4d0c89473310bebbebf2";
        static string trainingEndPoint = "https://dibujo2code.cognitiveservices.azure.com/";
        static string predictionKey = "101cb86d668e48e596b28a85f5b056eb";
        static string predictionEndPoint = "https://dibujo2code-prediction.cognitiveservices.azure.com/";

        static List<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Tag> Tags = new List<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Tag>();
        static ModelInfo model = Loader.File();
        static Iteration Iteration;

        static void Main(string[] args)
        {

            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndPoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndPoint, predictionKey);

            Project project = CreateProject(trainingApi);
            AddTags(trainingApi, project, model);
            UploadImages(trainingApi, project);
            TrainProject(trainingApi, project);
            PublishIteration(trainingApi, project);
            
        }

        private static CustomVisionTrainingClient AuthenticateTraining(string trainingEndPoint, string trainingKey)
        {
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = trainingEndPoint
            };
            return trainingApi;
        }
        private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {

            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }


        private static Project CreateProject(CustomVisionTrainingClient trainingApi)
        {

            var domains = trainingApi.GetDomains();
            var objDetectionDomain = domains.FirstOrDefault(d => d.Type == "ObjectDetection");


            Console.WriteLine("Creando nuevo proyecto:");
            Console.Out.Flush();
            return trainingApi.CreateProject("dibujo2codigo", null, objDetectionDomain.Id);
        }


        private static void AddTags(CustomVisionTrainingClient trainingApi, Project project, ModelInfo model)
        {
            List<string> tagsNames = new List<string>();

            model.Images.ToList().ForEach(img =>
            {
                img.tags.ToList().ForEach(tag =>
                {
                    if (!tagsNames.Contains(tag.tagName))
                    {
                        tagsNames.Add(tag.tagName);
                        var tg = trainingApi.CreateTag(project.Id, tag.tagName);
                        Console.WriteLine("Creando Tag " + tg.Name + " con ID  " + tg.Id);
                        Tags.Add(tg);
                    }
                });
            });

        }


        private static void UploadImages(CustomVisionTrainingClient trainingApi, Project project)
        {

            var globalEntries = new List<List<ImageFileCreateEntry>>();

            var entries = new List<ImageFileCreateEntry>();

            Loader.GetImages().ForEach(img =>
            {

                if (entries.Count > 62)
                {
                    globalEntries.Add(entries);
                    entries = new List<ImageFileCreateEntry>();
                    Console.WriteLine("Hemos llegado a las 63 entradas...");
                    Console.Out.Flush();
                }

                var name = System.IO.Path.GetFileNameWithoutExtension(img);
                Console.WriteLine("Imagen " + name);
                Console.Out.Flush();
                var info = model.Images.FirstOrDefault(x => x.id == name);
                if (info != null)
                {
                    List<Guid> tagsIDs = new List<Guid>();
                    List<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Region> regions = new List<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Region>();

                    info.regions.ToList().ForEach(reg =>
                    {
                        var tagInfo = Tags.FirstOrDefault(x => x.Name == reg.tagName);
                        Console.WriteLine("Tag " + reg.tagName + " encontrado = " + tagInfo == null);
                        Console.Out.Flush();
                        tagsIDs.Add(tagInfo.Id);
                        regions.Add(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Region()
                        {
                            Height = reg.height,
                            Left = reg.left,
                            TagId = tagInfo.Id,
                            Top = reg.top,
                            Width = reg.width
                        });
                        Console.WriteLine("Region agregada con el tag" + tagInfo.Name + " - " + tagInfo.Id);
                        Console.Out.Flush();
                    });

                    entries.Add(new ImageFileCreateEntry()
                    {
                        Name = name,
                        Contents = File.ReadAllBytes(img),
                        Regions = regions,
                        TagIds = tagsIDs
                    });
                    Console.WriteLine("Entrada agregada " + name);
                    Console.Out.Flush();
                }
            });

            var pos  = 1;
            foreach(var ent in globalEntries)
            {
                trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(ent));
                Console.WriteLine("Creando " + pos + " de " + globalEntries.Count);
                Console.Out.Flush();
                pos++;
            }
           

        }

        private static void TrainProject(CustomVisionTrainingClient trainingApi, Project project)
        {            
            Console.WriteLine("\tEntrenando");
            Iteration = trainingApi.TrainProject(project.Id);
            
            while (Iteration.Status == "Training")
            {
                Thread.Sleep(1000);

            
                Iteration = trainingApi.GetIteration(project.Id, Iteration.Id);
            }



        }

        private static void PublishIteration(CustomVisionTrainingClient trainingApi, Project project)
        {

            
            var publishedModelName = "2codeModel";
            var predictionResourceId = "/subscriptions/a83e907d-9362-438e-a90f-6fbc8ef1313a/resourceGroups/netcoreconf/providers/Microsoft.CognitiveServices/accounts/dibujo2code-Prediction";
            trainingApi.PublishIteration(project.Id, Iteration.Id, publishedModelName, predictionResourceId);
            Console.WriteLine("Listo!\n");
        }
    }
}
