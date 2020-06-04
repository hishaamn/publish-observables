define(["sitecore", "jquery"], function (sitecore, $) {
    var app = sitecore.Definitions.App.extend({
        initialized: function () {
            this.getItemList();
            
            this.PublishButton.on("click", this.openPanel, this);
            this.RefreshButton.on("click", this.getItemList, this);
        },
        openPanel: function () {
            var app = this;

            app.CloseBtn.on("click", this.closePanel, this);
            app.TriggerPublishBtn.on("click", this.triggerPublish, this);

            $.ajax({
                type: "GET",
                dataType: "json",
                url: "/api/publishing/getpublishtargets",
                cache: false,
                success: function (data) {
                    app.PublishTargetList.set("items", data.Databases);
                },
                error: function () {
                    console.log("There was an error. Try again please!");
                },
                complete: function () {
                    app.SmartPanel.set("isOpen", true);
                }
            });
        },
        closePanel: function () {
            var app = this;

            app.SmartPanel.set("isOpen", false);
        },
        getItemList: function () {
            var app = this;

            app.ProgressIndicator.set("isBusy", true);
            app.ProgressIndicator.set("IsVisible", true);

            $.ajax({
                type: "GET",
                dataType: "json",
                url: "/api/publishing/getitems",
                cache: false,
                success: function (data) {
                    app.ItemList.set("items", data.PublishModels);
                },
                error: function () {
                    console.log("There was an error. Try again please!");
                },
                complete: function () {
                    app.ProgressIndicator.set("isBusy", false);
                    app.ProgressIndicator.set("IsVisible", false);
                }
            });
        },
        triggerPublish: function () {
            var app = this;

            $.ajax({
                type: "POST",
                dataType: "json",
                data: { selectedItems: app.ItemList.attributes.checkedItems, publishingTargets: app.PublishTargetList.attributes.checkedItems },
                url: "/api/publishing/triggerpublish",
                cache: false,
                success: function (data) {
                    if (data === "success") {
                        alert("Publish Triggered");
                    } else {
                        alert(data);
                    }
                },
                error: function () {
                    console.log("There was an error. Try again please!");
                },
                complete: function () {
                    app.ProgressIndicator.set("isBusy", false);
                    app.ProgressIndicator.set("IsVisible", false);
                }
            });
        }
    });
    return app;
});