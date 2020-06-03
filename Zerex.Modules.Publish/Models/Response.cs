using System.Collections.Generic;

namespace Zerex.Modules.Publish.Models
{
    public class Response
    {
        public List<PublishItemModel> PublishModels { get; set; }

        public List<string> ConfiguredLanguages { get; set; }

        public List<Observable> Observables { get; set; }

        public List<Database> Databases { get; set; }

        public List<Workflow> Workflows { get; set; }
    }
}