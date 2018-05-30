var hierarchy = (function (my) {
    my.initItemGroup = function (categoryType, itemList, isTotal, totalColumns) {
        debugger;
        if (!categoryType || itemList.length==0) {
            return itemList;
        }
        var groupList = [];
        var totalRow = {};
        var categoryList = new Array();//获取分类所有项
        $.each(itemList, function (key, value) {
            if (categoryList.indexOf(value[categoryType]) < 0) {
                categoryList.push(value[categoryType]);
            }
        });
        if (isTotal) {
            //总的合计
            totalRow["isSumItem"] = true;
            totalRow[categoryType] = my.total;
            $.each(totalColumns, function (key, value) {
                totalRow[value] = 0;
            });
        }
        debugger;
        $.each(categoryList, function (key, value) {
            //分组统计
            var sumRow = {};
            sumRow["isSumItem"] = true;
            $.each(totalColumns, function (key2, value2) {
                sumRow[value2] = 0;
            });
            sumRow[categoryType] = value;
            var temp = new Array();
            $.each(itemList, function (key1, value1) {
                if (value1[categoryType] == value) {
                    temp.push(value1);
                    $.each(totalColumns, function (key3, value3) {
                        sumRow[value3] += parseFloat(value1[value3].replace(',',''));
                        totalRow[value3] += parseFloat(value1[value3].replace(',', ''));
                    });
                }
            });
            $.each(totalColumns, function (key3, value3) {
                //添加千位符
                sumRow[value3] = my.ConvertSeparator(sumRow[value3]);
            });
            groupList.push(sumRow);
            $.each(temp, function(i,j) {
                groupList.push(j);
            });
        });
        debugger;
        if (isTotal) {
            $.each(totalColumns, function (key, value) {
                totalRow[value] = my.ConvertSeparator(totalRow[value]);
            });
            groupList.push(totalRow);
        }
        return groupList;
    };
    
    my.ConvertSeparator = function(money) {//添加千位符
        var s = money; //获取小数型数据
        s += "";
        if (s.indexOf(".") == -1) s += ".0"; //如果没有小数点，在后面补个小数点和0
        if (/\.\d$/.test(s)) s += "0"; //正则判断
        while (/\d{4}(\.|,)/.test(s)) //符合条件则进行替换
            s = s.replace(/(\d)(\d{3}(\.|,))/, "$1,$2"); //每隔3位添加一个
        return s;
    };
    return my;
} (hierarchy || {}));