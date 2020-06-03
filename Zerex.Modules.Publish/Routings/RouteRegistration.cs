using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Zerex.Modules.Publish.Routings
{
    public class RouteRegistration
    {
        public void Process(PipelineArgs args)
        {
            RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("GetPublishTargets", "api/publishing/getpublishtargets", new
            {
                action = "GetPublishTargets",
                controller = "Publish"
            });

            routes.MapRoute("GetConfiguredWorkflows", "api/publishing/configuration/workflow/get", new
            {
                action = "GetConfiguredWorkflows",
                controller = "Publish"
            });

            routes.MapRoute("GetConfiguredDatabases", "api/publishing/configuration/database/get", new
            {
                action = "GetConfiguredDatabases",
                controller = "Publish"
            });

            routes.MapRoute("GetConfiguredObservables", "api/publishing/configuration/observable/get", new
            {
                action = "GetConfiguredObservables",
                controller = "Publish"
            });

            routes.MapRoute("SaveWorkflow", "api/publishing/configuration/workflow/save", new
            {
                action = "SaveWorkflow",
                controller = "Publish"
            });

            routes.MapRoute("SaveDatabase", "api/publishing/configuration/database/save", new
            {
                action = "SaveDatabase",
                controller = "Publish"
            });

            routes.MapRoute("SaveObservables", "api/publishing/configuration/observable/save", new
            {
                action = "SaveObservables",
                controller = "Publish"
            });

            routes.MapRoute("TriggerPublish", "api/publishing/triggerpublish", new
            {
                action = "TriggerPublish",
                controller = "Publish"
            });

            routes.MapRoute("ItemList", "api/publishing/getitems", new
            {
                action = "GetItems",
                controller = "Publish"
            });
        }
    }
}