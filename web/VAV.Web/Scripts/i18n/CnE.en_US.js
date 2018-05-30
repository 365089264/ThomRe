var Price = (function (my) {
    my.FanyaSource = 'Source:Fanya, Thomson Reuters',
    my.chartCommon = {
        title: 'Price charts',
        ytitle: 'Price'
    };

    my.paginateCommon = {
        firstText: 'first',
        lastText: 'last'
    };
    my.MaxSelectionText = 'Cannot select more than 5 products';
    my.MustSameUnit = 'The selected product has different unit';
    my.SMA30 = '30 days SMA';
    return my;
} (Price || {}));

var Output = (function (my) {

    my.chartCommon = {
        title: 'Output charts',
        ytitle: 'Output'
    };

    my.paginateCommon = {
        firstText: 'first',
        lastText: 'last'
    };
    my.MaxSelectionText = 'Cannot select more than 5 products';
    my.MustSameUnit = 'The selected product has different unit';
    return my;
} (Output || {}));


var Inventory = (function (my) {

    my.chartCommon = {
        title: 'Inventory charts',
        ytitle: 'Inventory'
    };

    my.paginateCommon = {
        firstText: 'first',
        lastText: 'last'
    };
    my.MaxSelectionText = 'Cannot select more than 5 products';
    my.MustSameUnit = 'The selected product has different unit';
    return my;
} (Inventory || {}));

var Sales = (function (my) {

    my.chartCommon = {
        title: 'Sales charts',
        ytitle: 'Sales'
    };

    my.paginateCommon = {
        firstText: 'first',
        lastText: 'last'
    };
    my.MaxSelectionText = 'Cannot select more than 5 products';
    my.MustSameUnit = 'The selected product has different unit';
    return my;
} (Sales || {}));

var EnergyInventory = (function (my) {

    my.chartCommon = {
        title: 'Inventory charts',
        ytitle: 'Inventory'
    };

    my.unit = 'MT';
    return my;
} (EnergyInventory || {}));