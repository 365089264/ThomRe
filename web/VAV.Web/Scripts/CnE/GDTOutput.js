var Output = (function (my) {
    var lastOrder = " [KeyWord],[LatestDate]  desc";
    var bytCurrentPage = 1;
    my.arrHighCharts = {};
    my.TickedCache = {};
    my.UpdateOutputChartData = function(seriesData, contentId) {
        debugger;
        var cachedItem = my.getFirstProductData(contentId);
        var unit = cachedItem ? cachedItem.unit : null;
        var type = $('#outputTopChartddlId' + contentId).val();
        $('#outputChartContainer' + contentId).highcharts('StockChart', {
            rangeSelector: {
                buttons:[{
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
                inputEnabled: $('#outputChartContainer' + contentId).width() > 480,
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
                    var s = Highcharts.dateFormat('%Y-%m', this.x) ;
                    $.each(this.points, function(i, point) {
					s += '<br/><span style="color:'+point.series.color+'">' + point.series.name + '</span>: ' + Highcharts.numberFormat(point.y, 2);
				    });
                    return s;
                }
		    }
        });
        var chart = $('#outputChartContainer' + contentId).highcharts();
        for (var i = 0; i < chart.series.length; i++) {
            chart.series[i].update({
                type: type
            });
        }
        my.arrHighCharts[contentId] = chart;
    };
    my.getFirstProductData = function(contentid) {
        debugger;
        var keyDict = my.TickedCache[contentid];
        var key = Object.keys(keyDict)[0];
        return keyDict[key];
    };
    my.UpdateOutputTableData = function (data, contentId, isPaging) {
        var itemId = contentId.split('_')[1];
        var id = contentId.split('_')[0];
        bytCurrentPage = data.CurrentPage;
        if ($('#outputPageDetailTable' + contentId + ' thead tr th').length == 0) {
            $.template("colTemp", '<th  class="sortColumn" tag="${Sort}" onclick="Output.SortOutputDataColumn(\'' + contentId + '\',this, \'${ColumnName}\')">${Name}<span></span></th>');
            $('#outputPageDetailTable' + contentId + ' thead tr').empty();
            $("#outputPageDetailTable" + contentId + " thead tr").append('<th></th>');
            $.tmpl("colTemp", data.ColumTemplate).appendTo("#outputPageDetailTable" + contentId + " thead tr");
        }
        var markup = my.BuildRowTemplate(data.ColumTemplate);
        $.template("rowTemplate", markup);
        $('#outputPageDetailTable' + contentId + ' tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#outputPageDetailTable" + contentId + " tbody");
        if (isPaging) {
            var keyDict = my.TickedCache[contentId];
            var keys = Object.keys(keyDict);
            $.each(keys, function (i, v) {
                var row = $('#outputPageDetailTable' + contentId).find('tr[data-key="' + v + '"]');
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
        $('#outputPagePaginateOfLabel' + contentId).html(start + '-' + end + ' of ' + data.Total);
        if (data.Total == 0) {
            $('#outputPagePaginateOfLabel' + contentId + ',#outputPagePaginate' + contentId).hide();
            return;
        } else {
            $('#outputPagePaginateOfLabel' + contentId + ',#outputPagePaginate' + contentId).show();
        }
        $('#outputPagePaginate' + contentId).paginate({
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
                AjaxWithBlock($('#outputPageDetailTable' + contentId), '/GDT/GetOutputDetail', {
                    currentPage: page,
                    itemId: itemId,
                    id: id,
                    category: Output.getFilterString("outputPage" + contentId),
                    order: lastOrder
                }, function (mydata) { my.UpdateOutputTableData(mydata, contentId, true); });
            },
            firstText: my.paginateCommon.firstText,
            lastText: my.paginateCommon.lastText
        });
    };
    function getLegend(contentid) {
        var cols = $('#outputPageDetailTable' + contentid).data('legend').toString().split(',');
        return cols;
    }

    function getLegendText(contentid, key) {
        var colsArray = getLegend(contentid);
        var cols = colsArray.slice(0, colsArray.length - 1);
        var row = $('#outputPageDetailTable' + contentid).find('tr[data-key="' + key + '"]');
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
        debugger;
        if (btn.hasClass('untickedChart')) {
            if (Object.keys(keyDict).length >= 5) {
                alert(Output.MaxSelectionText);
                return;
            }
            var unitIndex = getLegend(contentid).pop();
            var currentUnit = row.find('td').eq(unitIndex).text();
            if (Object.keys(keyDict).length > 0) {
                var firstUnit = my.getFirstProductData(contentid).unit;
                if (firstUnit != currentUnit) {
                    alert(Output.MustSameUnit);
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
            AjaxWithBlock($('#outputPage' + contentid), '/GDT/GetOutputChartDetailList',
            { itemId: father.data('item'), keyList: keylist },
            function (data) {
                for (var i = 0; i < data.length; i++) {
                    var ckey = data[i].name;
                    var legendItem = my.TickedCache[contentid][ckey].legend;
                    data[i].name = legendItem;
                }
                my.UpdateOutputChartData(data, contentid);
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
    my.ExportExcelOutputDetails = function (link, contentId, reportId, itemId) {
        var filtes = Output.getFilterString("outputPage" + contentId);
        $(link).attr("href", '/GDT/ExportExcelForOutputDetail'
                            + '?reportId=' + reportId
                            + '&itemId=' + itemId
                            + '&category=' + filtes
                            + '&currentPage=' + bytCurrentPage + '&order=' + lastOrder
                    );
    };
    my.ExportExcelOutputChartDetails = function (link, reportId, itemId, contentId) {
        var keyDict = my.TickedCache[contentId];
        var key = Object.keys(keyDict)[0];
        $(link).attr("href", '/GDT/ExportExcelForOutputChartDetail' + '?reportId=' + reportId + '&itemId=' + itemId + '&key=' + key );
    };
    my.UpdateOutputData = function (contentId, reportId, containerId) {
        var filterStr = Output.getFilterString(contentId);
        AjaxWithBlock($('#outputPage' + containerId), '/GDT/GetOutputData', {
            id: reportId,
            itemId: containerId.split('_')[1],
            category: filterStr,
            order: lastOrder
        }, function (data) {
            Output.UpdateOutputTableData(data.table, containerId);
            Output.resetCache(containerId);
            Output.UpdateOutputChartData(data.chart.series, containerId);
        });
    };
    my.resetCache = function (contentId) {
        debugger;
        my.TickedCache[contentId] = {};
        var row = $('#outputPageDetailTable' + contentId + ' tbody tr:first()');
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
        var tpl = '<tr data-key=\'${KeyWord}\'><td><button class="untickedChart" onclick="Output.ToggleChart(this)"></button></td></td>';
      $.each(rawData, function (key, value) {
            if (value.ColumnType != 'decimal') {
                tpl = tpl + '<td style="text-align:left">${' + value.ColumnName + '}</td>';
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
                url: "/GDT/GetOutputChartDetail",
                dataType: "json",
                data: { itemId: contentId.split('_')[1], key: key, term: '5Y', reDate: my.getFirstProductData(contentId).date },
                success: function (data) {
                    my.UpdateOutputChartData(data.series, contentId);
                }
            });
        } else {
            my.UpdateOutputChartData([], contentId);
        }

    };
    my.UpdateOutputChartByTerm = function (contentId, term) {
        var item = my.getFirstProductData(contentId);
        debugger;
        $.ajax({
            type: "POST",
            url: "/GDT/GetOutputChartDetail",
            dataType: "json",
            data: { itemId: contentId.split('_')[1], key: item.key, term: term, reDate: item.date },
            success: function (data) {
                my.UpdateOutputChartData(data.series, contentId);
            }
        });
    };

    my.SortOutputDataColumn = function (contentId, th, order) {
        var direction = $(th).attr('tag');
        $("#outputPageDetailTable" + contentId + " .sortColumn").attr('tag', '');
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
        my.UpdateOutputDataBySort(contentId, order);
    };
    my.UpdateOutputDataBySort = function (contentId, order) {
        var filtes = Output.getFilterString("outputPage" + contentId);
        AjaxWithBlock($('#outputPageDetailTable' + contentId), '/GDT/GetOutputDetail', {
            currentPage: bytCurrentPage,
            id: contentId.split('_')[0],
            itemId: contentId.split('_')[1],
            category: filtes,
            order: order
        }, function (data) { my.UpdateOutputTableData(data, contentId, true); });
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
} (Output || {}));