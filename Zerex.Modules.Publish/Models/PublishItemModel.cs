using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zerex.Modules.Publish.Models
{
    public class PublishItemModel
    {
        public string Name { get; set; }

        public string Language { get; set; }

        public string ItemPath { get; set; }

        public string ItemId { get; set; }

        public string WorkflowState { get; set; }
    }
}