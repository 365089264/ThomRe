var WMP = (function (my) {


    my.BuildBankWMPCompareTable = function (data) {
        $.template("colTemp", '<th><span data-sort="${Sort}">${Name}</span></th>');
        $('#bwmpcTable thead tr').empty();
        $.tmpl("colTemp", data.ColumTemplate).appendTo("#bwmpcTable thead tr");
        var tpl = '<tr>';
        $.each(data.ColumTemplate, function (key, value) {
            tpl = tpl + '<td >${' + value.ColumnName + '}</td>';
        });
        tpl = tpl + '</tr>';
        $.template("rowTemplate", tpl);
        $('#bwmpcTable tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#bwmpcTable tbody");
        $('#bwmpcTable thead tr th:gt(0)').append('<ins class="iconbox ui-iconBg ui-icon-pop-close" style="float:right;display:block"></ins>');
        $('#bwmpcTable thead tr th:gt(0) span').addClass('OpenLink').click(function (e) {
            var t = $(e.target);
            WMP.OpenBankWMPDetail(t.data('sort'), t.text(), '/WMP/BankWMPDetail/');
        });
        $('#bwmpcTable thead tr th ins').click(function () {
            debugger;
            var index = $('#bwmpcTable thead tr th ins').index(this) + 1;
            $('#bwmpcTable thead tr th:eq(' + index + ')').remove();
            $('#bwmpcTable tbody tr').each(function () {
                $(this).find('td:eq(' + index + ')').remove();
            });
        });
    };

    my.BuildTrustWMPCompareTable = function (data) {
        $.template("colTemp", '<th><span data-sort="${Sort}">${Name}</span></th>');
        $('#twmpcTable thead tr').empty();
        $.tmpl("colTemp", data.ColumTemplate).appendTo("#twmpcTable thead tr");
        var tpl = '<tr>';
        $.each(data.ColumTemplate, function (key, value) {
            tpl = tpl + '<td >${' + value.ColumnName + '}</td>';
        });
        tpl = tpl + '</tr>';
        $.template("rowTemplate", tpl);
        $('#twmpcTable tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#twmpcTable tbody");
        $('#twmpcTable thead tr th:gt(0)').append('<ins class="iconbox ui-iconBg ui-icon-pop-close" style="float:right;display:block"></ins>');
        $('#twmpcTable thead tr th:gt(0) span').addClass('OpenLink').click(function (e) {
            var t = $(e.target);
            WMP.OpenBankWMPDetail(t.data('sort'), t.text(), '/WMP/TrustWMPDetail/');
        });
        $('#twmpcTable thead tr th ins').click(function () {
            debugger;
            var index = $('#twmpcTable thead tr th ins').index(this) + 1;
            $('#twmpcTable thead tr th:eq(' + index + ')').remove();
            $('#twmpcTable tbody tr').each(function () {
                $(this).find('td:eq(' + index + ')').remove();
            });
        });
    };

    my.FilterBankWMPCompareTable = function (arg, rows) {
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
    };

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

    my.GetCurrentCompareItems = function () {
        var items = [];
        $('#bwmpcTable thead tr th:gt(0) span').each(function (index, item) {
            var $span = $(item);
            items.push({ inner_code: $span.data('sort'), prd_name: $span.text() });
        });
        return items;
    };
    my.GetCurrentTrustCompareItems = function () {
        var items = [];
        $('#twmpcTable thead tr th:gt(0) span').each(function (index, item) {
            var $span = $(item);
            items.push({ inner_code: $span.data('sort'), prd_name: $span.text() });
        });
        return items;
    };
    my.AddMoreBWMPItems = function () {
        var items = my.GetCurrentCompareItems();
        OpenReport(53, WMP.wmpcomparelanguage.FinancialProductsTitle, 'Financial Products', '', '', { products: JSON.stringify(items) });
        if (typeof my.cmp != "undefined") {
            my.cmp.updateSelectedProducts(items);
        }
    };

    my.AddMoreTrustWMPItems = function () {
        var items = my.GetCurrentTrustCompareItems();
        OpenReport(58, WMP.wmpcomparelanguage.TrustProductsTitle, 'Trust Products', '', '', { products: JSON.stringify(items) });
        if (typeof my.tcmp != "undefined") {
            my.tcmp.updateSelectedProducts(items);
        }
    };


    my.ExportExelForWMPCompare = function (link, reportName) {
        $(link).attr("href", "/WMP/ExportExelForWMPCompare" + "?ids=" + WMP.GetCurrentCompareItems().map(function (x) { return x.inner_code; }) + "&reportName=" + reportName);
    };

    my.ExportExelForTrustWMPCompare = function (link, reportName) {
        $(link).attr("href", "/WMP/ExportExelForTrustWMPCompare" + "?ids=" + WMP.GetCurrentTrustCompareItems().map(function (x) { return x.inner_code; }) + "&reportName=" + reportName);
    };

    my.updateUserCompare = function () {
        var compareDict = getUserSavedCompareItems();
        var select = $('#bwmpcFiles');
        select.html('');
        for (var key in compareDict) {
            if (compareDict.hasOwnProperty(key)) {
                select.append($('<option>', { value: key, text: key }));
            }
        }
    };

    my.updateUserTrustCompare = function () {
        var compareDict = getUserSavedTrustCompareItems();
        var select = $('#twmpcFiles');
        select.html('');
        for (var key in compareDict) {
            if (compareDict.hasOwnProperty(key)) {
                select.append($('<option>', { value: key, text: key }));
            }
        }
    };

    function getUserSavedCompareItems() {
        var ret = {};
        try {
            ret = JSON.parse(localStorage.bwmpDict);
        }
        catch (e) {
        }
        return ret;
    }

    function getUserSavedTrustCompareItems() {
        var ret = {};
        try {
            ret = JSON.parse(localStorage.twmpDict);
        }
        catch (e) {
        }
        return ret;
    }

    my.AddNewCompareItem = function () {
        var key = $('#wmbpNewNameInput').val();
        if (key) {
            var items = WMP.GetCurrentCompareItems().map(function (x) { return x.inner_code; });
            var compareDict = getUserSavedCompareItems();
            compareDict[key] = items;
            localStorage.bwmpDict = JSON.stringify(compareDict);
            my.updateUserCompare();
            $('#bwmpcFiles').val(key);
        }
        $('#wmbpNewName').dialog('close');
    };

    my.AddNewTrustCompareItem = function () {
        var key = $('#twmpNewNameInput').val();
        if (key) {
            var items = WMP.GetCurrentTrustCompareItems().map(function (x) { return x.inner_code; });
            var compareDict = getUserSavedTrustCompareItems();
            compareDict[key] = items;
            localStorage.twmpDict = JSON.stringify(compareDict);
            my.updateUserTrustCompare();
            $('#twmpcFiles').val(key);
        }
        $('#twmpNewName').dialog('close');
    };

    my.DeleteCompareItem = function (removedKey) {
        var compareDict = getUserSavedCompareItems();
        delete compareDict[removedKey];
        localStorage.bwmpDict = JSON.stringify(compareDict);
        my.updateUserCompare();
    };
    my.DeleteTrustCompareItem = function (removedKey) {
        var compareDict = getUserSavedTrustCompareItems();
        delete compareDict[removedKey];
        localStorage.twmpDict = JSON.stringify(compareDict);
        my.updateUserTrustCompare();
    };

    my.QuerySelectedCompareItem = function () {
        var selectedKey = $('#bwmpcFiles').val();
        if (selectedKey) {
            var compareDict = getUserSavedCompareItems();
            if (compareDict) {
                var itemList = compareDict[selectedKey] && compareDict[selectedKey].join(',');
                AjaxWithBlock($('#wmbpTableDiv').parent(), "WMP/GetBankWMPCompareData", { ids: itemList }, function (data) {
                    WMP.BuildBankWMPCompareTable(data);
                    WMP.FilterBankWMPCompareTable($('input[name="BWMPC"]:checked').val(), $('#bwmpcTable tbody tr'));
                });
            }
        }
    };
    my.QuerySelectedTrustCompareItem = function () {
        var selectedKey = $('#twmpcFiles').val();
        if (selectedKey) {
            var compareDict = getUserSavedTrustCompareItems();
            if (compareDict) {
                var itemList = compareDict[selectedKey] && compareDict[selectedKey].join(',');
                AjaxWithBlock($('#twmpTableDiv').parent(), "WMP/GetTrustWMPCompareData", { ids: itemList }, function (data) {
                    WMP.BuildTrustWMPCompareTable(data);
                    WMP.FilterBankWMPCompareTable($('input[name="BWMPC"]:checked').val(), $('#twmpcTable tbody tr'));
                });
            }
        }
    };

    return my;
} (WMP || {}));