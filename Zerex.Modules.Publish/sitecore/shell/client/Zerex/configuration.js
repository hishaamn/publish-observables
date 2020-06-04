define(["sitecore", "jquery"], function (sitecore, $) {
	var app = sitecore.Definitions.App.extend({
		initialized: function () {
			this.getConfiguredObservables();
			this.getConfiguredDatabases();
			this.getConfiguredWorkflows();
			
			this.ObservableSaveBtn.on("click", this.triggerSaveObservable, this);
			this.SaveDatabaseBtn.on("click", this.triggerSaveDatabase, this);
			this.SaveWorkflowBtn.on("click", this.triggerSaveWorkflow, this);
		},
		getConfiguredObservables: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				url: "/api/publishing/configuration/observable/get",
				cache: false,
				success: function (data) {
					app.ObservableList.set("items", data.Observables);		
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		},
		getConfiguredWorkflows: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				url: "/api/publishing/configuration/workflow/get",
				cache: false,
				success: function (data) {
					app.WorkflowList.set("items", data.Workflows);		
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		},
		getConfiguredDatabases: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				url: "/api/publishing/configuration/database/get",
				cache: false,
				success: function (data) {
					app.DatabaseList.set("items", data.Databases);		
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		},
		triggerSaveObservable: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);

			var selectedItems = app.ItemTreeView.attributes.checkedItemIds;
			
			$.ajax({
				type: "post",
				datatype: "json",
				data: { selectedPaths: selectedItems },
				url: "/api/publishing/configuration/observable/save",
				cache: false,
				success: function (data) {
					alert("publish triggered");				
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		},
		triggerSaveDatabase: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);
					
			var sourceDb = app.SourceDbTextBox.attributes.text;
			var targetDb = app.TargetDbTextBox.attributes.text;
			
			$.ajax({
				type: "post",
				datatype: "json",
				data: { sourceDatabase: sourceDb, targetDatabase: targetDb },
				url: "/api/publishing/configuration/database/save",
				cache: false,
				success: function (data) {
					alert("publish triggered");				
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		},
		triggerSaveWorkflow: function () {
			var app = this;
			
			app.ObservableProgressIndicator.set("isbusy", true);
			app.ObservableProgressIndicator.set("isvisible", true);
	
			var selectedItems = app.WorkflowContentTree.attributes.checkedItemIds;
			
			$.ajax({
				type: "post",
				datatype: "json",
				data: { selectedWorkflows: selectedItems },
				url: "/api/publishing/configuration/workflow/save",
				cache: false,
				success: function (data) {
					alert("publish triggered");				
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.ObservableProgressIndicator.set("isbusy", false);
					app.ObservableProgressIndicator.set("isvisible", false);
				}
			});
		}
	});
    return app;
});