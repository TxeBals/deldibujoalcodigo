using Dibujo2Code.Core.Entities;
using Dibujo2Code.Core.Entities.BoxGeometry;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dibujo2Code.Core.Services.Interfaces
{
    public interface IObjectDetectionAppService
    {
        Task<IList<PredictedObject>> GetPredictionAsync(byte[] image);
        Task<PredictionDetail> GetPredictionAsync(string folderId);
        Task SaveResults(byte[] file, string container, string fileName);
        Task SaveResults(IList<PredictedObject> predictedObjects, string id);
        Task<IList<CloudBlobContainer>> GetPredictionsAsync();
        Task<byte[]> GetFile(string container, string file);
        Task<GroupBox> CreateGroupBoxAsync(IList<PredictedObject> predictedObjects);
    }
}
