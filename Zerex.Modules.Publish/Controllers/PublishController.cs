using Cognifide.PowerShell.Core.Host;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sitecore.Diagnostics;
using Sitecore.StringExtensions;
using Zerex.Modules.Publish.Models;
using Database = Zerex.Modules.Publish.Models.Database;

namespace Zerex.Modules.Publish.Controllers
{
    public class PublishController : Controller
    {
        [System.Web.Http.HttpGet]
        public ActionResult GetItems()
        {
            var database = Factory.GetDatabase("master");

            var itemList = new List<PublishItemModel>();

            var languages = database.GetLanguages().Select(s => s.Name).ToList();

            using (var scriptSession = ScriptSessionManager.NewSession("Default", true))
            {
                var speScriptItem = database.GetItem(new ID("{1FF81FD2-C98A-46C5-8FF1-08E273FB9523}"));

                var script = speScriptItem["Script"];

                if (!string.IsNullOrEmpty(script))
                {
                    var observablePaths = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Observables").Children;

                    var workflows = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Workflows").Children;

                    foreach (Item observablePath in observablePaths)
                    {
                        scriptSession.SetVariable("path", observablePath.Fields["Observe Path"].Value);

                        var resultList = scriptSession.ExecuteScriptPart(script, false);

                        foreach (var resultItem in resultList)
                        {
                            if (resultItem is Item item)
                            {
                                var flag = item.Fields["__Workflow state"].Value.IsNullOrEmpty();

                                if (!flag)
                                {
                                    foreach (Item workflow in workflows)
                                    {
                                        if (item.State.GetWorkflow().WorkflowID.Equals(workflow.Fields["Workflow Approve State"].Value))
                                        {
                                            flag = true;
                                        }
                                    }
                                }

                                if (flag)
                                {
                                    var model = new PublishItemModel
                                    {
                                        ItemId = item.ID.ToString(),
                                        Name = item.Name,
                                        ItemPath = item.Paths.FullPath,
                                        Language = item.Language.ToString()
                                    };

                                    itemList.Add(model);
                                }
                            }
                        }
                    }
                }
            }

            var responseModel = new Response
            {
                ConfiguredLanguages = languages,
                PublishModels = itemList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetConfiguredObservables()
        {
            var database = Factory.GetDatabase("master");

            var observables = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Observables").Children;

            var observableList = new List<Observable>();

            if (observables.Any())
            {
                foreach (Item observable in observables)
                {
                    var configuredPath = observable.Fields["Observe Path"].Value;

                    var item = database.GetItem(configuredPath);
                    
                    var status = item != null ? "Item is available" : "Item is no more available";

                    observableList.Add(new Observable
                    {
                        ItemId = observable.ID.ToString(),
                        SitecorePath = observable.Fields["Observe Path"].Value,
                        Status = status
                    });
                }
            }

            var responseModel = new Response
            {
                Observables = observableList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetConfiguredWorkflows()
        {
            var database = Factory.GetDatabase("master");

            var workflows = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Workflows").Children;

            var workflowList = new List<Workflow>();

            if (workflows.Any())
            {
                foreach (Item workflow in workflows)
                {
                    var configuredId = workflow.Fields["Workflow Approve State"].Value;

                    var item = database.GetItem(new ID(configuredId));

                    var status = item != null ? "Item is available" : "Item is no more available";

                    workflowList.Add(new Workflow
                    {
                        ItemId = workflow.ID.ToString(),
                        ApprovedStateId = workflow.Fields["Workflow Approve State"].Value,
                        StateName = workflow.Fields["Name"].Value,
                        Status = status
                    });
                }
            }

            var responseModel = new Response
            {
                Workflows = workflowList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetConfiguredDatabases()
        {
            var masterDatabase = Factory.GetDatabase("master");

            var databases = masterDatabase.GetItem("/sitecore/system/Modules/Zerex Publishing/Databases").Children;

            var databaseList = new List<Database>();

            if (databases.Any())
            {
                foreach (Item database in databases)
                {
                    databaseList.Add(new Database
                    {
                        ItemId = database.ID.ToString(),
                        SourceDatabase = database.Fields["Source"].Value,
                        TargetDatabase = database.Fields["Target"].Value,
                    });
                }
            }

            var responseModel = new Response
            {
                Databases = databaseList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetPublishTargets()
        {
            var masterDatabase = Factory.GetDatabase("master");

            var databases = masterDatabase.GetItem("/sitecore/system/Modules/Zerex Publishing/Databases").Children;

            var databaseList = new List<Database>();

            if (databases.Any())
            {
                foreach (Item database in databases)
                {
                    databaseList.Add(new Database
                    {
                        SourceDatabase = database.Fields["Source"].Value,
                        TargetDatabase = database.Fields["Target"].Value,
                    });
                }
            }

            var responseModel = new Response
            {
                Databases = databaseList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult TriggerPublish(List<PublishItemModel> selectedItems, List<Database> publishingTargets)
        {
            try
            {
                foreach (var publishingTarget in publishingTargets)
                {
                    var sourceDb = Factory.GetDatabase(publishingTarget.SourceDatabase);
                    var targetDb = Factory.GetDatabase(publishingTarget.TargetDatabase);

                    foreach (var publishItemModel in selectedItems)
                    {
                        var options = new PublishOptions(sourceDb, targetDb, PublishMode.SingleItem, Language.Parse(publishItemModel.Language), DateTime.Now)
                        {
                            Deep = true,
                            PublishRelatedItems = true
                        };

                        if (publishItemModel.ItemId != null)
                        {
                            options.RootItem = sourceDb.GetItem(new ID(publishItemModel.ItemId));
                        }

                        var publisher = new Publisher(options);

                        publisher.PublishAsync();
                    }
                }

                return new JsonResult {Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            }
            catch (Exception ex)
            {
                Log.Error($"[Zerex Publishing] Error in triggering publish. Message: {ex.Message}.\r\n StackTrace: {ex.StackTrace}.\r\n InnerException: {ex.InnerException}", this);

                return new JsonResult { Data = "Failed to trigger publish. See Sitecore logs", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [System.Web.Http.HttpPost]
        public ActionResult SaveObservables(string selectedPaths)
        {
            var ids = selectedPaths.Split('|');

            var database = Factory.GetDatabase("master");

            var observableItem = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Observables");

            var observableTemplate = database.GetTemplate(new ID("{EB672122-CA1C-4801-B1D7-1BCA88C924D3}"));

            foreach (var id in ids)
            {
                var item = database.GetItem(new ID(id));

                if (observableTemplate != null)
                {
                    using (new SecurityDisabler())
                    {
                        var newItem = observableItem?.Add(item.Name, observableTemplate);

                        if (newItem == null)
                        {
                            continue;
                        }

                        newItem.Editing.BeginEdit();
                        newItem.Fields["Observe Path"].Value = item.Paths.FullPath;
                        newItem.Editing.EndEdit();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult SaveDatabase(string sourceDatabase, string targetDatabase)
        {
            var database = Factory.GetDatabase("master");

            var databaseItem = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Databases");

            var databaseTemplate = database.GetTemplate(new ID("{63F7064A-176C-4897-B41E-A8E3E9A6CC7A}"));
            
            if (databaseTemplate != null)
            {
                using (new SecurityDisabler())
                {
                    var newItem = databaseItem?.Add($"{sourceDatabase} {targetDatabase}", databaseTemplate);

                    if (newItem != null)
                    {
                        newItem.Editing.BeginEdit();
                        newItem.Fields["Source"].Value = sourceDatabase;
                        newItem.Fields["Target"].Value = targetDatabase;
                        newItem.Editing.EndEdit();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult SaveWorkflow(string selectedWorkflows)
        {
            var ids = selectedWorkflows.Split('|');

            var database = Factory.GetDatabase("master");

            var workflowItem = database.GetItem("/sitecore/system/Modules/Zerex Publishing/Workflows");

            var workflowTemplate = database.GetTemplate(new ID("{03B417EF-E54B-43AF-976A-DEE30430F455}"));

            foreach (var id in ids)
            {
                var item = database.GetItem(new ID(id));

                if (workflowTemplate != null)
                {
                    using (new SecurityDisabler())
                    {
                        var newItem = workflowItem?.Add(item.Name, workflowTemplate);

                        if (newItem == null)
                        {
                            continue;
                        }

                        newItem.Editing.BeginEdit();
                        newItem.Fields["Name"].Value = item.Name;
                        newItem.Fields["Workflow Approve State"].Value = item.ID.ToString();
                        newItem.Editing.EndEdit();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult RemoveConfiguredObservables(List<Observable> selectedItems)
        {
            var database = Factory.GetDatabase("master");

            foreach (var selectedItem in selectedItems)
            {
                var item = database.GetItem(new ID(selectedItem.ItemId));

                if (item != null)
                {
                    using (new SecurityDisabler())
                    {
                        item.Recycle();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult RemoveConfiguredDatabases(List<Database> selectedItems)
        {
            var database = Factory.GetDatabase("master");

            foreach (var selectedItem in selectedItems)
            {
                var item = database.GetItem(new ID(selectedItem.ItemId));

                if (item != null)
                {
                    using (new SecurityDisabler())
                    {
                        item.Recycle();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult RemoveConfiguredWorkflows(List<Workflow> selectedItems)
        {
            var database = Factory.GetDatabase("master");

            foreach (var selectedItem in selectedItems)
            {
                var item = database.GetItem(new ID(selectedItem.ItemId));

                if (item != null)
                {
                    using (new SecurityDisabler())
                    {
                        item.Recycle();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
