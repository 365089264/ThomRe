using System.Drawing;
using System.Linq;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using VAV.Model.Data;

namespace VAV.DAL.Services
{
    public class ChartService
    {
        public Highcharts GetChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;

            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName, chartModel.IsResizeable, chartModel.ReportID)
                    .InitChart(new Chart
                                   {
                                       DefaultSeriesType = chartModel.ChartType,
                                       ClassName = "chart" + chartModel.ChartName,
                                       Height = 280,
                                       Width = 430,
                                       Reflow = true,
                                   });
            }
            else
            {
                chart = original;
            }
            chart.SetTitle(new Title
                               {
                                   Text = chartModel.Title,
                                   X = -20
                               })
                .SetCredits(new Credits
                                {
                                    Text = chartModel.Source,
                                    Href = "http://thomsonreuters.com/"
                                })
                .SetXAxis(new XAxis
                              {
                                  Categories =
                                      chartModel.XAxisCategory != null ? chartModel.XAxisCategory.ToArray() : null,
                                  Labels = new XAxisLabels { Enabled = false },
                                  Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,

                              })
                .SetYAxis(new YAxis
                              {
                                  Title = new XAxisTitle { Text = chartModel.YAxisText },
                                  MinRange = 5,
                                  Labels = new XAxisLabels { Style = "color: '#999', fontWeight: 'bold'" },
                              });

            if (chartModel.NoUnit)
            {
                chart.SetTooltip(new Tooltip
                                    {
                                        Formatter =
                                        @"function() {
                                                            return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.y,0);
                                        }",
                                    });
            }
            else
            {
                chart.SetTooltip(new Tooltip
                                    {
                                        Formatter =
                                        @"function() {
                                                            return  '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.y,2);
                                         }",
                                    });
            }

            chart.SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        PointPadding = 0.2,
                        BorderWidth = 0,
                    },
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsPieDataLabels
                        {
                            Enabled = true,
                            Style = "color: '#999', fontSize: '8px', fontWeight:'lighter'",
                            Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                        }
                    },
                })
                .SetLegend(new Legend
                               {
                                   Layout = Layouts.Horizontal,
                                   Align = HorizontalAligns.Center,
                                   VerticalAlign = VerticalAligns.Bottom,
                                   BorderWidth = 0,
                               })
                .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        public Highcharts GetLargeChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;
            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName + "Large")
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = chartModel.ChartType,
                        ClassName = "chart" + chartModel.ChartName,
                        MarginRight = 50,
                        MarginBottom = 120,
                        MarginLeft = 120,
                        MarginTop = 50,
                        Width = chartModel.Width ?? 630,
                        Height = chartModel.Height ?? 500,
                        BackgroundColor = chartModel.Theme == "Pearl" ? ColorTranslator.FromHtml("#ffffff") : ColorTranslator.FromHtml("#424242"),
                        Reflow = true,
                    });
            }
            else
            {
                chart = original;
            }

            chart.SetTitle(new Title
                               {
                                   Text = chartModel.Title,
                                   X = -20
                               })
                .SetCredits(new Credits
                                {
                                    Text = chartModel.Source,
                                    Href = "http://thomsonreuters.com/",
                                    Style = SetStyle(chartModel.Theme),
                                })

                .SetXAxis(new XAxis
                              {
                                  Categories =
                                      chartModel.XAxisCategory != null ? chartModel.XAxisCategory.ToArray() : null,
                                  Labels = chartModel.ChartType != ChartTypes.Bar
                                               ? new XAxisLabels
                                                     {
                                                         Style = SetStyle(chartModel.Theme),
                                                         Rotation = 90,
                                                         Y = 50
                                                     }
                                               : new XAxisLabels { Enabled = false },
                                  Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,

                              })
                .SetYAxis(new YAxis
                              {
                                  Title = new XAxisTitle { Text = chartModel.YAxisText, Style = SetStyle(chartModel.Theme), },
                                  MinRange = 5,
                                  Labels = new XAxisLabels { Style = SetStyle(chartModel.Theme) },
                              });

            if (chartModel.NoUnit)
            {
                chart.SetTooltip(new Tooltip
                                    {
                                        Formatter =
                                        @"function() {
                                                            return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.y,0);
                                        }",
                                    });
            }
            else
            {
                chart.SetTooltip(new Tooltip
                                    {
                                        Formatter =
                                        @"function() {
                                                            return  '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.y,2);
                                         }",
                                    });
            }
            chart.SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        PointPadding = 0.2,
                        BorderWidth = 0,
                    },
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsPieDataLabels
                        {
                            Enabled = true,
                            Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                            Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                        }
                    },
                    Bar = new PlotOptionsBar
                    {
                        DataLabels = new PlotOptionsBarDataLabels
                        {
                            Enabled = !chartModel.IsXAxisDate,
                            Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                            Formatter = "function() { return this.point.name; }",
                            Y = -5
                        }
                    }
                })
                .SetLegend(new Legend
                {
                    Layout = Layouts.Horizontal,
                    Align = HorizontalAligns.Center,
                    VerticalAlign = VerticalAligns.Bottom,
                    BorderWidth = 0,
                    Style = SetStyle(chartModel.Theme),
                })
                .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        public Highcharts GetBondChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;

            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName, chartModel.IsResizeable, chartModel.ReportID)
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = chartModel.ChartType,
                        ClassName = "chart" + chartModel.ChartName,
                        MarginRight = 30,
                        Reflow = true,
                    });
            }
            else
            {
                chart = original;
            }

            chart.SetTitle(new Title
            {
                Text = chartModel.Title,
                X = -20
            })
            .SetCredits(new Credits
            {
                Text = chartModel.Source,
                Href = "http://thomsonreuters.com/"
            })
            .SetXAxis(new XAxis
            {
                Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,
                DateTimeLabelFormats = new DateTimeLabel { Day = "%b.%e", Week = "%b.%e", Month = "%Y.%b", Year = "%Y" }
            })
            .SetYAxis(new YAxis
            {
                Title = new XAxisTitle { Text = chartModel.YAxisText },
                MinRange = 5,
                Labels = new XAxisLabels { Style = "color: '#999', fontWeight: 'bold'" },
            })
            .SetLegend(new Legend
            {
                Layout = Layouts.Horizontal,
                Align = HorizontalAligns.Center,
                VerticalAlign = VerticalAligns.Bottom,
                BorderWidth = 0
            })
            .SetPlotOptions(new PlotOptions
            {
                Column = new PlotOptionsColumn
                {
                    PointPadding = 0.2,
                    BorderWidth = 0,
                },
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Enabled = true,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                    }
                },
                Bar = new PlotOptionsBar
                {
                    DataLabels = new PlotOptionsBarDataLabels
                    {
                        Enabled = !chartModel.IsXAxisDate,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return this.point.name; }",
                        Y = -5
                    }
                }
            })
            .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        public Highcharts GetLargeBondChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;

            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName + "_large")
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = chartModel.ChartType,
                        ClassName = "chart" + chartModel.ChartName,
                        MarginRight = 50,
                        MarginBottom = 80,
                        MarginLeft = 50,
                        MarginTop = 50,
                        Width = chartModel.Width ?? 630,
                        Height = chartModel.Height ?? 500,
                        BackgroundColor = chartModel.Theme == "Pearl" ? ColorTranslator.FromHtml("#ffffff") : ColorTranslator.FromHtml("#424242")
                    });
            }
            else
            {
                chart = original;
            }

            chart.SetTitle(new Title
            {
                Text = chartModel.Title,
                X = -20
            })
            .SetCredits(new Credits
            {
                Text = chartModel.Source,
                Href = "http://thomsonreuters.com/",
                Style = SetStyle(chartModel.Theme),
            })
            .SetXAxis(new XAxis
            {
                Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,
                Labels = new XAxisLabels { Style = SetStyle(chartModel.Theme) },
                DateTimeLabelFormats = new DateTimeLabel { Day = "%b.%e", Week = "%b.%e", Month = "%Y.%b", Year = "%Y" }
            })
            .SetYAxis(new YAxis
            {
                Title = new XAxisTitle { Text = chartModel.YAxisText, Style = SetStyle(chartModel.Theme) },
                MinRange = 5,
                Labels = new XAxisLabels { Style = SetStyle(chartModel.Theme) },
            })
            .SetLegend(new Legend
            {
                Layout = Layouts.Horizontal,
                Align = HorizontalAligns.Center,
                VerticalAlign = VerticalAligns.Bottom,
                BorderWidth = 0,
                Style = SetStyle(chartModel.Theme),
            })
            .SetPlotOptions(new PlotOptions
            {
                Column = new PlotOptionsColumn
                {
                    PointPadding = 0.2,
                    BorderWidth = 0,
                },
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Enabled = true,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                    }
                },
                Bar = new PlotOptionsBar
                {
                    DataLabels = new PlotOptionsBarDataLabels
                    {
                        Enabled = !chartModel.IsXAxisDate,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return this.point.name; }",
                        Y = -5
                    }
                },
                Series = new PlotOptionsSeries
                {
                    Point = new PlotOptionsSeriesPoint
                    {
                        Events = new PlotOptionsSeriesPointEvents
                        {
                            Click = "function() { GetGridDataByChartSeriesName('" + chartModel.ChartName + "', this.series.name); }"
                        }
                    }
                }
            })
            .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        public Highcharts GetOpenChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;

            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName, chartModel.IsResizeable, chartModel.ReportID)
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = chartModel.ChartType,
                        ClassName = "chart" + chartModel.ChartName,
                        Height = 280,
                        Width = 430,
                    });
            }
            else
            {
                chart = original;
            }

            chart.SetTitle(new Title
            {
                Text = chartModel.Title,
                X = -20
            })
            .SetCredits(new Credits
            {
                Text = chartModel.Source,
                Href = "http://thomsonreuters.com/"
            })
            .SetXAxis(new XAxis
            {
                Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,
                DateTimeLabelFormats = new DateTimeLabel { Day = "%b.%e", Week = "%b.%e", Month = "%Y.%b", Year = "%Y" }
            })
            .SetYAxis(new YAxis
            {
                Title = new XAxisTitle { Text = chartModel.YAxisText },
                MinRange = 5,
                Labels = new XAxisLabels { Style = "color: '#ccc', fontWeight: 'bold'", Align =  HorizontalAligns.Center}
            })
            .SetLegend(new Legend
            {
                Layout = Layouts.Horizontal,
                Align = HorizontalAligns.Center,
                VerticalAlign = VerticalAligns.Bottom,
                BorderWidth = 0
            })
            .SetPlotOptions(new PlotOptions
            {
                Column = new PlotOptionsColumn
                {
                    PointPadding = 0.2,
                    BorderWidth = 0,
                },
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Enabled = true,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                    }
                },
                Bar = new PlotOptionsBar
                {
                    DataLabels = new PlotOptionsBarDataLabels
                    {
                        Enabled = !chartModel.IsXAxisDate,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return this.point.name; }",
                        Y = -5
                    }
                },
                Series = new PlotOptionsSeries
                {
                    Point = new PlotOptionsSeriesPoint
                    {
                        Events = new PlotOptionsSeriesPointEvents
                        {
                            Click = "function() { GetGridDataByChartSeriesName('" + chartModel.ChartName + "', this.series.name); }"
                        }
                    }
                }
            })
            .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        public Highcharts GetLargeOpenChart(Highcharts original, ChartModel chartModel)
        {
            Highcharts chart;

            if (original == null)
            {
                chart = new Highcharts(chartModel.ChartName + "open_large")
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = chartModel.ChartType,
                        ClassName = "chart" + chartModel.ChartName,
                        Height = 310,
                        Width = 1000,
                    });
            }
            else
            {
                chart = original;
            }

            chart.SetTitle(new Title
            {
                Text = chartModel.Title,
                X = -20
            })
            .SetCredits(new Credits
            {
                Text = chartModel.Source,
                Href = "http://thomsonreuters.com/"
            })
            .SetXAxis(new XAxis
            {
                Type = chartModel.IsXAxisDate ? AxisTypes.Datetime : AxisTypes.Linear,
                DateTimeLabelFormats = new DateTimeLabel { Day = "%b.%e", Week = "%b.%e", Month = "%Y.%b", Year = "%Y" }
            })
            .SetYAxis(new YAxis
            {
                Title = new XAxisTitle { Text = chartModel.YAxisText },
                MinRange = 5,
                Labels = new XAxisLabels { Style = "color: '#999', fontWeight: 'bold'" },
            })
            .SetLegend(new Legend
            {
                Layout = Layouts.Horizontal,
                Align = HorizontalAligns.Center,
                VerticalAlign = VerticalAligns.Bottom,
                BorderWidth = 0
            })
            .SetPlotOptions(new PlotOptions
            {
                Column = new PlotOptionsColumn
                {
                    PointPadding = 0.2,
                    BorderWidth = 0,
                },
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Enabled = true,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return '<b>'+ this.point.name +'</b>'; }"
                    }
                },
                Bar = new PlotOptionsBar
                {
                    DataLabels = new PlotOptionsBarDataLabels
                    {
                        Enabled = !chartModel.IsXAxisDate,
                        Style = "color: '#999', fontSize: '10px', fontWeight:'lighter'",
                        Formatter = "function() { return this.point.name; }",
                        Y = -5
                    }
                },
                Series = new PlotOptionsSeries
                {
                    Point = new PlotOptionsSeriesPoint
                    {
                        Events = new PlotOptionsSeriesPointEvents
                        {
                            Click = "function() { GetGridDataByChartSeriesName('" + chartModel.ChartName + "', this.series.name); }"
                        }
                    }
                }
            })
            .SetSeries(chartModel.Series.ToArray());

            return chart;
        }

        private static string SetStyle(string theme)
        {
            return theme == "Pearl" ? "color: '#000000'" : "color: '#ffffff'";
        }


    }
}
