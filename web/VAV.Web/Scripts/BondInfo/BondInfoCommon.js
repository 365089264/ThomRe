var BondInfo = (function (my) {
    my.ChangeContextMenu = function (ev, id) {
        JET.contextMenu({
            mouse: {
                screenX: ev.screenX,
                screenY: ev.screenY,
                clientX: ev.clientX,
                clientY: ev.clientY
            },
            entities: [{ "EJVAssetID": '0x' + id}]
        });
    };


    my.OpenExplorer = function (assetId) {
        var url = 'cpurl://views.cp./Explorer/Default.aspx?s=0x' + assetId + '&st=EJVAssetID';
        window.open(url);
    };

    my.OpenExplorer_OrgId = function (orgId) {
        var url = 'cpurl://views.cp./Explorer/EVzCORPxPRIVzOV_TEST.aspx?s=' + orgId + '&st=Orgid';
        window.open(url);
    };

    my.BuildRowTemplate = function (rawData, namePrefix) {
        var tpl = '<tr onContextMenu="BondInfo.ChangeContextMenu(event,\'${AssetId}\')">';
        $.each(rawData, function (key, value) {
            if (value.ColumnName == 'BondName') {
                tpl = tpl + '<td title="${BondName}" class="textLeft readTimeTD" onclick="BondInfo.OpenExplorer(\'${AssetId}\')\"><a href="javascript:void(0)" class="realTime"></a><span style="' + value.ColumnStyle + '">${BondName}</span></td>';
            } else if (value.ColumnName == 'BondRatingHist') {
                var openName = namePrefix + '_' + '${Code}' + ' ' +'${BondName}';
                tpl = tpl + '{{if Code}}<td style="text-align:left;">'
                    + '<a class="link" href="' + 'javascript:BondInfo.OpenBondRatingHist(\'${Code}\', \''+ openName + '\');">'
                    + BondInfo.bondinfoTexts.textView + '</a></td>' +
                    '{{else}}<td></td>{{/if}}';
            } else if (value.ColumnName == 'Issuer') {
                tpl = tpl + '{{if Code}}<td class="readTimeTD" style="text-align: left;" onclick="BondInfo.OpenIssuerDetail(\'${Code}\', \'${Issuer}\' );">'
                    + '${Issuer}</td>' +
                    '{{else}}<td style="text-align:left;">${Issuer}</td>{{/if}}';
            } else if (value.ColumnName == 'BondIssuerRating') {
                tpl = tpl + '{{if Code}}<td class="readTimeTD" style="text-align: left;" onclick="BondInfo.OpenIssuerDetailToRating(\'${Code}\', \'${Issuer}\' );">'
                    + BondInfo.bondinfoTexts.textView + '</td>' +
                    '{{else}}<td></td>{{/if}}';
            } else {
                if (value.ColumnStyle != "") {
                    tpl = tpl + '<td title="${' + value.ColumnName + '}" ' + appendTextAlgin(value.ColumnType) + '><span style="' + value.ColumnStyle + '">${' + value.ColumnName + '}</span></td>';
                } else {
                    tpl = tpl + '<td ' + appendTextAlgin(value.ColumnType) + '>${' + value.ColumnName + '}</td>';
                }
            }
        });
        tpl = tpl + '</tr>';
        return tpl;
    };

    my.BuildRowTemplate_DimsumBondAnalysis = function (rawData, namePrefix) {
        var tpl = '<tr onContextMenu="BondInfo.ChangeContextMenu(event,\'${AssetId}\')">';
        $.each(rawData, function (key, value) {
            if (value.ColumnName == 'BondName') {
                tpl = tpl + '<td class="textLeft readTimeTD" onclick="BondInfo.OpenExplorer(\'${AssetId}\')\"><a href="javascript:void(0)" class="realTime"></a>${BondName}</td>';
            } else if (value.ColumnName == 'BondRatingHist') {
                var openName = namePrefix + '_' + '${Code}' + ' ' + '${BondName}';
                tpl = tpl + '{{if Code}}<td style="text-align:left;">'
                    + '<a class="link" href="' + 'javascript:BondInfo.OpenBondRatingHist(\'${Code}\', \'' + openName + '\');">'
                    + BondInfo.bondinfoTexts.textView + '</a></td>' +
                    '{{else}}<td></td>{{/if}}';
            } else if (value.ColumnName == 'Issuer') {
                tpl = tpl + '<td class="textLeft readTimeTD" style="text-align:left;" onclick="BondInfo.OpenExplorer_OrgId(\'${IssuerOrgId}\')\"><a href="javascript:void(0)" class="realTime"></a>${Issuer}</td>';
            } else if (value.ColumnName == 'BondIssuerRating') {
                tpl = tpl + '{{if Code}}<td class="readTimeTD" style="text-align: left;" onclick="BondInfo.OpenIssuerDetailToRating(\'${Code}\', \'${Issuer}\' );">'
                    + BondInfo.bondinfoTexts.textView + '</td>' +
                    '{{else}}<td></td>{{/if}}';
            } else {
                tpl = tpl + '<td ' + appendTextAlgin(value.ColumnType) + '>${' + value.ColumnName + '}</td>';
            }
        });
        tpl = tpl + '</tr>';
        return tpl;
    };


    my.BuildDimSumSummaryRowTemplate = function (id) {
        var tpl = '<tr tag="${Type}" onclick="BondInfo.RefreshDimSumDetail(this, \'' + id + '\')\">';
        tpl = tpl + '<td class="textLeft">${TypeName}</td>';
        tpl = tpl + '<td>${InitialBalance}</td>';
        tpl = tpl + '<td>${Issues}</td>';
        tpl = tpl + '<td>${IssuesPercent}</td>';
        tpl = tpl + '<td>${IssuesAmount}</td>';
        tpl = tpl + '<td>${IssuesAmountPercent}</td>';
        tpl = tpl + '<td>${MaturityBonds}</td>';
        tpl = tpl + '<td>${MaturityBondsPercent}</td>';
        tpl = tpl + '<td>${MaturityAmount}</td>';
        tpl = tpl + '<td>${MaturityAmountPercent}</td>';
        tpl = tpl + '<td>${EndBalance}</td>';
        tpl = tpl + '</tr>';
        return tpl;
    };

    my.GetCheckedMultiSelectValue = function (id) {
        var checkedValues = $.map($("#" + id).multiselect("getChecked"), function (input) {
            return input.value;
        });
        return checkedValues;
    };

    function appendTextAlgin(stringType) {
        var className = '';
        if (stringType == 'datetime' || stringType == 'text')
            className = 'class="textLeft"';
        return className;
    }

    my.OpenIssuerDetail = function(id, name) {
        OpenReport('FundamentalDetail' + id, name, 'Fundamental Detail', '/Fundamental/NonlistIssuerDetailMainFromCode/', name);
    };

    my.OpenIssuerDetailToRating = function(id, name) {
        OpenReport('FundamentalDetail' + id, name, 'Fundamental Detail', '/Fundamental/NonlistIssuerDetailMainFromCode/', name, { show: "rating" });
    };

    my.OpenBondRatingHist = function(id, name) {
        var title = name.trim();
        if (title.length > 30) {
            title = name.substring(0, 25) + " ...";
        }
        OpenReport('BondRating' + id, title, 'Bond Rating', '/BondReport/BondRatingHist/', name);
    };

    my.UpdatePaginationLabel = function (current, pageSize, total, reportId, text) {
        var start = (current - 1) * pageSize + 1;
        var end = current * pageSize;
        if (end > total) end = total;
        $('#pricePagePaginateOfLabel' + reportId).html(start + '-' + end + text + total);
    };
    my.UpdateBondPagination = function (data, reportId, text, ftext, ltext, callback) {
        debugger;
        if (data.Total == 0) {
            $('#pricePagePaginateOfLabel' + reportId + ',#pricePagePaginate' + reportId).hide();
            return;
        } else {
            $('#pricePagePaginateOfLabel' + reportId + ',#pricePagePaginate' + reportId).show();
        }
        my.UpdatePaginationLabel(data.CurrentPage, 200, data.Total, reportId, text);
        $('#pricePagePaginate' + reportId).paginate({
            count: Math.ceil(data.Total / 200),
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
                callback(reportId, page);
            },
            firstText: ftext,
            lastText: ltext
        });
    };
    return my;
} (BondInfo || {}));

