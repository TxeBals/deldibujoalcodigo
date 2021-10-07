using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.ProjectOxford.Vision;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dibujo2Code.AI
{

    public abstract class CustomVisionClient
    {
        protected string _trainingApiKey;
        protected string _predictionApiKey;
        protected string _trainingEndPoint;
        protected string _predictionEndPoint;
        private CustomVisionTrainingClient _trainingApi;
        protected string _projectName;
        protected string _projectModelName;
        protected Project _project;
        protected Iteration _iteration;
        protected VisionServiceClient _visionClient;

        public CustomVisionTrainingClient TrainingApi { get => _trainingApi; }

        public CustomVisionClient(string trainingKey, string trainingEndPoint, string predictionKey, string predictionEndPoint, string projectName,string projectModelName)
        {
            _trainingApiKey = trainingKey;
            _trainingEndPoint = trainingEndPoint;
            _predictionApiKey = predictionKey;
            _predictionEndPoint = predictionEndPoint;
            _projectName = projectName;
            _projectModelName = projectModelName;
            this._trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = trainingEndPoint
            };

            _visionClient = new VisionServiceClient(ConfigurationManager.AppSettings["textKey"],
                ConfigurationManager.AppSettings["textEndPoint"]);
        }

        protected async Task<Project> GetProjectAsync(string projectName)
        {
            if (String.IsNullOrWhiteSpace(projectName)) throw new ArgumentNullException("Nombre de Projecto");
            var projects = await this._trainingApi.GetProjectsAsync();

            return projects.SingleOrDefault(p => p.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void Initialize()
        {
            if (String.IsNullOrWhiteSpace(_projectName)) throw new ArgumentNullException("Nombre de Projecto");
            var projects = this._trainingApi.GetProjects();

            this._project = projects.SingleOrDefault(p => p.Name.Equals(_projectName, StringComparison.InvariantCultureIgnoreCase));

            if (_project == null) throw new InvalidOperationException($"Error al inizializar CustomVision. ({_projectName} No encontrado.)");


            SetDefaultIteration(ConfigurationManager.AppSettings["ObjectDetectionIterationName"]);
        }

        protected async Task<IList<Iteration>> GetIterations(string projectName)
        {
            if (String.IsNullOrWhiteSpace(projectName)) throw new ArgumentNullException("Nombre de Projecto");

            var prj = await this.GetProjectAsync(projectName);

            var iterations = await this._trainingApi.GetIterationsAsync(prj.Id);

            return iterations;
        }

        public virtual async Task SetDefaultIterationAsync(string iterationName)
        {
            if (_project == null) throw new InvalidOperationException("No se ha especificado el nombre del proyecto");

            var iterations = await this._trainingApi.GetIterationsAsync(_project.Id);

            var iteration = iterations.SingleOrDefault(i => i.Name == iterationName);

            if (iteration == null) throw new InvalidOperationException($"Iteración {iterationName} no encontrada");
           

            await _trainingApi.UpdateIterationAsync(_project.Id, iteration.Id, iteration);
        }

        public virtual void SetDefaultIteration(string iterationName)
        {
            if (_project == null) throw new InvalidOperationException("No se ha especificado el nombre del proyecto");

            var iterations = this._trainingApi.GetIterations(_project.Id);

            var iteration = iterations.SingleOrDefault(i => i.Name == iterationName);

            if (iteration == null) throw new InvalidOperationException($"Iteración {iterationName} no encontrada");
    
            this._trainingApi.UpdateIteration(_project.Id, iteration.Id, iteration);
        }
    }

}
