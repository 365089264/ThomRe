var Coal = (function (my) {
    var lastOrder = "";
    var bytCurrentPage = 1;
    my.arrHighCharts = {};
    my.TickedCache = {};
    my.UpdateCoalChartData = function (seriesData, reportId) {

        var modData;
        var unit = '';
        if (seriesData.length == 1) {
            modData = [
                {
                    name: getLegendText(reportId, getFirstProductData(reportId).key),
                    type: 'line',
                    id: 'primary',
                    data: seriesData[0].data
                },
                {
                    name: Price.SMA30,
                    linkedTo: 'primary',
                    showInLegend: true,
                    type: 'trendline',
                    algorithm: 'SMA',
                    periods: 30
                }
            ];
           
        } else {
            modData = seriesData;
        }
        if (seriesData.length > 0) {
            unit = getUnitText(reportId, seriesData[0].key);
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
                renderTo: 'coalChart' + reportId,
                zoomType: 'x'
            },
            title: { text: /*Price.chartCommon.title*/$("#coalChart" + reportId).attr("data-ChartTitle") },
            yAxis: { title: { text: $("#coalChart" + reportId).attr("data-ChartYLabel") + " ( " + unit + " )" }/*, opposite: false*/ },
            series: modData,
            legend: {
                align: 'left',
                verticalAlign: 'top',
                x: 1,
                y: 60,
                layout: 'vertical',
                floating: true,
                enabled: true
            },

            tooltip: {
                useHTML: false,
                formatter: function () {
                    var s = Highcharts.dateFormat('%Y-%m-%d', this.x);
                    $.each(this.points, function (i, point) {
                        s += '<br/><span style="color:' + point.series.color + '">' + point.series.name + '</span>: ' + Highcharts.numberFormat(point.y, 2);
                    });
                    return s;
                }
            }
        });
        my.arrHighCharts[reportId] = chart;
    };
    function getFirstProductData(reportId) {
        var keyDict = my.TickedCache[reportId];
        var key = Object.keys(keyDict)[0];
        return keyDict[key];
    }
    my.UpdateCoalTableData = function (data, reportId, isPaging) {
        bytCurrentPage = data.CurrentPage;
        if ($('#coalPageDetailTable' + reportId + ' thead tr th').length == 0) {
            $.template("colTemp", '<th  class="sortColumn" tag="${Sort}" onclick="Coal.SortCoalDataColumn(\'' + reportId + '\',this, \'${ColumnName}\')">${Name}<span></span></th>');
            $('#coalPageDetailTable' + reportId + ' thead tr').empty();
            $("#coalPageDetailTable" + reportId + " thead tr").append('<th></th>');
            $.tmpl("colTemp", data.ColumTemplate).appendTo("#coalPageDetailTable" + reportId + " thead tr");
        }
        var markup = my.BuildRowTemplate(data.ColumTemplate);
        $.template("rowTemplate", markup);
        $('#coalPageDetailTable' + reportId + ' tbody').empty();
        $.tmpl("rowTemplate", data.RowData).appendTo("#coalPageDetailTable" + reportId + " tbody");
        if (isPaging) {
            var keyDict = my.TickedCache[reportId];
            var keys = Object.keys(keyDict);
            $.each(keys, function (i, v) {
                var row = $('#coalPageDetailTable' + reportId).find('tr[data-key="' + v + '"]');
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
        $('#coalPagePaginateOfLabel' + reportId).html(start + '-' + end + ' of ' + data.Total);
        if (data.Total == 0) {
            $('#coalPagePaginateOfLabel' + reportId + ',#coalPaginate' + reportId).hide();
            return;
        } else {
            $('#coalPagePaginateOfLabel' + reportId + ',#coalPagePaginate' + reportId).show();
        }
        $('#coalPagePaginate' + reportId).paginate({
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
                AjaxWithBlock($('#coalPageDetailTable' + reportId), '/CNE/GetCoalTableData', {
                    currentPage: page,
                    reportId: reportId,
                    order: lastOrder
                }, function (mydata) { my.UpdateCoalTableData(mydata, reportId, true); });
            },
            firstText: Price.paginateCommon.firstText,
            lastText: Price.paginateCommon.lastText
        });
    };
    function getLegend(reportId) {
        var cols = $('#coalPageDetailTable' + reportId).data('legend').toString().split(',');
        return cols;
    }

    function getLegendText(reportId, key) {
        var cols = getLegend(reportId);
        var row = $('#coalPageDetailTable' + reportId).find('tr[data-key="' + key + '"]');
        if (cols.length == 1) {
            return row.find('td').eq(cols[0] +1).text();
        }
        return cols.map(function (v) {
            return row.find('td').eq(parseInt( v)  +1).text();
        }).join('-');
    }
    function getUnitText(reportId, key) {
        var unitIndex = $('#coalPageDetailTable' + reportId).data('unit');
        var row = $('#coalPageDetailTable' + reportId).find('tr[data-key="' + key + '"]');
        return row.find('td').eq(parseInt( unitIndex) + 1).text();
    }

    my.ToggleChart = function (obj) {
        var btn = $(obj);
        var row = btn.closest('tr');
        var key = row.data('key');
        var father = btn.closest('table');
        var reportId = father.data('report');
        var keyDict = my.TickedCache[reportId];
        if (!keyDict) {
            keyDict = {};
            my.TickedCache[reportId] = keyDict;
        };
        if (btn.hasClass('untickedChart')) {
            if (Object.keys(keyDict).length >= 5) {
                ShowAlert(reportId, Price.MaxSelectionText, 250, 120);
                return;
            }
            btn.removeClass('untickedChart');
            btn.addClass('tickedChart');
            keyDict[key] = { key: key, legend: getLegendText(reportId, key)};
        }
        else {
            btn.removeClass('tickedChart');
            btn.addClass('untickedChart');
            delete keyDict[key];
        }
        if (Object.keys(keyDict).length > 1) {
            changeChartExcelStatus(reportId, false);
            var keylist = Object.keys(keyDict).join('@_@');
            AjaxWithBlock($('#coalPage' + reportId), '/CNE/GetCoalChartDetailList',
            { reportId:reportId,keyList: keylist },
            function (data) {
                for (var i = 0; i < data.length; i++) {
                    var ckey = data[i].key;
                    var legendItem = my.TickedCache[reportId][ckey].legend;
                    data[i].name = legendItem;
                }
                my.UpdateCoalChartData(data, reportId);
            });
        } else if (Object.keys(keyDict).length == 1) {
            key = Object.keys(keyDict)[0];
            my.RefreshSingleProductChart(key, reportId);
            changeChartExcelStatus(reportId, true);
        } else {
            my.RefreshSingleProductChart('', reportId);
            changeChartExcelStatus(reportId, false);
        }
    };

    function changeChartExcelStatus(reportId, status) {
        var excel = $('#excelcoal' + reportId);
        if (status) {
            excel.show();
        } else {
            excel.hide();
        }
    };
    my.getFilterString = function (reportId) {
        var filterStr = '';
        var pFilter = $('#pFilter' + reportId);
        var sFilter = $('#sFilter' + reportId);
        if (pFilter.length > 0) {
            filterStr = pFilter.attr('pfieldname') + "=N'" + pFilter.val() +"'";
        }
        if (sFilter.length > 0) {
            filterStr += ' and ' + sFilter.attr('sfieldname') + "=N'" + sFilter.val()+"'";
        }
        return filterStr;
    };
    my.ExportExcelCoalDetails = function (link, reportId,name) {
        var filters = Coal.getFilterString(reportId);
        $(link).attr("href", '/CNE/ExportExcelForCoalDetail'
                            + '?reportId=' + reportId
                            + '&filters=' + filters
                            + '&currentPage=' + bytCurrentPage + '&order=' + lastOrder
                            +'&name=' +name
                    );
    };
    my.ExportExcelCoalChartDetails = function (link, reportId,name) {
        var keyDict = my.TickedCache[reportId];
        var key = Object.keys(keyDict)[0];
        $(link).attr("href", '/CNE/ExportExcelForCoalChartDetail?reportId='+ reportId + '&key=' + key + '&name=' + name);
    };

    my.UpdateCoalData = function (reportId) {
        var filterStr = Coal.getFilterString(reportId);
        AjaxWithBlock($('#coalPage' + reportId), '/CNE/GetCoalData', {
            reportId: reportId,
            currentPage:1,
            queryStr: filterStr,
            order: lastOrder
        }, function (data) {
            Coal.UpdateCoalTableData(data.table, reportId);
            Coal.resetCache(reportId);
            Coal.UpdateCoalChartData(data.chart, reportId);
        });
    };
    my.resetCache = function (reportId) {
        my.TickedCache[reportId] = {};
        var row = $('#coalPageDetailTable' + reportId + ' tbody tr:first()');
        var key = row.data('key');
        var keyDict = my.TickedCache[reportId];
        var btn = row.find('button');
        if (btn.length == 0) {
            changeChartExcelStatus(reportId, false);
        } else {
            changeChartExcelStatus(reportId, true);
        }
        btn.removeClass('untickedChart');
        btn.addClass('tickedChart');
        keyDict[key] = keyDict[key] = { key: key, legend: getLegendText(reportId, key)};
    };
    my.BuildRowTemplate = function (rawData) {
        var tpl = '<tr data-key=\'${KeyWord}\'><td><button class="untickedChart" onclick="Coal.ToggleChart(this)"></button></td></td>';
        $.each(rawData, function (key, value) {
            if (value.ColumnType != 'decimal') {
                tpl = tpl + '<td style="text-align:left">${' + value.ColumnName + '}</td>';
            } else {
                tpl = tpl + '<td>${' + value.ColumnName + '}</td>';
            }
        });
        tpl = tpl + '</tr>';
        return tpl;
    };
    my.RefreshSingleProductChart = function (key, reportId) {
        if (key) {
            $.ajax({
                type: "POST",
                url: "/CNE/GetCoalChartDetailList",
                dataType: "json",
                data: { keyList: key, reportId: reportId },
                success: function (data) {
                    my.UpdateCoalChartData(data, reportId);
                }
            });
        } else {
            my.UpdateCoalChartData([], reportId);
        }

    };


    my.SortCoalDataColumn = function (reportId, th, order) {
        var direction = $(th).attr('tag');
        $("#pricePageDetailTable" + reportId + " .sortColumn").attr('tag', '');
        switch (direction) {
            case "ASC":
                $(th).attr('tag', 'DESC');
                order = order + "  DESC";
                break;
            case "DESC":
                $(th).attr('tag', '');
                order = "";
                break;
            case "":
                $(th).attr('tag', 'ASC');
                order = order + " ASC";
                break;
            default:
                break;
        }
        lastOrder = order;
        my.UpdateCoalDataBySort(reportId, order);
    };
    my.UpdateCoalDataBySort = function (reportId, order) {
        var filterStr = Coal.getFilterString(reportId);
        AjaxWithBlock($('#coalPageDetailTable' + reportId), '/CNE/GetCoalTableData', {
            currentPage: 1,
            reportId:reportId,
            order: order,
            queryStr: filterStr
        }, function (data) { my.UpdateCoalTableData(data, reportId, true); });
    };
    my.HandlePFilterChange = function(reportId) {
        var pFilter = $('#pFilter' + reportId);
        $.ajax({
            type: "POST",
            url: "/CNE/GetSubFilter",
            dataType: "json",
            data: { sfilterId: pFilter.attr('sfilterId'), selectedPrimaryItem: pFilter.val() },
            success: function (data) {
                var sFilter = $('#sFilter' + reportId);
                sFilter.find('option').remove();
                $(data.Items).each(function () {
                    var $option = $("<option />");
                    $option.attr("value", this.Value).text(this.Text);
                    if (this.Selected) {
                        $option.attr('selected', 'selected');
                    }
                    $(sFilter).append($option);
                });
            }
        });
    }
    return my;
} (Coal || {}));