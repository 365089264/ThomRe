var Price = (function (my) {
    my.FanyaSource = '数据来源：泛亚金属交易所 、汤森路透',
    my.chartCommon = {
        title: '价格走势图',
        ytitle: '价格'
    };

    my.paginateCommon = {
        firstText: '首页',
        lastText: '尾页'
    };
    my.MaxSelectionText = '最多可以选择5个产品';
    my.MustSameUnit = '新选择的产品单位和其他不一样';
    my.SMA30 = '30日均线';
    return my;
} (Price || {}));

var Output = (function (my) {

    my.chartCommon = {
        title: '产量走势图',
        ytitle: '产量'
    };

    my.paginateCommon = {
        firstText: '首页',
        lastText: '尾页'
    };
    my.MaxSelectionText = '最多可以选择5个产品';
    my.MustSameUnit = '新选择的产品单位和其他不一样';
    return my;
} (Output || {}));

var Inventory = (function (my) {

    my.chartCommon = {
        title: '库存走势图',
        ytitle: '库存'
    };

    my.paginateCommon = {
        firstText: '首页',
        lastText: '尾页'
    };
    my.MaxSelectionText = '最多可以选择5个产品';
    my.MustSameUnit = '新选择的产品单位和其他不一样';
    return my;
} (Inventory || {}));

var Sales = (function (my) {

    my.chartCommon = {
        title: '销量走势图',
        ytitle: '销量'
    };

    my.paginateCommon = {
        firstText: '首页',
        lastText: '尾页'
    };
    my.MaxSelectionText = '最多可以选择5个产品';
    my.MustSameUnit = '新选择的产品单位和其他不一样';
    return my;
} (Sales || {}));

var EnergyInventory = (function (my) {

    my.chartCommon = {
        title: '库存走势图',
        ytitle: '库存'
    };

    my.unit = '百万吨';
    return my;
} (EnergyInventory || {}));