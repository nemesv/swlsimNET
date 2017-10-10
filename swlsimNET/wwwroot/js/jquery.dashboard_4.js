/**
* Theme: Ubold Admin Template
* Author: Coderthemes
* Component: Dashboard 4
* 
*/

!function($) {
    "use strict";

    const Dashboard4 = function () {
    };


    //creates area chart
    Dashboard4.prototype.createAreaChart =
        function (element, pointSize, lineWidth, data, xkey, ykeys, labels, lineColors) {
            Morris.Area({
                element: element,
                pointSize: 0,
                lineWidth: 0,
                data: data,
                xkey: xkey,
                ykeys: ykeys,
                labels: labels,
                hideHover: "auto",
                resize: true,
                gridLineColor: "#2f3e47",
                gridTextColor: "#98a6ad",
                lineColors: lineColors
            });
        },

        //creates Bar chart
        Dashboard4.prototype.createBarChart = function (element, data, xkey, ykeys, labels, lineColors) {
            Morris.Bar({
                element: element,
                data: data,
                xkey: xkey,
                ykeys: ykeys,
                labels: labels,
                hideHover: "auto",
                resize: true, //defaulted to true
                gridLineColor: "#2f3e47",
                gridTextColor: "#98a6ad",
                barColors: lineColors
            });
        },
        Dashboard4.prototype.init = function () {

            //creating area chart
            const $areaData = [
                {y: "2009", a: 10, b: 20, c: 30},
                {y: "2010", a: 75, b: 65, c: 30},
                {y: "2011", a: 50, b: 40, c: 30},
                {y: "2012", a: 75, b: 65, c: 30},
                {y: "2013", a: 50, b: 40, c: 30},
                {y: "2014", a: 75, b: 65, c: 30},
                {y: "2015", a: 90, b: 60, c: 30}
            ];
            this.createAreaChart("morris-area-example",
                0,
                0,
                $areaData,
                "y",
                ["a", "b", "c"],
                ["Mobiles", "Tablets", "Desktops"],
                ["#4793f5", "#ff3f4e", "#bbbbbb"]);

            //creating bar chart
            const $barData = [
                {y: "2009", a: 100, b: 90, c: 40},
                {y: "2010", a: 75, b: 65, c: 20},
                {y: "2011", a: 50, b: 40, c: 50},
                {y: "2012", a: 75, b: 65, c: 95},
                {y: "2013", a: 50, b: 40, c: 22},
                {y: "2014", a: 75, b: 65, c: 56},
                {y: "2015", a: 100, b: 90, c: 60}
            ];
            this.createBarChart("morris-bar-example",
                $barData,
                "y",
                ["a", "b", "c"],
                ["Series A", "Series B", "Series C"],
                ["#2dc4b9", "#f9c851", "#98a6ad"]);

        },
        //init
        $.Dashboard4 = new Dashboard4, $.Dashboard4.Constructor = Dashboard4;
}(window.jQuery),

//initializing
    function ($) {
        "use strict";
        $.Dashboard4.init();
    }(window.jQuery);