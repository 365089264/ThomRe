var Inventory = (function (my) {
    var lastOrder = " [KeyWord],[LatestDate]  desc";
    var bytCurrentPage = 1;
    my.arrHighCharts = {};
    my.TickedCache = {};
    my.UpdateInventoryChartData = function(seriesData, contentId) {

        var cachedItem = my.getFirstProductData(contentId);
        var unit = cachedItem ? cachedItem.unit : null;
        var type = $('#inventoryTopChartddlId' + contentId).val();
        $('#inventoryChartContainer' + contentId).highcharts('StockChart', {
            rangeSelector: {
                buttons: [{
                    type: 'month',
                    count: 6,
                    text: '6m'
                }, {
                    type: 'year',
                    count: 1,
                    text: '1y'
                }, {
                    type: 'year',
                    count: 3,
                    text: '3y'
                }, {
                    type: 'year',
                    count: 5,
                    text: '5y'
                }, {
                    type: 'all',
                    text: 'All'
                }],
                inputDateFormat: '%Y-%m-%d',
                inputEditDateFormat: '%Y-%m-%d',
                inputEnabled: $('#inventoryChartContainer' + contentId).width() > 480,
                selected: 1
            },
            chart: {
	        zoomType: 'x'
	        },
            title: { text: my.chartCommon.title },
            yAxis: { title: { text: my.chartCommon.ytitle + " ( " + unit + " )" } },
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
                formatter: function() {
                    var s = Highcharts.dateFormat('%Y-%m-%d', this.x);
                    $.each(this.points, function(i, point) {
					s += '<br/><span style="color:'+point.series.color+'">' + point.series.name + '</span>: ' + Highcharts.numberFormat(point.y, 2);
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
    my.getFirstProductData = function(contentid) {
        
        var keyDict = my.TickedCache[contentid];
        var key = Object.keys(keyDict)[0];
        return keyDict[key];
    };
    my.UpdateInventoryTableData = function (data, contentId, isPaging) {
        var itemId = contentId.split('_')[1];
        var id = contentId.split('_')[0];
        bytCurrentPage = data.CurrentPage;
        if ($('#inventoryPageDetailTable' + contentId + ' thead tr th').length == 0) {
            $.template("colTemp", '<th  class="sortColumn" tag="${Sort}" onclick="Inventory.SortInventoryDataColumn(\'' + contentId + '\',this, \'${ColumnName}\')">${Name}<span></span></th>');
            $('#inventoryPageDetailTable' + contentId + ' thead tr').empty();
            $("#inventoryPageDetailTable" + contentId + " thead tr").append('<th></th>');
            $.tmpl("colTemp", data.ColumTemplate).appendTo("#inventoryPageDetailTable" + contentId + " thead tr");
        }
        var markup = my.BuildRowTemplate(data.ColumTemplate);
        $.template("rowTemplate", markup);
        $('#inventoryPageDetailTable' + contentId + ' tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#inventoryPageDetailTable" + contentId + " tbody");
        if (isPaging) {
            var keyDict = my.TickedCache[contentId];
            var keys = Object.keys(keyDict);
            $.each(keys, function (i, v) {
                var row = $('#inventoryPageDetailTable' + contentId).find('tr[data-key="' + v + '"]');
                if (row) {
                    var btn = row.find('button');
                    btn.removeClass('untickedChart');
                    btn.addClass('tickedChart');
                }
            });
        }
        var start = (data.CurrentPage - 1) * data.PageSize + 1;
        var end = data.CurrentPage * data.PageSize;
        if (end > data.Total) end = data.Total;
        $('#inventoryPagePaginateOfLabel' + contentId).html(start + '-' + end + ' of ' + data.Total);
        if (data.Total == 0) {
            $('#inventoryPagePaginateOfLabel' + contentId + ',#inventoryPagePaginate' + contentId).hide();
            return;
        } else {
            $('#inventoryPagePaginateOfLabel' + contentId + ',#inventoryPagePaginate' + contentId).show();
        }
        $('#inventoryPagePaginate' + contentId).paginate({
            count: Math.ceil(data.Total / data.PageSize),
            start: data.CurrentPage,
            display: 10,
            border: false,
            text_color: '#00B3E3',
            background_color: 'none',
            text_hover_color: '#28D2FF',
            background_hover_color: 'none',
            images: false,
            mouse: 'press',
            onChange: function (page) {
                AjaxWithBlock($('#inventoryPageDetailTable' + contentId), '/GDT/GetInventoryDetail', {
                    currentPage: page,
                    itemId: itemId,
                    id: id,
                    category: Inventory.getFilterString("inventoryPage" + contentId),
                    order: lastOrder
                }, function (mydata) { my.UpdateInventoryTableData(mydata, contentId, true); });
            },
            firstText: my.paginateCommon.firstText,
            lastText: my.paginateCommon.lastText
        });
    };
    function getLegend(contentid) {
        var cols = $('#inventoryPageDetailTable' + contentid).data('legend').toString().split(',');
        return cols;
    }

    function getLegendText(contentid, key) {
        var colsArray = getLegend(contentid);
        var cols = colsArray.slice(0, colsArray.length - 1);
        var row = $('#inventoryPageDetailTable' + contentid).find('tr[data-key="' + key + '"]');
        return cols.map(function (v) {
            return row.find('td').eq(v).text();
        }).join('-');
    }

    my.ToggleChart = function (obj) {
        var btn = $(obj);
        var row = btn.closest('tr');
        var key = row.data('key');
        var father = btn.closest('table');
        var contentid = father.data('report') + '_' + father.data('item');
        var keyDict = my.TickedCache[contentid];
        if (!keyDict) {
            keyDict = {};
            my.TickedCache[contentid] = keyDict;
        };
        
        if (btn.hasClass('untickedChart')) {
            if (Object.keys(keyDict).length >= 5) {
                alert(Inventory.MaxSelectionText);
                return;
            }
            var unitIndex = getLegend(contentid).pop();
            var currentUnit = row.find('td').eq(unitIndex).text();
            if (Object.keys(keyDict).length > 0) {
                var firstUnit = my.getFirstProductData(contentid).unit;
                if (firstUnit != currentUnit) {
                    alert(Inventory.MustSameUnit);
                    return;
                }
            }
            btn.removeClass('untickedChart');
            btn.addClass('tickedChart');
            var date = row.find('.date').text();
            keyDict[key] = { key: key, legend: getLegendText(contentid, key), unit: currentUnit, date: date };
        }
        else {
            btn.removeClass('tickedChart');
            btn.addClass('untickedChart');
            delete keyDict[key];
        }
        if (Object.keys(keyDict).length > 1) {
            changeChartExcelStatus(contentid,false);
            var keylist = Object.keys(keyDict).join(',');
            AjaxWithBlock($('#inventoryPage' + contentid), '/GDT/GetInventoryChartDetailList',
            { itemId: father.data('item'), keyList: keylist },
            function (data) {
                for (var i = 0; i < data.length; i++) {
                    var ckey = data[i].name;
                    var legendItem = my.TickedCache[contentid][ckey].legend;
                    data[i].name = legendItem;
                }
                my.UpdateInventoryChartData(data, contentid);
            });
        } else if (Object.keys(keyDict).length == 1) {
            key = Object.keys(keyDict)[0];
            my.RefreshSingleProductChart(key, contentid);
            changeChartExcelStatus(contentid,true);
        } else {
            my.RefreshSingleProductChart('', contentid);
            changeChartExcelStatus(contentid,false);
        }
    };
    
    function changeChartExcelStatus(contentid, status) {
        var excel = $('#excel' + contentid);
        if (status) {
            excel.show();
        } else {
            excel.hide();
        }
    };


    my.getFilterString = function (contentId) {
        var filterStr = '';
        var dataBaseFilter = '';
        $("#" + contentId + " .Statisticalfilter select").each(function () {
            var id = $(this).attr("id");
            filterStr += id + ",";
        });
        if (filterStr) {
            filterStr = filterStr.substring(0, filterStr.lastIndexOf(","));
            var filters = filterStr.split(',');
            for (var i = 0; i < filters.length; i++) {
                var columnName = $("#" + filters[i]).attr("columnName") + "_en";
                var value = $("#" + filters[i]).val();
                if (value) {
                    dataBaseFilter += columnName + "=N'" + value + "'";
                    for (var k = i + 1; k < filters.length; k++) {
                        if ($("#" + filters[k]).val()) {
                            dataBaseFilter += "  and   ";
                            break;
                        }
                    }
                }
            }
        }
        return dataBaseFilter;
    };
    my.ExportExcelInventoryDetails = function (link, contentId, reportId, itemId) {
        var filtes = Inventory.getFilterString("inventoryPage" + contentId);
        $(link).attr("href", '/GDT/ExportExcelForInventoryDetail'
                            + '?reportId=' + reportId
                            + '&itemId=' + itemId
                            + '&category=' + filtes
                            + '&currentPage=' + bytCurrentPage + '&order=' + lastOrder
                    );
    };
    my.ExportExcelInventoryChartDetails = function (link, reportId, itemId, contentId) {
        var keyDict = my.TickedCache[contentId];
        var key = Object.keys(keyDict)[0];
        $(link).attr("href", '/GDT/ExportExcelForInventoryChartDetail' + '?reportId=' + reportId + '&itemId=' + itemId + '&key=' + key );
    };
    my.UpdateInventoryData = function (contentId, reportId, containerId) {
        var filterStr = Inventory.getFilterString(contentId);
        AjaxWithBlock($('#inventoryPage' + containerId), '/GDT/GetInventoryData', {
            id: reportId,
            itemId: containerId.split('_')[1],
            category: filterStr,
            order: lastOrder
        }, function (data) {
            Inventory.UpdateInventoryTableData(data.table, containerId);
            Inventory.resetCache(containerId);
            Inventory.UpdateInventoryChartData(data.chart.series, containerId);
        });
    };
    my.resetCache = function (contentId) {
        
        my.TickedCache[contentId] = {};
        var row = $('#inventoryPageDetailTable' + contentId + ' tbody tr:first()');
        var key = row.data('key');
        var keyDict = my.TickedCache[contentId];
        var btn = row.find('button');
        if (btn.length == 0) {
            changeChartExcelStatus(contentId, false);
        } else {
            changeChartExcelStatus(contentId, true);
        }
        btn.removeClass('untickedChart');
        btn.addClass('tickedChart');
        var unitIndex = getLegend(contentId).pop();
        var currentUnit = row.find('td').eq(unitIndex).text();
        keyDict[key] = keyDict[key] = { key: key, legend: getLegendText(contentId, key), unit: currentUnit, date: row.find('.date').text() };
    };

    my.BuildRowTemplate = function (rawData) {
        var tpl = '<tr data-key=\'${KeyWord}\'><td><button class="untickedChart" onclick="Inventory.ToggleChart(this)"></button></td></td>';
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
    my.RefreshSingleProductChart = function (key, contentId) {
        if (key) {
            $.ajax({
                type: "POST",
                url: "/GDT/GetInventoryChartDetail",
                dataType: "json",
                data: { itemId: contentId.split('_')[1], key: key, term: '5Y', reDate: my.getFirstProductData(contentId).date },
                success: function (data) {
                    my.UpdateInventoryChartData(data.series, contentId);
                }
            });
        } else {
            my.UpdateInventoryChartData([], contentId);
        }

    };
    my.UpdateInventoryChartByTerm = function (contentId, term) {
        var item = my.getFirstProductData(contentId);
        
        $.ajax({
            type: "POST",
            url: "/GDT/GetInventoryChartDetail",
            dataType: "json",
            data: { itemId: contentId.split('_')[1], key: item.key, term: term, reDate: item.date },
            success: function (data) {
                my.UpdateInventoryChartData(data.series, contentId);
            }
        });
    };

    my.SortInventoryDataColumn = function (contentId, th, order) {
        var direction = $(th).attr('tag');
        $("#inventoryPageDetailTable" + contentId + " .sortColumn").attr('tag', '');
        switch (direction) {
            case "ASC":
                $(th).attr('tag', 'DESC');
                order = order + "  " + $(th).attr('tag') + "";
                break;
            case "DESC":
                $(th).attr('tag', '');
                order = " [KeyWord],[LatestDate]  desc";
                break;
            case "":
                $(th).attr('tag', 'ASC');
                order = order + " " + $(th).attr('tag') + "";
                break;
            default:
                break;
        }
        lastOrder = order;
        my.UpdateInventoryDataBySort(contentId, order);
    };
    my.UpdateInventoryDataBySort = function (contentId, order) {
        var filtes = Inventory.getFilterString("inventoryPage" + contentId);
        AjaxWithBlock($('#inventoryPageDetailTable' + contentId), '/GDT/GetInventoryDetail', {
            currentPage: bytCurrentPage,
            id: contentId.split('_')[0],
            itemId: contentId.split('_')[1],
            category: filtes,
            order: order
        }, function (data) { my.UpdateInventoryTableData(data, contentId, true); });
    };
    my.refreshTopChart = function(type, contentId) {
        var chart=my.arrHighCharts[contentId];
        for (var i = 0; i < chart.series.length; i++) {
            chart.series[i].update({
                type: type
            });
        }
    };
    return my;
} (Inventory || {}));