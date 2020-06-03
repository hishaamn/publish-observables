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
using Sitecore.StringExtensions;
using Zerex.Modules.Publish.Models;

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
                var speScriptItem = Sitecore.Context.Database.GetItem(new ID("{1FF81FD2-C98A-46C5-8FF1-08E273FB9523}"));

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

        [System.Web.Http.HttpPost]
        public ActionResult TriggerPublish(List<PublishItemModel> selectedItems)
        {
            var masterDatabase = Factory.GetDatabase("master");

            var sourceTargetDatabase = masterDatabase.GetItem("/sitecore/system/Modules/Zerex Publishing/Databases").Children.FirstOrDefault();

            var sourceDb = Factory.GetDatabase(sourceTargetDatabase?.Fields["Source"].Value);
            var targetDb = Factory.GetDatabase(sourceTargetDatabase?.Fields["Target"].Value);

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

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
    }
}
