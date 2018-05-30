

(function () {
    



    (function ($) {

        $.fn.wmpcompare = function (options) {

            var root = this;

            /*
            reportId:60,// report id to be opened
            tabName:"tab name", // tab name
            trace:'Bank Financial Products Compare' // trace name
            defaultItems:[{inner_code:107094538,prd_name:"tt"}] // default checked items
            itemClickHandler:function
            */

            var opts = $.extend({}, $.fn.wmpcompare.defaults, options);

            var selectedItems = opts.defaultItems;
            var hideBtn = $('<button class="ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only ui-dialog-titlebar-close" role="button" aria-disabled="false" title="close"><span class="ui-button-icon-primary ui-icon ui-icon-closethick"></span><span class="ui-button-text">close</span></button>');
            var floatBox = $('<div class="comparebox" style="display:none;"></div>').append($('<div class="toolbar toolbarPop" ><span style="width:90%;float:left;">' + opts.title + '</span></div>').append(hideBtn));
            hideBtn.click(function() {
                floatBox.hide();
            });
            var ul = $('<ul class="prodli"></ul>');
            var btnDiv = $('<div style="text-align:center;"></div>');
            var btnCmp = $('<input type="button" value="' + WMP.wmpcomparelanguage.compareButtonText + '"/>');
            btnCmp.click(function () {
                var ids = selectedItems.map(function (elem) {
                    return elem.inner_code;
                }).join(",");
                CloseReport(opts.reportId);
                OpenReport(opts.reportId, opts.tabName, opts.trace, '', '', { products: ids });
            });
            var btnClr = $('<input type="button" value="' + WMP.wmpcomparelanguage.clearText + '"/>');
            btnClr.click(function () {
                selectedItems.forEach(function (item) {
                    removeItem(item);
                });
            });

            var alertBox = $('<div style="display: none" title=' + WMP.wmpcomparelanguage.alertBoxTitle + '><br/><p>' + WMP.wmpcomparelanguage.maxSelectCountAlertText + '</p></div>');
            var alertBoxOkBtn = $('<input type="button" value="' + WMP.wmpcomparelanguage.alertBoxOkBtnText + '"></input>');
            alertBoxOkBtn.click(function() {
                alertBox.dialog("close");
            });
            $('<div style="text-align:center;"></div>').append(alertBoxOkBtn).appendTo(alertBox);
            alertBox.appendTo(root);
            
            floatBox.append($('<div class="PopEventContainer" ></div>').append($('<div class="colorBlock2" style="margin:8px 8px 8px 8px;padding:0 18px 18px 0;"></div>').append(ul).append(btnDiv.append(btnCmp).append(btnClr)))).appendTo(root);


            function addCheckboxHandler() {
                $("input:checkbox", root).unbind("change");
                $("input:checkbox", root).change(function () {
                    var checked = $(this).is(":checked");
                    var selectedItem = {
                        inner_code: $(this).data("inner-code"),
                        prd_name: $(this).data("prd-name")
                    };

                    if (checked) {
                        if (selectedItems.length >= 10) {
                            $(this).prop('checked', false);
                            alertBox.dialog({
                                modal: true
                            });
                            return;
                        }   
                        addItem(selectedItem);
                    } else {
                        removeItem(selectedItem);
                    }
                });
            }

            this.updatePage = function () {
                selectedItems.forEach(function (item) {
                    //check items after build table
                    if ($("input[data-inner-code='" + item.inner_code + "']").prop("checked") !== true) {
                        $("input[data-inner-code='" + item.inner_code + "']").prop("checked", true);
                    }
                    //check if compare box contains current item
                    if ($("li[data-inner-code='" + item.inner_code + "']", ul).length === 0) {
                        addliItem(item);
                    }
                });
                addCheckboxHandler();
            };
            this.updatePage();

            function addItem(item) {
                selectedItems.push(item);
                addliItem(item);
            }

            function addliItem(item) {
                var newItem = $('<li data-inner-code="' + item.inner_code + '"></li>')
                    .append($('<a href="javascript:void(0)">' + item.prd_name + '</a>')
                    .click(function() {
                            opts.itemClickHandler(item.inner_code, item.prd_name);
                        })
                    )
                    .append($('<ins class="iconbox ui-iconBg ui-icon-pop-close"  style="float:right;margin:-12px 4px 0 0;cursor: pointer;"></ins>')
                    .click(function () {
                        removeItem(item);
                    }))
                    .appendTo(ul);
                updateStyle();
            }

            function removeItem(item) {
                selectedItems = $.grep(selectedItems, function (n, i) {
                    return n.inner_code != item.inner_code;
                });
                $("li[data-inner-code='" + item.inner_code + "']", ul).remove();
                $("input[data-inner-code='" + item.inner_code + "']").prop('checked', false);
                updateStyle();
            }

            function updateStyle() {
                if (selectedItems.length == 0) {
                    floatBox.hide();
                }
                if (selectedItems.length > 0) {
                    floatBox.show();
                }
                var top = root.height() / 4;
                if (top < 97) {
                    top = 97;
                }
                floatBox.css("top", top);
                var ulheight = root.height() / 2 - 100;
                if (ulheight < 0) {
                    ulheight = 50;
                }
                ul.css("max-height", ulheight);
            }

            this.updateSelectedProducts = function (products) {
                selectedItems.forEach(function (item) {
                    removeItem(item);
                });
                selectedItems = products;
                this.updatePage();
            };

            return this;

        };

        $.fn.wmpcompare.defaults = {
            //nothing in defaults
            defaultItems: [],
            title:"",
            itemClickHandler:function() {
                alert("itemClickHandler not set.");
            }
        };

    } (jQuery));



})();