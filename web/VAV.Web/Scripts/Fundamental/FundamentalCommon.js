var Fundamental = (function (my) {
    my.OpenFundamentalDetail = function (id, name) {
        OpenReport('FundamentalDetail' + id, name, 'Fundamental Detail', '/Fundamental/NonlistIssuerDetailMain/', name);
    };
    my.BuildFundamentalDetail = function (data, pageId) {
        if (!data.ColumTemplate.length) {
            $('#emptyMessage' + pageId).show();
        }
        else {
            $('#emptyMessage' + pageId).hide();
        }
        var tableName = 'fundamentalDetail' + pageId;
        var topChart = 'topUnlistedDetailChart' + pageId;
        var bottomChart = 'bottomUnlistedDetailChart' + pageId;
        Fundamental.BuildDetailTable(data, 'fundamentalDetail' + pageId);
        $('#' + tableName + ' tbody tr').on('click', function () {
            Fundamental.UpdateDetailChart(this, topChart, pageId);
        });
        Fundamental.UpdateDetailChart($('#' + tableName + ' tbody tr[IP="0"]:first'), topChart, pageId);
        var bottomChartRow = $('#' + tableName + ' tbody tr[CR !="0"]');
        Fundamental.UpdateDetailChart(bottomChartRow, bottomChart, pageId);
        var graphTitle = $('#FDGraph' + pageId);
        graphTitle.html(bottomChartRow.attr('CR') + graphTitle.attr('tag'));
        var period = '#period' + pageId;
        var viewBy = '#viewBy' + pageId;
        var years = '#years' + pageId;
        var unit = '#unit' + pageId;
        $(period).change(function () {
            if ($(this).val() == "Y") {
                $(viewBy + ' option[value="QoQ"]').hide();
                $(years + ' option[value="1"]').hide();
                if ($(viewBy).val() == 'QoQ') {
                    $(viewBy).val('RawReport');
                }
                if ($(years).val() == '1') {
                    $(years).val('5');
                }
            } else {
                $(viewBy + ' option[value="QoQ"]').show();
                $(years + ' option[value="1"]').show();
            }
        });

        $(viewBy).change(function () {
            if ($(this).val() != "RawReport") {
                $(unit + ' option[value="P"]').show();
                $(unit).val('P');
                $(unit).prop('disabled', 'disabled');
            } else {
                $(unit + ' option[value="P"]').hide();
                $(unit).val('100M');
                $(unit).prop('disabled', false);
            }
        });

        amplify.subscribe('RefreshChart', pageId, Fundamental.ResizeChart);
        amplify.subscribe('RefreshFundamentalDetailChart', pageId, Fundamental.ResizeChart);

    };
    my.BuildDetailTable = function (tableData, tableName) {
        var header = $('#' + tableName + ' thead').empty();
        var body = $('#' + tableName + ' tbody').empty();
        var headerRow = $('<tr class="hr"></tr>');
        var rowTemplate = '<tr PID="${PID}" IP="${IP}" CR="${CR}">';
        $.each(tableData.ColumTemplate, function (key, value) {
            headerRow.append('<th>' + value.Name + '</th>');
            rowTemplate += '<td>${' + value.ColumnName + '}</td>';
        });
        rowTemplate += '</tr>';
        if (tableData.ExtraHeaders) {
            var exHeaderRow = $('<tr class="hr"></tr>');
            $.each(tableData.ExtraHeaders, function (key, value) {
                exHeaderRow.append('<th colspan="' + value.ColSpan + '">' + value.Name + '</th>');
            });
            header.append(exHeaderRow);
        }
        header.append(headerRow);
        $.template(tableName, rowTemplate);
        $.tmpl(tableName, tableData.RowData).appendTo(body);
    };
    my.UpdateDetailChart = function (row, chartname, id) {
        var headers = [];
        var headerLevel = $('#fundamentalDetail' + id + ' thead tr').length;
        $('#fundamentalDetail' + id + ' thead tr:nth-child(' + headerLevel + ') th:gt(0)').each(
                function () {
                    headers.push(GetUTC($(this).text()));
                });
        var sData = [];
        $(row).find('td:gt(0)').each(
                function (key, value) {
                    var cValue = parseFloat($(this).text().replace(/,/g, ''));
                    if (!$.isNumeric(cValue)) {
                        cValue = 0;
                    }
                    var point = [headers[key], cValue];
                    sData.push(point);
                });
        var chartTitle = $(row).find('td:first()').text();
        var yText = '%';
        if ($('#viewBy' + id).val() == 'RawReport') {
            yText = $('#unit' + id + ' option[value="' + $('#unit' + id).val() + '"]').text();
        }
        var currentChart = new Highcharts.Chart({
            chart: {
                type: $('#' + chartname + 'type').val(),
                renderTo: chartname,
                zoomType: 'None'
            },
            title: { text: chartTitle },
            xAxis: { type: 'datetime' },
            yAxis: { title: { text: yText} },
            plotOptions: { column: { borderWidth: 0} },
            series: [{ name: chartTitle, data: sData.reverse()
            }],
            legend: { enabled: false },
            credits: { href: 'http://thomsonreuters.com/', text: window.Common.ChartSource },
            tooltip: { formatter: function () { return Highcharts.dateFormat('%Y-%m-%d', this.x) + '<br><b>' + this.point.series.name + '</b>: ' + Highcharts.numberFormat(this.y,2); } }
        });
        my.detailCharts = my.detailCharts || {};
        my.detailCharts[chartname] = currentChart;
    };
    my.DetailChartTypeChanged = function (chartName, pageid) {
        var topChartId = 'topUnlistedDetailChart' + pageid;
        var bottomChartId = 'bottomUnlistedDetailChart' + pageid;
        if (chartName == topChartId) {
            Fundamental.UpdateDetailChart($('#fundamentalDetail' + pageid + ' tbody tr[IP="0"]:first'), topChartId, pageid);
        } else {
            Fundamental.UpdateDetailChart($('#fundamentalDetail' + pageid + ' tbody tr[CR !="0"]'), bottomChartId, pageid);
        }
    };
    my.RefreshDetailData = function (id, page, url) {
        var pageId = id + page;
        AjaxWithBlock($('#FD' + pageId).parent(), url,
                    {
                        period: $('#period' + pageId).val(),
                        viewBy: $('#viewBy' + pageId).val(),
                        years: $('#years' + pageId).val(),
                        unit: $('#unit' + pageId).val(),
                        id: id,
                        page: page
                    }, function (data) {
                        my.BuildFundamentalDetail(data, pageId);
                    });
    };
    my.ResizeChart = function () {
        var pageId = this;
        if ($('#fundamentalDetail' + pageId).is(":visible")) {
            var topChartId = 'topUnlistedDetailChart' + pageId;
            var upperChart = Fundamental.detailCharts[topChartId];
            if (upperChart) {
                var parent = $('#' + topChartId);
                setTimeout(function () {
                    upperChart.setSize(parent.width() - 10, parent.height() - 5, false);
                }, 200);
            }
            var bottomChartId = 'bottomUnlistedDetailChart' + pageId;
            var bottomChart = Fundamental.detailCharts[bottomChartId];
            if (upperChart) {
                var parent2 = $('#' + bottomChartId);
                setTimeout(function () {
                    bottomChart.setSize(parent2.width() - 10, parent2.height() - 5, false);
                }, 200);
            }
        }
    };
    my.ExportDetailTableExcel = function (link, id, page, url) {
        var pageId = id + page;
        $(link).attr("href", url
            + '?period=' + $('#period' + pageId).val()
            + '&viewBy=' + $('#viewBy' + pageId).val()
            + '&years=' + $('#years' + pageId).val()
            + '&unit=' + $('#unit' + pageId).val()
            + '&id=' + id
            + '&page=' + page
            + '&reportName=' + $('#fundamentalDetail' + id + 'header').text()
            );
    };
    return my;
} (Fundamental || {}))