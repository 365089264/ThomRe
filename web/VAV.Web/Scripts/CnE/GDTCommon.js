var GDT = (function (my) {
    my.initial = function (containerID) {

        $("#" + containerID).tabs({
            cache: true,
            beforeLoad: function (event, ui) {
                ui.ajaxSettings.dataTypes = ['html'];
                ui.jqXHR.error(function () {
                    debugger;
                });
                if (ui.tab.data("loaded")) {
                    event.preventDefault();
                    return;
                }

                ui.jqXHR.success(function () {
                    ui.tab.data("loaded", true);
                });


                $("#" + containerID).block(
                {
                    message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
                    css: {
                        top: ($("#" + containerID).height() - 54) / 2 + 'px',
                        left: ($("#" + containerID).width() - 54) / 2 + 'px',
                        width: '54px',
                        height: '54px',
                        border: '0px',
                        overflow: 'visible'
                    },
                    overlayCSS: { backgroundColor: '#131313' }
                });
            },
            activate: function () {
                amplify.publish('RefreshFundamentalDetailChart', { source: "fundamentalDetailTab" });
                $("#" + containerID).unblock();
            }
        });


    }
    return my;

})(GDT || {})