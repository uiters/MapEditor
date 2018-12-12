using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MapEditor
{
    class CObject
    {
        private int id;
        public Rectangle region;
        public Color color;
        public int type;
        public int idName;
        public bool direction;
        public int ID { get => id; }

        public CObject(int id, Rectangle region, Color color, int idName, int type, bool direction)
        {
            this.id = id;
            this.region = region;
            this.color = color;
            this.idName = idName;
            this.type = type;
            this.direction = direction;
        }
    }
}
