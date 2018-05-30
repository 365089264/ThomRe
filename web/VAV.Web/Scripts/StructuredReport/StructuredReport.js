

var StructuredReport = (function (my) {

    my.BuildTable = function (id, data) {

        var $table = $('#table' + id);
        var $emptyMessage = $('#emptyMessage' + id);

        $('thead', $table).empty();
        $('tbody', $table).empty();

        if (data.RowData.length === 0) {
            //$('#' + id).prepend('<div><br /><span style="font-family:Arial;color:#bfbfbf;font-size:12px;font-weight:normal!important;margin-left:10px;">' + data.EmptyMessage + '</span></div>');
            $emptyMessage.show();
            $table.hide();
        } else {
            $emptyMessage.hide();
            $table.show();
            buildHeader();
            addRowspanForHeader();
            buildRows();
            addRowspanForRow();
            addTextAlignForRows();
            //$('#' + id).prepend($table);
        }


        function buildHeader() {


            if (data.ExtraHeaders && data.ExtraHeaders.length > 0) {
                buildExtraHeader();
            }

            $.template('headerTmpl', '<th>${Name}</th>');
            $('<tr class="hr"></tr>').append($.tmpl('headerTmpl', data.ColumTemplate)).appendTo($('thead', $table));

        }

        /**
         * add rowspan on header
         * @returns {} 
         */
        function addRowspanForHeader() {
            if ($('thead tr', $table).length > 1) {
                var $trs = $('thead tr', $table);
                var rowCount = $trs.length;
                var text = '';
                $trs.each(function () {
                    var $firstTh = $('th:first', $(this));
                    if ($firstTh.text()) {
                        text = $firstTh.text();
                    }
                    $firstTh.remove();
                });
                $('<th rowspan="' + rowCount + '">' + text + '</th>').prependTo($('thead tr:first', $table));
            }
        }

        /**
         * add rowspan for rows
         * @returns {} 
         */
        function addRowspanForRow() {
            if ($('tbody tr', $table).length > 1) {
                var tds = [];
                var groupedTds = {};
                var groupkey = 0;
                $('tbody tr td:first-child', $table)
                    .each(function (index) {
                        var td = {
                            text: $(this).text(),
                            element: $(this),
                            index: index
                        };
                        //$(this).remove();
                        tds.push(td);
                        if (groupedTds[groupkey]) {
                            if ((groupedTds[groupkey][groupedTds[groupkey].length - 1].text) === td.text &&
                            (td.index - groupedTds[groupkey][groupedTds[groupkey].length - 1].index) === 1) {
                                groupedTds[groupkey].push(td);
                            } else {
                                groupedTds[++groupkey] = [td];
                            }
                        } else {
                            groupedTds[++groupkey] = [td];
                        }
                    });
                //var groupedTds = groupBy(tds, 'text');

                

                for (var key in groupedTds) {
                    if (groupedTds.hasOwnProperty(key)) {
                        var tdlist = groupedTds[key];
                        if (tdlist.length > 1) {
                            var tdcount = tdlist.length;
                            $(tdlist[0].element).attr('rowspan', tdcount);
                            for (var i = 1; i < tdcount; i++) {
                                $(tdlist[i].element).remove();
                            }
                        }
                    }
                }
            }
        }

        function addTextAlignForRows() {
            $('td', $table)
                .each(function () {
                    var $ele = $(this);
                    if ($ele.text()) {
                        if (/^-?[0-9.,]+$/.test($ele.text())) {
                            this.style.textAlign = 'right';
                        }
                    }
                });
        }

        /**
         * Build extra headers and append to table.
         * @returns {} 
         */
        function buildExtraHeader() {
            var groupedHeaders = generateGroupHeadersByLevel(data.ExtraHeaders);
            for (var key in groupedHeaders) {
                if (groupedHeaders.hasOwnProperty(key)) {
                    var groupedHeader = groupedHeaders[key];
                    var extraHeadersHtmlString = '<tr class="hr"><th></th>';

                    groupedHeader.forEach(function (header) {

                        var thString = '';
                        thString += header.ColSpan > 0
                            ? '<th colspan="' + header.ColSpan + '" '
                            : '<th ';
                        thString += header.ColumnStyle ? 'style = ' + header.ColumnStyle : '';
                        thString += '>' + header.Name + '</th>';

                        extraHeadersHtmlString += thString;
                    });
                    extraHeadersHtmlString += '</tr>';
                    $('thead', $table).append(extraHeadersHtmlString);
                }
            }

        }

        function buildRows() {
            var sortedData = data.RowData.sort(function (a, b) {
                var aindex = parseInt(a.row_index), bindex = parseInt(b.row_index);
                return (aindex > bindex) ? 1 : ((bindex > aindex) ? -1 : 0);
            });
            sortedData.forEach(function (row) {
                //row.row_level is a string so use ==, or === '1'
                if (row.row_level == 1) {
                    row.chinese_name = '&nbsp;&nbsp;&nbsp;&nbsp;' + row.chinese_name;
                    row.variety = '&nbsp;&nbsp;&nbsp;&nbsp;' + row.variety;
                }
            });
            var markup = '<tr>';
            data.ColumTemplate.forEach(function (column) {
                markup += '<td>${' + column.ColumnName + '}</td>';
            });
            markup += '</tr>';
            $.template('rowTmpl', markup);
            $.tmpl('rowTmpl', sortedData).appendTo($('tbody', $table));
        }



        /**
         * @description group extra headers by level
         * @param {Array<Header>} extraHeaders 
         * @returns {} 
         */
        function generateGroupHeadersByLevel(extraHeaders) {
            return groupBy(extraHeaders, 'HeaderLevel');
        }

        /**
         * @description group an array with property key
         * @param {} xs 
         * @param {} key 
         * @returns {} 
         */
        function groupBy(xs, key) {
            return xs.reduce(function (rv, x) {
                (rv[x[key]] = rv[x[key]] || []).push(x);
                return rv;
            }, {});
        };
    };

    my.ExportExcel = function (link, reportId) {
        var startDate = $("#datePicker" + reportId).val();
        $(link).attr("href", "/StructureReport/ExportReport?reportId=" + reportId + "&startDate=" + startDate);
    };

    return my;
}(StructuredReport || {}));

