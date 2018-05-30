var Price = (function (my) {
    var lastOrder = " [orderKey],[re_date] desc";
    var bytCurrentPage = 1;
    my.arrHighCharts = {};
    my.TickedCache = {};
    my.UpdatePriceChartData = function (seriesData, contentId) {
        var cachedItem = getFirstProductData(contentId);
        var unit = cachedItem ? cachedItem.unit : null;
        var modData;
        if (seriesData.length > 1) {
            modData = seriesData;
        } else {
            modData = [
                {
                    name: (seriesData.length>0?seriesData[0].name:""),
                    type: 'line',
                    id: 'primary',
                    data: (seriesData.length > 0 ? seriesData[0].data : "")
                }, {
                    name: Price.SMA30,
                    linkedTo: 'primary',
                    showInLegend: true,
                    type: 'trendline',
                    algorithm: 'SMA',
                    periods: 30
                }
            ];
        }
        var chart = Highcharts.StockChart({
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
                inputEnabled: true,
                selected: 1
            },
            chart: {
                renderTo: 'priceChartContainer' + contentId,
                zoomType: 'x'
            },
            title: { text: my.chartCommon.title },
            yAxis: { title: { text: my.chartCommon.ytitle + " ( " + unit + " )"} },
            series: modData,
            legend: {
                align: 'left',
                verticalAlign: 'top',
                x: 0,
                y: 50,
                layout: 'vertical',
                floating: true,
                enabled: true
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
        my.arrHighCharts[contentId] = chart;
    };
    function getFirstProductData(contentid) {
        var keyDict = my.TickedCache[contentid];
        var key = Object.keys(keyDict)[0];
        return keyDict[key];
    }
    my.UpdatePriceTableData = function (data, contentId, isPaging) {
        var itemId = contentId.split('_')[1];
        var id = contentId.split('_')[0];
        bytCurrentPage = data.CurrentPage;
        if ($('#pricePageDetailTable' + contentId + ' thead tr th').length == 0) {
            $.template("colTemp", '<th  class="sortColumn" tag="${Sort}" onclick="Price.SortPriceDataColumn(\'' + contentId + '\',this, \'${ColumnName}\')">${Name}<span></span></th>');
            $('#pricePageDetailTable' + contentId + ' thead tr').empty();
            $("#pricePageDetailTable" + contentId + " thead tr").append('<th></th>');
            $.tmpl("colTemp", data.ColumTemplate).appendTo("#pricePageDetailTable" + contentId + " thead tr");
        }
        var markup = my.BuildRowTemplate(data.ColumTemplate);
        $.template("rowTemplate", markup);
        $('#pricePageDetailTable' + contentId + ' tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#pricePageDetailTable" + contentId + " tbody");
        if (isPaging) {
            var keyDict = my.TickedCache[contentId];
            var keys = Object.keys(keyDict);
            $.each(keys, function (i, v) {
                var row = $('#pricePageDetailTable' + contentId).find('tr[data-key="' + v + '"]');
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
        $('#pricePagePaginateOfLabel' + contentId).html(start + '-' + end + ' of ' + data.Total);
        if (data.Total == 0) {
            $('#pricePagePaginateOfLabel' + contentId + ',#pricePagePaginate' + contentId).hide();
            return;
        } else {
            $('#pricePagePaginateOfLabel' + contentId + ',#pricePagePaginate' + contentId).show();
        }
        $('#pricePagePaginate' + contentId).paginate({
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
                AjaxWithBlock($('#pricePageDetailTable' + contentId), '/GDT/GetPriceDetail', {
                    currentPage: page,
                    itemId: itemId,
                    id: id,
                    category: Price.getFilterString("pricePage" + contentId),
                    order: lastOrder
                }, function (mydata) { my.UpdatePriceTableData(mydata, contentId, true); });
            },
            firstText: my.paginateCommon.firstText,
            lastText: my.paginateCommon.lastText
        });
    };
    function getLegend(contentid) {
        var cols = $('#pricePageDetailTable' + contentid).data('legend').toString().split(',');
        return cols;
    }

    function getLegendText(contentid, key) {
        var colsArray = getLegend(contentid);
        var cols = colsArray.slice(0, colsArray.length - 1);
        var row = $('#pricePageDetailTable' + contentid).find('tr[data-key="' + key + '"]');
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
                ShowAlert(contentid, Price.MaxSelectionText, 250, 120);
                return;
            }
            var unitIndex = getLegend(contentid).pop();
            var currentUnit = row.find('td').eq(unitIndex).text();
            if (Object.keys(keyDict).length > 0) {
                var firstUnit = getFirstProductData(contentid).unit;
                if (firstUnit != currentUnit) {
                    ShowAlert(contentid, Price.MustSameUnit, 250, 120);
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
            changeChartExcelStatus(contentid, false);
            var keylist = Object.keys(keyDict).join(',');
            AjaxWithBlock($('#pricePage' + contentid), '/GDT/GetPriceChartDetailList',
            { itemId: father.data('item'), keyList: keylist },
            function (data) {
                for (var i = 0; i < data.length; i++) {
                    var ckey = data[i].name;
                    var legendItem = my.TickedCache[contentid][ckey].legend;
                    data[i].name = legendItem;
                }
                my.UpdatePriceChartData(data, contentid);
            });
        } else if (Object.keys(keyDict).length == 1) {
            key = Object.keys(keyDict)[0];
            my.RefreshSingleProductChart(key, contentid);
            changeChartExcelStatus(contentid, true);
        } else {
            my.RefreshSingleProductChart('', contentid);
            changeChartExcelStatus(contentid, false);
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
        $("#" + contentId + " select").each(function () {
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
    my.ExportExcelPriceDetails = function (link, contentId, reportId, itemId) {
        var filtes = Price.getFilterString("pricePage" + contentId);
        $(link).attr("href", '/GDT/ExportExcelForPriceDetail'
                            + '?reportId=' + reportId
                            + '&itemId=' + itemId
                            + '&category=' + filtes
                            + '&currentPage=' + bytCurrentPage + '&order=' + lastOrder
                    );
    };
    my.ExportExcelPriceChartDetails = function (link, reportId, itemId, contentId) {
        var keyDict = my.TickedCache[contentId];
        var key = Object.keys(keyDict)[0];
        var term = $('#chartTab_priceChartContainer' + contentId + ' li[class$="ui-tabs-active ui-state-active stCurrentTab"] a').first().text();
        $(link).attr("href", '/GDT/ExportExcelForPriceChartDetail' + '?reportId=' + reportId + '&itemId=' + itemId + '&key=' + key + '&term=' + term);
    };

    my.UpdatePriceData = function (contentId, reportId, containerId) {
        var filterStr = Price.getFilterString(contentId);
        AjaxWithBlock($('#pricePage' + containerId), '/GDT/GetPriceData', {
            id: reportId,
            itemId: containerId.split('_')[1],
            category: filterStr,
            order: lastOrder
        }, function (data) {
            Price.UpdatePriceTableData(data.table, containerId);
            Price.resetCache(containerId);
            Price.UpdatePriceChartData(data.chart.series, containerId);
        });
    };
    my.resetCache = function(contentId) {
        my.TickedCache[contentId] = {};
        var row = $('#pricePageDetailTable' + contentId + ' tbody tr:first()');
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
        var tpl = '<tr data-key=\'${KeyWord}\'><td><button class="untickedChart" onclick="Price.ToggleChart(this)"></button></td></td>';
        $.each(rawData, function (key, value) {
            if (value.ColumnType != 'decimal' && value.ColumnName != 're_date') {
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
                url: "/GDT/GetPriceChartDetail",
                dataType: "json",
                data: { itemId: contentId.split('_')[1], key: key, term: '5Y', reDate: getFirstProductData(contentId).date },
                success: function (data) {
                    my.UpdatePriceChartData(data.series, contentId);
                }
            });
        } else {
            my.UpdatePriceChartData([], contentId);
        }

    };


    my.SortPriceDataColumn = function (contentId, th, order) {
        var direction = $(th).attr('tag');
        $("#pricePageDetailTable" + contentId + " .sortColumn").attr('tag', '');
        switch (direction) {
            case "ASC":
                $(th).attr('tag', 'DESC');
                order = order + "  " + $(th).attr('tag') + ",[orderKey]";
                break;
            case "DESC":
                $(th).attr('tag', '');
                order = "re_date desc,[orderKey] ";
                break;
            case "":
                $(th).attr('tag', 'ASC');
                order = order + " " + $(th).attr('tag') + ",[orderKey]";
                break;
            default:
                break;
        }
        lastOrder = order;
        my.UpdatePriceDataBySort(contentId, order);
    };
    my.UpdatePriceDataBySort = function (contentId, order) {
        var filtes = Price.getFilterString("pricePage" + contentId);
        AjaxWithBlock($('#pricePageDetailTable' + contentId), '/GDT/GetPriceDetail', {
            start: $("#datePicker" + contentId).val(),
            end: $("#endDatePicker" + contentId).val(),
            currentPage: bytCurrentPage,
            id: contentId.split('_')[0],
            itemId: contentId.split('_')[1],
            category: filtes,
            order: order
        }, function (data) { Price.UpdatePriceTableData(data, contentId, true); });
    };
    return my;
} (Price || {}));