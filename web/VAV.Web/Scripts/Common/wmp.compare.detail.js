/*

@author: Qing Sheng
@date: 2014.05.19

*/

(function() {
    (function ($) {

        $.fn.compareDetail = function (options) {
            var root = this;

            var opts = $.extend({}, $.fn.compareDetail.defaults, options);

            /* build html start */

            var nameSelect = $('<select class="largeSelect"></select>');
            var queryInput = $('<input type="button" value="' + WMP.frontTexts.Report_Query + '" />');
            var saveInput = $('<input type="button" value="' + WMP.frontTexts.Save + '" />');
            var deleteInput = $('<input id="twmpDelete" type="button" value="' + WMP.frontTexts.Delete + '" />');
            var filterInputs = $('<input type="radio" name="displayControl" style="margin-left:30px" checked="checked" value="show" />' + WMP.frontTexts.compareShowAll +
                '<input type="radio" name="displayControl" style="margin-left:10px" value="fadeout" />' + WMP.frontTexts.compareFadeOutTheSame +
                '<input type="radio" name="displayControl" style="margin-left:10px" value="hide" /><label >' + WMP.frontTexts.compareHideTheSame + '</label>');
            var addMoreInput = $('<input type="button" style="float: right" value="' + WMP.frontTexts.select + '" />');
            $('<div class="Statisticalfilter" style="height:auto;"></div>').append($('<label >' + WMP.frontTexts.name + '</label>'))
            .append(nameSelect).append(queryInput).append(saveInput).append(deleteInput).append(filterInputs).append(addMoreInput)
            .appendTo($('<div class="colorBlock" style="min-width:765px;"></div>')).appendTo(root);

            var exportExcelLink = $('<a href="#" class="exportExcel"><img alt="Export" src="/Content/themes/base/images/excel_icon.png" /></a>');
            var table = $('<table class="datatable Zebra compareColumnWidth compareTable" style="cursor: pointer;"><thead><tr style="white-space: normal"></tr></thead><tbody></tbody></table>');

            $('<div class="colorBlock" style="height: 91%; height: -webkit-calc(100% - 52px);min-width:765px;"></div>')
            .append($('<div class="colorBlock-header"><span class="colorBlock-header-span">' + WMP.frontTexts.compareResult + '</span></div>')
                    .append(exportExcelLink))
            .append($('<div class="ui-layout-pane" style="height: 90%; height: -webkit-calc(100% - 28px);min-width: 328px; overflow: auto">' +
                        '<div>' + WMP.frontTexts.Source + '</div></div>')
                    .prepend(table))
            .appendTo(root);

            var newNameDlg = $('<div style="display: none" title="' + WMP.frontTexts.Save + '">' +
                    '<table style="width:250px;margin-top: 10px"><tbody>' +
                    '<tr><td style="width:100px;font-size:12px">' + WMP.frontTexts.newName + '</td>' +
                        '<td><input style="width:150px" type="text" /></td></tr>' +
                    '<tr><td colspan="2">&nbsp;</td></tr>' +
                    '<tr><td></td><td>' +
                    '</td></tr>' +
                '</tbody></table</div>');
            var newNameDlgCancel = $('<input style="float: right" type="button" value="' + WMP.frontTexts.Cancel + '" />');
            var newNameDlgOkBtn = $('<input style="float: right" type="button" value="' + WMP.frontTexts.OK + '" />');
            $("td:eq(4)", newNameDlg).append(newNameDlgCancel).append(newNameDlgOkBtn);
            newNameDlg.appendTo(root.parent());

            var deleteDlg = $('<div style="display: none" title="' + WMP.frontTexts.DeleteBoxTitle + '"><div></div></div>');
            deleteDlg.appendTo(root.parent());


            /* build html end */



            /* event handlers start */

            queryInput.click(function() {
                querySelectedCompareItem();
            });

            $('input[name=displayControl]:radio', root).change(function () {
                filterBankWmpCompareTable($(this).val(), $('tbody tr', table));
            });

            saveInput.click(function () {
                $('input:text', newNameDlg).val('');
                newNameDlg.dialog('open');
            });

            newNameDlgCancel.click(function() {
                newNameDlg.dialog("close");
            });

            newNameDlgOkBtn.click(function() {
                var key = $('input:text', newNameDlg).val();
                if (key) {
                    var items = getCurrentCompareItems().map(function (x) { return x.inner_code; });
                    var compareDict = getUserSavedCompareItems();
                    compareDict[key] = items;
                    localStorage["Dict" + opts.compareId] = JSON.stringify(compareDict);
                    updateUserCompare();
                    nameSelect.val(key);
                }
                newNameDlg.dialog('close');
            });

            deleteInput.click(function() {
                var removedKey = nameSelect.val();
                if (removedKey) {
                    var localW = WMP.frontTexts.DeleteConfirmMsg.replace('{0}', removedKey);
                    deleteDlg.find("div").html(localW);
                    deleteDlg.dialog('open');
                }
            });

            addMoreInput.click(function() {
                opts.addMoreFun(getCurrentCompareItems());
            })

            exportExcelLink.click(function() {
                opts.exportFun(exportExcelLink, getCurrentCompareItems().map(function (x) { return x.inner_code; }));
            });

            /* event handlers end */


            buildWmpCompareTable(opts.products);
            updateUserCompare();

            newNameDlg.dialog(
            {
                autoOpen: false,
                width: 270,
                height: 150,
                modal: true
            });

            deleteDlg.dialog(
            {
                autoOpen: false,
                width: 320,
                height: 120,
                modal: true,
                buttons: [
                    {
                        text: WMP.frontTexts.OK,
                        click: function() {
                            deleteTrustCompareItem(nameSelect.val());
                            $(this).dialog("close");
                        }
                    },
                    {
                        text: WMP.frontTexts.Cancel,
                        click: function() {
                            $(this).dialog("close");
                        }
                    }
                ]
            });


            function buildWmpCompareTable (data) {
                $.template("colTemp", '<th><span data-sort="${Sort}">${Name}</span></th>');
                $('thead tr',table).empty();
                $.tmpl("colTemp", data.ColumTemplate).appendTo("thead tr", table);
                var tpl = '<tr>';
                $.each(data.ColumTemplate, function (key, value) {
                    tpl = tpl + '<td >${' + value.ColumnName + '}</td>';
                });
                tpl = tpl + '</tr>';
                $.template("rowTemplate", tpl);
                $('tbody', table).empty(); 
                $.tmpl("rowTemplate", data.RowData).appendTo($('tbody', table)); 
                $('thead tr th:gt(0)', table).append('<ins class="iconbox ui-iconBg ui-icon-pop-close" style="float:right;display:block"></ins>');
                $('thead tr th:gt(0) span', table).addClass('OpenLink').click(function (e) {
                    var t = $(e.target);
                    opts.itemClickHandler(t.data('sort'), t.text());
                });
                $('thead tr th ins', table).click(function () {
                    debugger;
                    var index = $('thead tr th ins', table).index(this) + 1;
                    $('thead tr th:eq(' + index + ')', table).remove();
                    $('tbody tr', table).each(function () {
                        $(this).find('td:eq(' + index + ')').remove();
                    });
                }); 
            }

            function filterBankWmpCompareTable  (arg, rows) {
                $.each(rows, function (idex, value) {
                    var cRow = $(value);
                    if (arg == 'show') {
                        cRow.show();
                        modifyDifferentStyle(false, cRow);
                        return;
                    }
                    if (arg == 'fadeout') {
                        cRow.show();
                    }
                    var cols = cRow.find('td:gt(0)');
                    if (cols.lenght < 2) return;
                    if (isColumnIdentical(cols)) {
                        if (arg == 'fadeout') {
                            modifyDifferentStyle(true, cRow);
                        } else if (arg == 'hide') {
                            cRow.hide();
                        }
                    }
                });
            }

            function isColumnIdentical(cols) {
                for (var i = 0; i < cols.length - 1; i++) {
                    var a = $(cols[i]).html();
                    var b = $(cols[i + 1]).html();
                    if (a != b) {
                        return false;
                    }
                }
                return true;
            }

            function modifyDifferentStyle(isAdd, row) {
                var tds = row.find('td:gt(0)');
                if (isAdd) {
                    $.each(tds, function (x, y) {
                        $(y).addClass('comparediff');
                    });
                } else {
                    $.each(tds, function (x, y) {
                        $(y).removeClass('comparediff');
                    });
                }
            }

            function getCurrentCompareItems() {
                var items = [];
                $('thead tr th:gt(0) span',table).each(function (index, item) {
                    var $span = $(item);
                    items.push({ inner_code: $span.data('sort'), prd_name: $span.text() });
                });
                return items;
            }

            function getUserSavedCompareItems() {
                var ret = {};
                try {
                    ret = JSON.parse(localStorage["Dict" + opts.compareId]);
                }
                catch (e) {
                }
                return ret;
            }

            function updateUserCompare () {
                var compareDict = getUserSavedCompareItems();
                nameSelect.html('');
                for (var key in compareDict) {
                    if (compareDict.hasOwnProperty(key)) {
                        nameSelect.append($('<option>', { value: key, text: key }));
                    }
                }
            }

            function querySelectedCompareItem () {
                var selectedKey = nameSelect.val();
                if (selectedKey) {
                    var compareDict = getUserSavedCompareItems();
                    if (compareDict) {
                        var itemList = compareDict[selectedKey] && compareDict[selectedKey].join(',');
                        AjaxWithBlock(root, opts.queryUrl, { ids: itemList }, function (data) {
                            buildWmpCompareTable(data);
                            filterBankWmpCompareTable($('input[name=displayControl]:checked', root).val(), $('tbody tr', table));
                        });
                    }
                }
            }

            function deleteTrustCompareItem (removedKey) {
                var compareDict = getUserSavedCompareItems();
                delete compareDict[removedKey];
                localStorage["Dict" + opts.compareId] = JSON.stringify(compareDict);
                updateUserCompare();
            }


        };

        $.fn.compareDetail.defaults = {
            
        };

    }(jQuery));
})();