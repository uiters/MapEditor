using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MapEditor
{
    class QuadNode
    {
        private readonly int CLevel = 6;

        private readonly int id;
        private QuadNode leftTop;
        private QuadNode rightTop;
        private QuadNode LeftBottom;
        private QuadNode rightBottom;
        private Rectangle region;
        private int level;

        public List<int> objects;
        public QuadNode(int id, int level, Rectangle rect)
        {
            this.id = id;
            this.level = level;
            region = rect;

            leftTop = null;
            rightTop = null;
            LeftBottom = null;
            rightBottom = null;

            objects = new List<int>();
        }
        private Size GetCenter(Rectangle rect)
        {
            return new Size((rect.Width + rect.Left) / 2, (rect.Top + rect.Height) / 2);
        }

        private bool CreateQuadNodeSub()
        {
            Size center = GetCenter(region);
            if (center.Width < 200 || objects.Count == 0 || level >= CLevel) return false;

            leftTop = new QuadNode(id * 10 + 1,level + 1, new Rectangle(region.Location, center));
            rightTop = new QuadNode(id * 10 + 2, level + 1, new Rectangle(new Point(center.Width + region.Left), center));
            LeftBottom = new QuadNode(id * 10 + 3, level + 1, new Rectangle(new Point(region.Left, region.Top + center.Height), center));
            rightBottom = new QuadNode(id * 10 + 4, level + 1, new Rectangle(new Point(region.Left + center.Width, region.Top + center.Height), center));
            return true;
        }

        public void Build(int obj, Dictionary<int, ColorRegion> map)
        {
            objects.Add(obj);
            if (CreateQuadNodeSub() == false) return;
            for (int i = 0; i < objects.Count; ++i)
            {
                if (leftTop.region.IntersectsWith(map[objects[i]].region))
                    leftTop.Build(obj, map);

                if (rightTop.region.IntersectsWith(map[objects[i]].region))
                    rightTop.Build(obj, map);

                if (LeftBottom.region.IntersectsWith(map[objects[i]].region))
                    LeftBottom.Build(obj, map);

                if (rightBottom.region.IntersectsWith(map[objects[i]].region))
                    rightBottom.Build(obj, map);
            }
            objects.Clear(); 
        }

        public void Save(StreamWriter writer)
        {
            int nodeSub = leftTop is null ? 0 : 4;

            writer.Write(id + " " + region.Left + " " + region.Top + " " + region.Right + " " + region.Bottom + " " + nodeSub + " " + objects.Count + " ");
            writer.Flush();
            for (int i = 0; i < objects.Count; ++i)
            {
                writer.Write(objects[i] + " ");
            }
            writer.WriteLine();
            writer.Flush();

            if (nodeSub > 0)
            {
                leftTop.Save(writer);
                rightTop.Save(writer);
                LeftBottom.Save(writer);
                rightBottom.Save(writer);
            }

        }
       
    }
}
