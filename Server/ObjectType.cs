using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ObjectType
    {
        private string type;
        int width, height;

        public ObjectType(string type, int width, int height)
        {
            this.type = type;
            this.width = width;
            this.height = height;
        }

        public string GetObjType()
        {
            return type;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }
    }
}
