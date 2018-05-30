var BalanceTable = (function (my) {
    my.BuildTable = function (data, contentId) {
        debugger;
        $('#balanceTable' + contentId + ' thead').empty();
        $.each(data.header, function (i, val) {
            debugger;
            var tr = "<tr class='hr'>";
            $.each(val, function (x, y) {
                debugger;
                tr += "<th colspan='" + y.Colspan + "'>" + y.ColName + "</th>";
            });
            tr += "</tr>";
            $(tr).appendTo($('#balanceTable' + contentId + ' thead'));
        });
        $('#balanceTable' + contentId + ' tbody').empty();
        var tbody = "";
        $.each(data.body, function (i, val) {
            debugger;
            var tpl = "<tr>";
            $.each(val, function (x, colVal) {
                tpl += "<td>" + colVal + "</td>";
            });
            tpl = tpl + '</tr>';
            tbody += tpl;
        });
        $(tbody).appendTo($('#balanceTable' + contentId + ' tbody'));
    };

    my.BuildQuery = function (data, contentId) {
        debugger;
        $('#balanceCategory' + contentId).empty();
        $.each(data.balanceCategory, function (i, val) {
            $("<option value='" + val.Code + "'>" + val.ItemName + "</option>").appendTo($("#balanceCategory" + contentId));
        });
        $('#balanceRegion' + contentId).empty();
        $.each(data.balanceRegion, function (i, val) {
            $("<option value='" + val.Code + "'>" + val.ItemName + "</option>").appendTo($("#balanceRegion" + contentId));
        });
    };

    my.BuildRegionQuery = function (contentId) {
        $('#balanceRegion' + contentId).empty();
        $.ajax({
            type: 'Get',
            url: '/GDT/GetBalanceRegionData',
            data: {
                category: $('#balanceCategory' + contentId).val(),
                reportID: contentId.substring(0, contentId.indexOf("_"))
            },
            success: function (mydata) {
                $.each(mydata.balanceRegion, function (i, val) {
                    $("<option value='" + val.Code + "'>" + val.ItemName + "</option>").appendTo($("#balanceRegion" + contentId));
                });
            }
        });
       
    };

    my.GetTable = function (contentId) {
        AjaxWithBlock($('#balanceTable' + contentId), '/GDT/GetBalanceTabletData', {
            category: $('#balanceCategory' + contentId).val(),
            region: $('#balanceRegion' + contentId).val(),
            reportID: contentId.substring(0, contentId.indexOf("_")),
            isQuery: true
        }, function (mydata) { my.BuildTable(mydata, contentId); });
    };

    my.ExportBalanceData = function (link, contentId) {
        $(link).attr("href", '/GDT/ExportBalanceTabletData?category=' + $("#balanceCategory" + contentId).val() + '&region=' + $("#balanceRegion" + contentId).val() + '&reportID=' + contentId.substring(0, contentId.indexOf("_")));
    };
    return my;
} (BalanceTable || {}));