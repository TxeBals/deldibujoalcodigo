using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dibujo2Code.Core.Entities.BoxGeometry
{
    public enum ProjectionAxisEnum
    {
        X, Y
    }

    public class ProjectionRuler
    {
        public List<Section> Sections = new List<Section>();
    }
}
