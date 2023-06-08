using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class NestedResult
    {
        public NestedResult()
        {
        }

        public NestedResult(object parent, object children)
        {
            this.Parent = parent;
            this.Children = children;
        }

        public object Parent { get; set; }

        public object Children { get; set; }
    }
}
