using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zerex.Modules.Publish.Models
{
    public class Response
    {
        public List<PublishItemModel> PublishModels { get; set; }

        public List<string> ConfiguredLanguages { get; set; }
    }
}