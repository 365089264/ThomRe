var WMP = (function (my) {
    my.BuildRowTemplate = function (rawData, actionMethod) {
        var tpl = '<tr>';
        $.each(rawData, function (key, value) {
            if (value.ColumnName == 'PRD_NAME') {
                tpl = tpl + '<td title="${PRD_NAME}" class="textLeft readTimeTD" onclick="WMP.OpenBankWMPDetail(\'${INNER_CODE}\', \'${PRD_NAME}\',\'' + actionMethod + '\')\"><a href="javascript:void(0)" class="realTime"></a><span style="width:350px;overflow:hidden;text-overflow: ellipsis;display:inline-block;">${PRD_NAME}</span></td>';
            } else if (value.ColumnName == 'ACCE_ROUTE') {
                tpl = tpl + '{{if ACCE_ROUTE}}<td style="text-align:center;" onclick="WMP.DownloadBankWMPFile(\'${INNER_CODE}\')\"><a href="javascript:void(0)" class="link">' + WMP.frontTexts.downloadText + '</a></td>{{else}}<td style="text-align:center;">-</td>{{/if}}';
            } else if (value.ColumnName == 'CFPNAME') {
                tpl = tpl + '<td title="${CFPNAME}" class="textLeft readTimeTD" onclick="WMP.OpenBankWMPDetail(\'${INNER_CODE}\', \'${CFPNAME}\',\'' + actionMethod + '\')\"><a href="javascript:void(0)" class="realTime"></a><span style="width:350px;overflow:hidden;text-overflow: ellipsis;display:inline-block;">${CFPNAME}</span></td>';
            } else if (value.ColumnName == 'WEEK_GR') {
                tpl = tpl + '{{if WEEK_GR>0}}<td style="color:#FF0000;">${WEEK_GR}</td>{{else WEEK_GR<0}}<td style="color:#3FFF01;">${WEEK_GR}</td>{{else}}<td>${WEEK_GR}</td>{{/if}}';
            } else if (value.ColumnName == 'MONTH_GR') {
                tpl = tpl + '{{if MONTH_GR>0}}<td style="color:#FF0000;">${MONTH_GR}</td>{{else MONTH_GR<0}}<td style="color:#3FFF01;">${MONTH_GR}</td>{{else}}<td>${MONTH_GR}</td>{{/if}}';
            } else if (value.ColumnName == 'ANNU_GR') {
                tpl = tpl + '{{if ANNU_GR>0}}<td style="color:#FF0000;">${ANNU_GR}</td>{{else ANNU_GR<0}}<td style="color:#3FFF01;">${ANNU_GR}</td>{{else}}<td>${ANNU_GR}</td>{{/if}}';
            } else if (value.ColumnName == 'BUILD_ANNU_GR') {
                tpl = tpl + '{{if BUILD_ANNU_GR>0}}<td style="color:#FF0000;">${BUILD_ANNU_GR}</td>{{else BUILD_ANNU_GR<0}}<td style="color:#3FFF01;">${BUILD_ANNU_GR}</td>{{else}}<td>${BUILD_ANNU_GR}</td>{{/if}}';
            } else if (value.ColumnName == 'ISS_AREA') {
                tpl = tpl + '<td class="textLeft width-limitedSmal" style="max-width: 100px;" title="${ISS_AREA}">${ISS_AREA}</td>';
            } else {
                tpl = tpl + '<td ' + appendTextAlgin(value.ColumnType) + '>${' + value.ColumnName + '}</td>';
            }
        });
        tpl = tpl + '</tr>';
        return tpl;
    };

    my.DownloadBankWMPFile = function (innercode) {
        location.href = "/WMP/DonwloadBankWMPFile/" + innercode;
    };

    my.BuildBankSelect = function (sl, data) {
        var markup = WMP.BuildBankOptionTempl();
        $.template("optionTemplate", markup);
        $(sl + ' option').remove();
        $.tmpl("optionTemplate", data).appendTo(sl);
    };

    my.BuildGeneralMultiSelect = function (sl, data, markupTmpl) {
        var markup = markupTmpl;
        $.template("optionTemplate", markup);
        $(sl + ' option').remove();
        $.tmpl("optionTemplate", data).appendTo(sl);
        $(sl).multiselect({
            buttonMinWidth: 120,
            menuMinWidth: 120,
            checkAllText: WMP.multiSelectTexts.checkAllText,
            uncheckAllText: WMP.multiSelectTexts.uncheckAllText,
            noneSelectedText: WMP.multiSelectTexts.noneSelectedText,
            selectedText: '# ' + WMP.multiSelectTexts.selectedText
        });
        $(sl).multiselect("refresh");
        $(sl).multiselect("checkAll");
    };

    my.BuildBankMultiSelect = function (sl, data) {
        var markup = WMP.BuildBankOptionTempl();
        $.template("optionTemplate", markup);
        $(sl + ' option').remove();
        $.tmpl("optionTemplate", data).appendTo(sl);
        $(sl).multiselect({
            buttonMinWidth: 120,
            menuMinWidth: 240,
            checkAllText: WMP.multiSelectTexts.checkAllText,
            uncheckAllText: WMP.multiSelectTexts.uncheckAllText,
            noneSelectedText: WMP.multiSelectTexts.noneSelectedText,
            selectedText: '# ' + WMP.multiSelectTexts.selectedText
        });
        $(sl).multiselect("refresh");
        $(sl).multiselect("checkAll");
    };

    my.BuildBankOptionTempl = function () {
        var tpl = '<option value="${BankId}">';
        tpl = tpl + '${BankName}</option>';
        return tpl;
    };

    my.BuildCityOptionTempl = function () {
        var tpl = '<option value="${Code}">';
        tpl = tpl + '${Name}</option>';
        return tpl;
    };

    my.UpdateBankOption = function (bankTypeSl, bankSl) {
        var code = $(bankTypeSl).val();
        if (code != null) code = code.join();
        $.ajax({
            type: 'Get',
            url: '/WMP/GetBankOptionByType',
            data: { typeCode: code },
            success: function (data) {
                WMP.BuildBankSelect(bankSl, data);
            },
            error: function () {
                debugger;
            },
            async: true
        });
    };

    my.UpdateMultipleBankOption = function (bankTypeSl, bankSl) {
        //debugger;
        var code = $(bankTypeSl).val();
        if (code != null) code = code.join();
        $.ajax({
            type: 'Get',
            url: '/WMP/GetMultipleBankOptionByType',
            data: { typeCode: code },
            success: function (data) {
                WMP.BuildBankMultiSelect(bankSl, data);
            },
            error: function () {
                debugger;
            },
            async: true
        });
    };

    my.UpdateMultipleCityOption = function (regionSl, citySl, btnQuery) {
        var code = $(regionSl).val();
        if (code != null) code = code.join();
        $.ajax({
            type: 'Get',
            url: '/WMP/GetMultipleCityOptionByType',
            data: { regionCode: code },
            success: function (data) {
                WMP.BuildGeneralMultiSelect(citySl, data, my.BuildCityOptionTempl());
            },
            error: function () {
                debugger;
            },
            beforeSend: function (XMLHttpRequest) {
                //debugger;
                if (typeof (btnQuery) != "undefined") {
                    //$(btnQuery).hide();
                    //$(btnQuery).attr("readonly", true);
                    $(btnQuery).attr("disabled", true);
                    $(btnQuery).addClass("disabledButton");
                }
            },
            complete: function (XMLHttpRequest, textStatus) {
                if (typeof (btnQuery) != "undefined") {
                    //$(btnQuery).show();
                    //$(btnQuery).attr("readonly", false);
                    $(btnQuery).attr("disabled", false);
                    $(btnQuery).removeClass("disabledButton");
                    //setTimeout(function(){delayRun(btnQuery)}, 1000*60)
                }
            },
            async: true
        });
    };

    //function delayRun(btnQuery) {
    //    //debugger;
    //    if (typeof (btnQuery) != "undefined") {
    //        //$("#btnQuery").show();
    //        //$(btnQuery).attr("readonly", false);
    //        $(btnQuery).attr("disabled", false);
    //        $(btnQuery).removeClass("disabledButton");
    //    }
    //}

    my.OpenBankWMPDetail = function (id, name, openMethod) {
        var title = name;
        if (title.length > 25) {
            title = name.substring(0, 20) + " ...";
        }
        OpenReport('WMPD' + id, title, 'WMP Detail', openMethod, name);
    };

    function appendTextAlgin(stringType) {
        var className = '';
        if (stringType == 'datetime' || stringType == 'text')
            className = 'class="textLeft"';
        return className;
    }

    my.AddSortWMPBankTag = function (th, order) {
        //disable columns sorting when it's of text;
        if (order == "AD_TRM_CND" || order == "AD_PRC_CND" || order == "RETN_TYPE" || order == "INC_CLA_BASIS") {
            return;
        }
        var ch = $(th);
        ch.siblings().attr('tag', '');
        switch (ch.attr('tag')) {
            case "ASC":
                ch.attr('tag', 'DESC');
                break;
            case "DESC":
                ch.attr('tag', '');
                break;
            case "":
                ch.attr('tag', 'ASC');
                break;
            default:
                break;
        }
    };

    my.UpdateBrokerNetWorthData = function (pageId, url) {
        AjaxWithBlock(
            $("#cfpWMPDetailTabs" + pageId).parent(),
            url,
            {
                id: pageId,
                startDate: $("#datePicker" + pageId).val(),
                endDate: $("#endDatePicker" + pageId).val()
            },
            function (data) {
                my.BuildBrokerNetWorth(data, pageId);
            }
        );
    };

    my.ExportExcelForNetWorthDetail = function (link, id, url) {
        $(link).attr("href", url
            + '?id=' + id
            + '&reportName=' + $('#WMPD' + id + ' h2').text()
            + '&startDate=' + $('#datePicker' + id).val()
            + '&endDate=' + $('#endDatePicker' + id).val()
        );
    };

    my.ExportExcelForBrokerFinIdx = function (link, id, url) {
        $(link).attr("href", url
            + '?id=' + id
        );
    };

    my.BuildBrokerNetWorth = function (data, pageId) {
        my.BuildBrokerNetWorthTable(data, pageId);
        my.BuildBrokerNetWorthChart(data, pageId);

        var parentTab = "cfpWMPDetailTabs" + pageId;
        $("#" + parentTab).unblock();

        amplify.subscribe('RefreshChart', pageId, WMP.ResizeBrokerChart);
        amplify.subscribe('RefreshBrokerChart', pageId, WMP.ResizeBrokerChart);
    };

    my.BuildBrokerNetWorthTable = function (data, pageId) {
        var tableName = "table" + pageId;
        var header = $('#' + tableName + ' thead').empty();
        var body = $('#' + tableName + ' tbody').empty();
        var headerRow = $('<tr class="hr"></tr>');
        var rowTemplate = '<tr >';
        $.each(data.ColumTemplate, function (key, value) {
            headerRow.append('<th>' + value.Name + '</th>');
            rowTemplate += '<td style="text-align: center;">${' + value.ColumnName + '}</td>';
        });
        rowTemplate += '</tr>';
        header.append(headerRow);
        $.template(tableName, rowTemplate);
        $.tmpl(tableName, data.RowData).appendTo(body);
    };

    my.BuildBrokerNetWorthChart = function (data, pageId) {
        var chartname = "chart" + pageId;

        var xdate = [];
        var unitName = $("#table" + pageId + " th").eq(1).text();
        var unitData = [];

        var accuName = $("#table" + pageId + " th").eq(2).text();
        var accuData = [];

        $.each(data.RowData, function () {
            var unitVal = parseFloat(this["Unit"].replace(/,/g, ''));
            if (!$.isNumeric(unitVal)) {
                unitVal = 0;
            }
            var accuVal = parseFloat(this["Acc"].replace(/,/g, ''));
            if (!$.isNumeric(accuVal)) {
                accuVal = 0;
            }
            xdate.push(GetUTC(this["Date"]));
            unitData.push([GetUTC(this["Date"]), unitVal]);
            accuData.push([GetUTC(this["Date"]), accuVal]);
        });
        xdate = xdate.reverse();
        unitData = unitData.reverse();
        accuData = accuData.reverse();


        $('#' + chartname).highcharts('StockChart', {
            rangeSelector: {
                enabled: false
            },
            navigator: {
                enabled: false
            },
            scrollbar: {
                enabled: false
            },
            title: { text: WMP.brokerTexts.netWorthChartTitle },
            xAxis: {
                type: 'datetime',
                dateTimeLabelFormats: {
                    day: '%m-%d',
                    week: '%m-%d',
                    month: '%Y-%m',
                    year: '%Y'
                }
            },
            yAxis: {
                title: { text: WMP.brokerTexts.netWorth },
                opposite: false // display y in the left
            },
            series: [{ name: unitName, data: unitData }, { name: accuName, data: accuData }],
            legend: { enabled: true },
            credits: { href: 'http://thomsonreuters.com/', text: window.Common.ChartSource },
            tooltip: {
                formatter: function () {
                    var s = Highcharts.dateFormat('%Y-%m-%d', this.x);
                    $.each(this.points, function (i, point) {
                        s += '<br/><span style="color:' + point.series.color + '">' + point.series.name + '</span>: ' + Highcharts.numberFormat(point.y, 4);
                    });
                    return s;
                }
            }
        });
        my.detailCharts = my.detailCharts || {};
        var chart = $('#' + chartname).highcharts();
        my.detailCharts[chartname] = chart;
    };

    my.ResizeBrokerChart = function () {
        var pageId = this;
        if ($('#cfpWMPDetailTabs' + pageId).is(":visible")) {
            var chartname = 'chart' + pageId;
            var chart = WMP.detailCharts[chartname];
            if (chart) {
                var parent = $('#' + chartname);
                setTimeout(function () {
                    console.log(parent.width());
                    chart.setSize(parent.width(), parent.height(), false);
                }, 200);
            }
        }
    };


    ///Broker content 
    my.ShowBrokerTextContent = function (content) {
        ShowDialog(content.TITLE, content.TXT_CONTENT);
    };

    return my;
}(WMP || {}));

