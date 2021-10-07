using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace UploadImagesProgram
{
    public static class Loader
    {

        public static ModelInfo File(string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "E:\\Txema\\Charlas\\Netcoreconf 2021\\Virtual 2\\ailab\\Sketch2Code\\model\\dataset.min.json";
            var content = System.IO.File.ReadAllText(fileName);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(content);
            return new ModelInfo() { Images = obj };
        }

        public static List<string> GetImages(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "E:\\Txema\\Charlas\\Netcoreconf 2021\\Virtual 2\\ailab\\Sketch2Code\\model\\images";
            return System.IO.Directory.GetFiles(path).ToList();
        }
    }
}
