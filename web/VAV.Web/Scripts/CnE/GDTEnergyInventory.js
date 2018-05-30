var EnergyInventory = (function (my) {
    var lastOrder = " [LatestDate] desc";
    my.arrHighCharts = {};
    my.UpdateInventoryChartData = function (seriesData, contentId) {
        debugger;
        var type = $('#inventoryTopChartddlId' + contentId).val();
        $('#inventoryChartContainer' + contentId).highcharts('StockChart', {
            rangeSelector: {
                buttons: [],
                inputDateFormat: '%Y-%m-%d',
                inputEditDateFormat: '%Y-%m-%d',
                inputEnabled: $('#inventoryChartContainer' + contentId).width() > 480,
                selected: 1
            },
            chart: {
                zoomType: 'x'
            },
            title: { text: my.chartCommon.title },
            yAxis: { title: { text: my.chartCommon.ytitle + " ( " + my.unit + " )"} },
            series: seriesData,
            legend: {
                enabled: true,
                align: 'left',
                verticalAlign: 'top',
                x: 10,
                y: 60,
                layout: 'vertical',
                floating: true
            },
            tooltip: {
                formatter: function () {
                    var s = Highcharts.dateFormat('%Y-%m-%d', this.x);
                    $.each(this.points, function (i, point) {
                        s += '<br/><span style="color:' + point.series.color + '">' + point.series.name + '</span>: ' + Highcharts.numberFormat(point.y, 2);
                    });
                    return s;
                }
            }
        });
        var chart = $('#inventoryChartContainer' + contentId).highcharts();
        for (var i = 0; i < chart.series.length; i++) {
            chart.series[i].update({
                type: type
            });
        }
        my.arrHighCharts[contentId] = chart;
    };
    my.UpdateInventoryTableData = function (data, contentId) {
        if ($('#inventoryPageDetailTable' + contentId + ' thead tr th').length == 0) {
            $.template("colTemp", '<th>${Name}</th>');
            $('#inventoryPageDetailTable' + contentId + ' thead tr').empty();
            $.tmpl("colTemp", data.ColumTemplate).appendTo("#inventoryPageDetailTable" + contentId + " thead tr");
        }
        var markup = my.BuildRowTemplate(data.ColumTemplate);
        $.template("rowTemplate", markup);
        $('#inventoryPageDetailTable' + contentId + ' tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#inventoryPageDetailTable" + contentId + " tbody");
    };

    my.ExportExcelInventoryTable = function (link, contentId, reportId, itemId) {
        $(link).attr("href", '/GDT/ExportExcelForEnergyInventoryTable'
                            + '?reportId=' + reportId
                            + '&itemId=' + itemId
                            + '&order=' + lastOrder
                    );
    };
    my.ExportExcelInventoryDetails = function (link, contentId, reportId, itemId) {
        $(link).attr("href", '/GDT/ExportExcelForEnergyInventoryChart'
                            + '?reportId=' + reportId
                            + '&itemId=' + itemId
                    );
    };
    my.BuildRowTemplate = function (rawData) {
        var tpl = '<tr>';
        $.each(rawData, function (key, value) {
            if (value.ColumnType == 'decimal') {
                tpl = tpl + '<td style="text-align:right">${' + value.ColumnName + '}</td>';
            } else if (value.ColumnName == 're_date') {
                tpl = tpl + '<td class="date">${' + value.ColumnName + '}</td>';
            } else {
                tpl = tpl + '<td>${' + value.ColumnName + '}</td>';
            }
        });
        tpl = tpl + '</tr>';
        return tpl;
    };
    my.refreshTopChart = function (type, contentId) {
        var chart = my.arrHighCharts[contentId];
        for (var i = 0; i < chart.series.length; i++) {
            chart.series[i].update({
                type: type
            });
        }
    };
    return my;
} (EnergyInventory || {}));