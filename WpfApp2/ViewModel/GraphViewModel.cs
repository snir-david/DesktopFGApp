﻿using AdvancedCoding2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Threading;
using DesktopFGApp.Model;

namespace DesktopFGApp.ViewModel
{
    class GraphViewModel : INotifyPropertyChanged
    {
        private IClientModel clientModel;
        private ViewModelController viewModelController;
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> attList, corrList;
        private PlotModel plotModel1, plotModel2, plotModel3;
        private OxyPlot.Wpf.PlotView VM_pvAtt, VM_pvCorr, VM_pvLR;

        // property for the chart of the chosen.
        public PlotModel VM_PlotModel1
        {
            get
            {
                return plotModel1;
            }
            set
            {
                if (VM_PlotModel1 != value)
                {
                    plotModel1 = value;
                    onPropertyChanged("VM_PlotModel1");

                }
            }
        }

        // property for the chart of the corr.
        public PlotModel VM_PlotModel2
        {
            get
            {
                return plotModel2;
            }
            set
            {
                if (VM_PlotModel2 != value)
                {
                    plotModel2 = value;
                    onPropertyChanged("VM_PlotModel2");

                }
            }
        }

        // property for the Linear Reg.
        public PlotModel VM_PlotModel3
        {
            get
            {
                return plotModel3;
            }
            set
            {
                if (VM_PlotModel3 != value)
                {
                    plotModel3 = value;
                    onPropertyChanged("VM_PlotModel3");

                }
            }
        }
        // property of the list of att.
        public List<String> nameList
        {
            get
            {
                return clientModel.HeaderNames;
            }
        }
        // property of the chosen att.
        public string VM_chosen
        {
            get
            {
                return clientModel.Chosen;
            }
            set
            {
                if (VM_chosen != value)
                {
                    clientModel.Chosen = value;
                    onPropertyChanged("VM_chosen");
                }
            }
        }
        // property for the corralative feature.
        public string VM_corralative
        {
            get
            {
                return clientModel.Corralative;
            }
            set
            {
                if (VM_corralative != value)
                {
                    clientModel.Corralative = value;
                    onPropertyChanged("VM_corralative");
                }
            }
        }
        // property of the current line we are in.
        public int VM_currLine
        {
            get
            {
                return clientModel.lineNumber;
            }
        }
        // property of the list of all the values of the atts.
        public List<List<string>> VM_attsList
        {
            get
            {
                return clientModel.CurrentAtt;
            }
        }
        // property of the list of the att's names.
        public List<string> VM_attsName
        {
            get
            {
                return clientModel.HeaderNames;
            }
        }

        // the constructor of the Graph View Model.
        public GraphViewModel(IClientModel c, OxyPlot.Wpf.PlotView pv1, OxyPlot.Wpf.PlotView pv2, OxyPlot.Wpf.PlotView pv3)
        {
            this.clientModel = c;
            this.VM_pvAtt = pv1;
            this.VM_pvCorr = pv2;
            this.VM_pvLR = pv3;

            clientModel.xmlParser();
            this.viewModelController = new ViewModelController(this.clientModel);
            VM_PlotModel1 = new PlotModel();
            VM_PlotModel2 = new PlotModel();
            VM_PlotModel3 = new PlotModel();

            clientModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                onPropertyChanged("VM_" + e.PropertyName);
            };
            
