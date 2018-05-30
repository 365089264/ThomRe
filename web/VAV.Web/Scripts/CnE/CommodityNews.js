var CommodityNews = (function (my) {
    my.JustifyLayout = function (domID) {
        $('#' + domID).layout({
            south: {
                size: 325
            , closable: false
            }
        });
    }
    function UpdatePaginationLabel(current, pageSize, total, id, text) {
        var start = (current - 1) * pageSize + 1;
        var end = current * pageSize;
        if (end > total) end = total;
        $(id).html(start + '-' + end + text + total);
    }

    my.BuildReportTable = function (data, reportID) {
        var markup = '<tr onclick="CommodityNews.GetFirstRow(this,\'${NewsId}\')">'
                + '<td class="textLeft" style="display:none">${NewsId}</td>'
                + '<td class="textLeft">${NewsTitle}</td>'
                + '<td class="textLeft" style="border-left:solid 1px #363636;width:15%">${TimeString}</td>'
                + '</tr>';
        $.template("rowTemplate", markup);
        $('#tableId' + reportID + ' tbody').empty();
        $.tmpl("rowTemplate", data).appendTo("#tableId" + reportID + " tbody");
    };
    my.GetFirstRow = function (obj, rowID) {
        $("#newsTitle").text(($($(obj).children('td').get(1)).html()));
        $("#tableId22073 tr").removeClass("SelectedRow");
        $(obj).addClass('SelectedRow');
        AjaxWithBlock($('#newsContent').parent(), '/CNE/GetSingleNews',
            {
                newID: rowID

            }, function (data) {
                $("#newsMaster").html(data);
            });

    };
    my.UpdateReportData = function (currentPage, reportID, text, ftext, ltext) {
        var titleText = $("#title").val() == "'" ? "''" : $("#title").val();
        AjaxWithBlock($('#tableConent' + reportID).parent(), '/CNE/GetRSNews',
            {

                startDate: $("#startPicker").val(),
                endDate: $("#endPicker").val(),
                ntitle: titleText,
                pageNo: currentPage,
                isHTML: false
            }, function (data) {

                if (data.Total == 0) {
                    $("#pager" + reportID).hide();
                    $("#tableId" + reportID).hide();
                    $("#emptyMessage").show();
                    $("#newsMaster").html("");
                    $("#newsTitle").text("");
                }
                else {
                    $("#emptyMessage").hide();
                    $("#pager" + reportID).show();
                    $("#tableId" + reportID).show();
                    CommodityNews.BuildReportTable(data.Data, reportID);
                    CommodityNews.UpdateReportPagination(data, "#rsPaginate" + reportID, reportID, text, ftext, ltext);
                    var obj = $("#tableId" + reportID + " tbody tr").first();
                    var rowID = $(obj.children("td").eq(0)).text();
                    CommodityNews.GetFirstRow(obj, rowID);
                }
            });
    };


    my.UpdateReportPagination = function (data, id, reportID, text, ftext, ltext) {
        UpdatePaginationLabel(data.CurrentPage, 15, data.Total, "#rsPaginateLabel" + reportID, text);
        $(id).paginate({
            count: Math.ceil(data.Total / 15),
            start: data.CurrentPage,
            display: 15,
            border: false,
            text_color: '#00B3E3',
            background_color: 'none',
            text_hover_color: '#28D2FF',
            background_hover_color: 'none',
            images: false,
            mouse: 'press',
            onChange: function (page) {
                CommodityNews.UpdateReportData(page, reportID, text, ftext, ltext);
            },
            firstText: ftext,
            lastText: ltext
        });
    };


    return my;
} (CommodityNews || {}));