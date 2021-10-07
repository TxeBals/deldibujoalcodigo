using System;
using System.Collections.Generic;
using System.Text;

namespace UploadImagesProgram
{

    public class Tag
    {
        public string tagId { get; set; }
        public string tagName { get; set; }
        public DateTime created { get; set; }
    }

    public class Region
    {
        public string regionId { get; set; }
        public string tagName { get; set; }
        public DateTime created { get; set; }
        public string tagId { get; set; }
        public double left { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class Image
    {
        public string id { get; set; }
        public DateTime created { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object imageUri { get; set; }
        public object thumbnailUri { get; set; }
        public List<Tag> tags { get; set; }
        public List<Region> regions { get; set; }
    }

    public class ModelInfo
    {
        public List<Image> Images { get; set; }
    }

}
