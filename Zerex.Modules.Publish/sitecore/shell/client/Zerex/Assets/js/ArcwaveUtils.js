define(["sitecore", "jquery"], function (sitecore, $) {
	return {
		getParameterByName: function (name, url) {
            if (!url) {
                url = window.location.href;
            }
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
				results = regex.exec(url);

            if (!results) return null;

            if (!results[2]) return '';

            return decodeURIComponent(results[2].replace(/\+/g, " "));
        },
		activateNextButton: function(app){
			var textValue = app.ProcessorName.Value;
					
			if(textValue != ""){
				return true;
			} else{
				return false;
			}
		},
		changeCursor: function(id, controlClass, cursor){
			$(id).find(controlClass).attr("style", "cursor:" + cursor);
		}
	};
});
