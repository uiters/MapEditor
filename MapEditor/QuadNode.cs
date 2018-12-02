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
        private QuadNode leftBottom;
        private QuadNode rightBottom;
        private Rectangle region;
        private int level;

        public List<CObject> objects;
        public QuadNode(int id, int level, Rectangle rect)
        {
            this.id = id;
            this.level = level;
            region = rect;

            leftTop = null;
            rightTop = null;
            leftBottom = null;
            rightBottom = null;

            objects = new List<CObject>();
        }
        private Size GetCenter(Rectangle rect)
        {
            return new Size((rect.Width + rect.Left) / 2, (rect.Top + rect.Height) / 2);
        }

        private bool CreateQuadNodeSub()
        {
            Size center = GetCenter(region);
            if (center.Width < 200 || objects.Count < 2 || level >= CLevel) return false;
            else
            {
                this.leftTop = new QuadNode(id * 10 + 1, level + 1, new Rectangle(region.Location, center));
                this.rightTop = new QuadNode(id * 10 + 2, level + 1, new Rectangle(new Point(center.Width + region.X), center));
                this.leftBottom = new QuadNode(id * 10 + 3, level + 1, new Rectangle(new Point(region.X, region.Y + center.Height), center));
                this.rightBottom = new QuadNode(id * 10 + 4, level + 1, new Rectangle(new Point(region.X + center.Width, region.Y + center.Height), center));
                return true;
            }
        }
        private int count()
        {
            if (leftTop == null) return objects.Count;
            else return leftTop.count() + rightTop.count() + leftBottom.count() + rightBottom.count();
        }
        public void Build(CObject obj)
        {
            objects.Add(obj);
            if (leftBottom is null)
            {
                if (CreateQuadNodeSub() == false) return;
            }
            else
            {
                for (int i = 0; i < objects.Count; ++i)
                {
                    if (leftTop.region.IntersectsWith(objects[i].region))
                        leftTop.Build(objects[i]);

                    if (rightTop.region.IntersectsWith(objects[i].region))
                        rightTop.Build(objects[i]);

                    if (leftBottom.region.IntersectsWith(objects[i].region))
                        leftBottom.Build(objects[i]);

                    if (rightBottom.region.IntersectsWith(objects[i].region))
                        rightBottom.Build(objects[i]);
                }
                objects.Clear();
            }
        }

        public void Save(StreamWriter writer)
        {
            int nodeSub = leftTop is null ? 0 : 4;
            
            writer.Write(id + " " + region.Left + " " + region.Top + " " + region.Right + " " + region.Bottom + " " + nodeSub + " " + objects.Count + " ");
            writer.Flush();
            for (int i = 0; i < objects.Count; ++i)
            {
                writer.Write(objects[i].ID + " ");
            }
            writer.WriteLine();
            writer.Flush();

            if (leftTop != null)
            {
                leftTop.Save(writer);
                rightTop.Save(writer);
                leftBottom.Save(writer);
                rightBottom.Save(writer);
            }

        }
       
    }
}