            // responsibles of updating the graph.
            clientModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "lineNumber" && VM_chosen != null && VM_corralative != null && VM_pvLR != null)
                {
                    double playSpeed = (10 * (2.00 - viewModelController.VM_TransSpeed / 100));
                    if ((VM_currLine  % playSpeed == 0.0 && playSpeed < 1.9) || (VM_currLine % 20 == 0.0 && playSpeed >= 1.9))
                    {
                        VM_PlotModel1.Series.Clear();
                        LoadAttData(VM_currLine, VM_pvAtt);
                        VM_pvAtt.InvalidatePlot(true);

                        VM_PlotModel2.Series.Clear();
                        LoadCorrData(VM_currLine, VM_pvCorr);
                        VM_pvCorr.InvalidatePlot(true);

                        VM_PlotModel3.Series.Clear();
                        LoadLRData(VM_currLine, VM_pvLR);
                        VM_pvLR.InvalidatePlot(true);
                    }
                }
            };
               
        }

        public void onPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        // gets a list of strings and returns a list of floats.
        /************* only this class uses this function. *************/
        private List<float> stringToFloat(List<string> list)
        {
            List<float> result = new List<float>();
            foreach (string s in list)
            {
                result.Add(float.Parse(s));
            }
            return result;
        }
        
        // finds the corralated feature.
        /************ the graph view uses it ***********/
        public string FindCorralativeFeature(string s)
        {
            string corrFeature;
            List<string> valuesOfChosen = new List<string>();
            List<string> valuesOfCorr = new List<string>();

            int index = viewModelController.VM_headerNames.FindIndex(a => a.Contains(s));
            // the values of the chosen feature.
            valuesOfChosen = viewModelController.VM_currentAtt[index];
            List<float> valuesOfChosenInFloat = new List<float>();
            valuesOfChosenInFloat = stringToFloat(valuesOfChosen);

            int indexOfCorr = 0;
            int counter = 0;
            double max = 0;
            double result = 0;

            foreach (List<string> list in viewModelController.VM_currentAtt)
            {
               
                List<float> valuesInFloat = new List<float>();
                if (counter == index)
                {
                    counter++;
                    
                }
                else
                {
                    valuesInFloat = stringToFloat(list);
                    result = Math.Abs(clientModel.pearson(valuesOfChosenInFloat, valuesInFloat,valuesInFloat.Count));
                    if (result > max)
                    {
                        max = result;
                        indexOfCorr = counter;
                    }
                    counter++;
                }
            }
            // takes care of the atts with vector 0.
            if(indexOfCorr == 0 && s != viewModelController.VM_headerNames[0])
            {
                corrFeature = s;
                VM_corralative = corrFeature;
                return corrFeature;
            }

            corrFeature = viewModelController.VM_headerNames[indexOfCorr];
            VM_corralative = corrFeature;
            return corrFeature;
        }

        // creates the chosen graph.
        public void LoadAttData(int lineNumber, OxyPlot.Wpf.PlotView pv)
        {
            int idx = VM_attsName.FindIndex(a => a.Contains(VM_chosen));
            attList = VM_attsList[idx];

            var lineSerie = new LineSeries()
            {
                StrokeThickness = 2,
                Color = OxyColors.Black,
                
            };


            for (int i = 0; i < lineNumber; i++)
            {
                lineSerie.Points.Add(new DataPoint(i, Double.Parse(attList[i])));
            }

            VM_PlotModel1.Series.Add(lineSerie);
        }
        
        // creates the corralative graph.
        // ********** maybe we can put it in the view graph ***********/
        public void LoadCorrData(int lineNumber, OxyPlot.Wpf.PlotView pv)
        {
            int idx = VM_attsName.FindIndex(a => a.Contains(VM_corralative));
            corrList = VM_attsList[idx];

            var lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                Color = OxyColors.Black,
            };

            for (int i = 0; i < lineNumber; i++)
            {
                lineSerie.Points.Add(new DataPoint(i, Double.Parse(corrList[i])));
            }

            VM_PlotModel2.Series.Add(lineSerie);
        }

        public void LoadLRData(int lineNumber, OxyPlot.Wpf.PlotView pv)
        {
            int idx = VM_attsName.FindIndex(a => a.Contains(VM_chosen));
            attList = VM_attsList[idx];

            idx = VM_attsName.FindIndex(a => a.Contains(VM_corralative));
            corrList = VM_attsList[idx];

            //LineSeries for reg line
            var lineSeries = new LineSeries()
            {
                Color = OxyColors.Red,
                StrokeThickness = 2
            };
            //converting attList and corrList to floats list and than making a point list for linear_reg function
            List<float> valuesOfChosenInFloat = new List<float>();
            List<float> valuesOfChosen2InFloat = new List<float>();
            List<Point> pointList = new List<Point>();
            valuesOfChosenInFloat = stringToFloat(attList);
            valuesOfChosen2InFloat = stringToFloat(corrList);
            for(int i = 0; i < attList.Count; i++)
            {
                pointList.Add(new Point(valuesOfChosenInFloat[i], valuesOfChosen2InFloat[i]));
            }
            //finding reg Line
            Line regLine = clientModel.linear_reg(pointList, pointList.Count);
            //finding max and min values of attList;
            float max = valuesOfChosenInFloat.Max();
            float min = valuesOfChosenInFloat.Min();
            //drawing line between to extrem points
            lineSeries.Points.Add(new DataPoint(max, regLine.f(max)));
            lineSeries.Points.Add(new DataPoint(min, regLine.f(min)));

            var scatterPoint = new ScatterSeries
            {
                MarkerType = MarkerType.Circle

            };

            //TODO - Make 300 last points in red
            for (int i = 0; i < lineNumber; i++)
            {
                scatterPoint.Points.Add(new ScatterPoint( Double.Parse(attList[i]), Double.Parse(corrList[i]), 2, 100));
            }

            VM_PlotModel3.Series.Add(scatterPoint);
            VM_PlotModel3.Series.Add(lineSeries);
        }

        // sets up a given "graph".
        public void SetUpModel(PlotModel pm)
        {
            pm = new PlotModel();
            pm.LegendOrientation = LegendOrientation.Horizontal;
            pm.LegendPlacement = LegendPlacement.Outside;
            pm.LegendPosition = LegendPosition.TopRight;
            pm.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            pm.LegendBorder = OxyColors.Black;

            //Creating Axis
            var timeAxis = new LinearAxis() { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80, Title = "Time" };
            pm.Axes.Add(timeAxis);
            var valueAxis = new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Value" };
            pm.Axes.Add(valueAxis);
            
        }
    }

}

