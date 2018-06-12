using AzureDashboard.Services;
using AzureDashboard.Services.Models;
using AzureDashboard.Wpf.Models;
using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AzureDashboard.Wpf.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        ArmClient _client;

        public ObservableCollection<Subscription> Subscriptions { get; set; }
        public Prop<Subscription> SelectedSubscription { get; set; }
        public Prop<bool> SubscriptionsSelectorEnabled { get; set; }

        public ObservableCollection<ResourceType> ResourceTypes { get; set; }
        public Prop<ResourceType> SelectedResourceType { get; set; }
        public Prop<bool> ResourceTypesSelectorEnabled { get; set; }


        public ObservableCollection<Resource> Resources { get; set; }
        public Prop<Resource> SelectedResource { get; set; }
        public Prop<bool> ResourcesSelectorEnabled { get; set; }

        public ObservableCollection<Metric> Metrics { get; set; }
        public Prop<Metric> SelectedMetric { get; set; }
        public Prop<bool> MetricsSelectorEnabled { get; set; }

        public ObservableCollection<string> TimeGrains { get; set; }
        public Prop<string> SelectedTimeGrain { get; set; }
        public Prop<bool> TimeGrainsSelectorEnabled { get; set; }

        public ObservableCollection<Metric.MetricDimension> MetricDimensions { get; set; }
        public Prop<Metric.MetricDimension> SelectedMetricDimension { get; set; }
        public Prop<bool> MetricDimensionsSelectorEnabled { get; set; }


        public ObservableCollection<DimensionValue> DimensionValues { get; set; }
        public Prop<DimensionValue> SelectedDimensionValue { get; set; }
        public Prop<bool> DimensionValuesSelectorEnabled { get; set; }


        public Prop<DateTime> Start { get; set; }
        public Prop<DateTime> End { get; set; }

        public Prop<bool> LoadGraphEnabled { get; set; }
        public SeriesCollection Series { get; set; }
        public ObservableCollection<string> Labels { get; set; }
        public Prop<DateTime> MaxLabel { get; set; }
        public Prop<DateTime> MinLabel { get; set; }

        public ShellViewModel(ArmClient client)
        {
            Subscriptions = new ObservableCollection<Subscription>();
            SelectedSubscription = new Prop<Subscription>();
            SubscriptionsSelectorEnabled = new Prop<bool>();
            ResourceTypes = new ObservableCollection<ResourceType>();
            SelectedResourceType = new Prop<ResourceType>();
            ResourceTypesSelectorEnabled = new Prop<bool>();
            Resources = new ObservableCollection<Resource>();
            SelectedResource = new Prop<Resource>();
            ResourcesSelectorEnabled = new Prop<bool>();
            Metrics = new ObservableCollection<Metric>();
            SelectedMetric = new Prop<Metric>();
            MetricsSelectorEnabled = new Prop<bool>();
            TimeGrains = new ObservableCollection<string>();
            SelectedTimeGrain = new Prop<string>();
            TimeGrainsSelectorEnabled = new Prop<bool>();
            MetricDimensions = new ObservableCollection<Metric.MetricDimension>();
            SelectedMetricDimension = new Prop<Metric.MetricDimension>();
            MetricDimensionsSelectorEnabled = new Prop<bool>();
            DimensionValues = new ObservableCollection<DimensionValue>();
            SelectedDimensionValue = new Prop<DimensionValue>();
            DimensionValuesSelectorEnabled = new Prop<bool>();
            var now = DateTime.Now;
            Start = new Prop<DateTime>(now.AddDays(-10));
            End = new Prop<DateTime>(now);
            LoadGraphEnabled = new Prop<bool>();
            Series = new SeriesCollection();
            Labels = new ObservableCollection<string>();
            MaxLabel = new Prop<DateTime>();
            MinLabel = new Prop<DateTime>();
            _client = client;
        }

        public async void SelectSubscription(SelectionChangedEventArgs args)
        {
            if (args.AddedItems[0] is Subscription sub)
            {
                ResourceTypes.Clear();
                _client.SetSubscription(SelectedSubscription.Value);
                var types = await _client.ResourceTypes();
                foreach (var type in types)
                {
                    ResourceTypes.Add(type);
                }
                SelectedResourceType.Value = types.First();
                ResourceTypesSelectorEnabled.Value = types.Count() > 1;
            }
        }

        public async void SelectResourceType(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 1)
            {
                LoadGraphEnabled.Value = false;
                var resourceType = args.AddedItems[0] as ResourceType;
                Resources.Clear();
                var resources = await _client.Resources(resourceType);
                foreach (var resource in resources)
                {
                    Resources.Add(resource);
                }
                SelectedResource.Value = resources.First();
                ResourcesSelectorEnabled.Value = resources.Count() > 1;
            }
        }

        public async void SelectResource(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 1)
            {
                LoadGraphEnabled.Value = false;
                var resource = args.AddedItems[0] as Resource;
                Metrics.Clear();
                var metrics = await _client.Metrics(resource);
                foreach(var metric in metrics)
                {
                    Metrics.Add(metric);
                }
                if (metrics.Count() > 0)
                {
                    SelectedMetric.Value = metrics.First();
                }
                else
                {
                    var m = new Metric { Name = new Name {  LocalizedValue = "No metrics supported" } };
                    Metrics.Add(m);
                    SelectedMetric.Value = m;
                }
                MetricsSelectorEnabled.Value = metrics.Count() > 1;
            }
        }

        public async void SelectMetric(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 1)
            {
                var metric = args.AddedItems[0] as Metric;
                MetricDimensions.Clear();
                if (metric.Dimensions != null && metric.Dimensions.Count() > 0)
                {
                    LoadGraphEnabled.Value = false;
                    foreach (var d in metric.Dimensions)
                    {
                        MetricDimensions.Add(d);
                    }
                    SelectedMetricDimension.Value = metric.Dimensions.First();
                    MetricDimensionsSelectorEnabled.Value = metric.Dimensions.Count() > 1;
                    var timegrains = new [] { "Automatic" }.Concat(metric.MetricAvailabilities.Select(x => x.TimeGrain).ToList());
                    TimeGrains.Clear();
                    foreach(var tg in timegrains)
                    {
                        TimeGrains.Add(tg);
                    }
                    SelectedTimeGrain.Value = timegrains.First();
                    TimeGrainsSelectorEnabled.Value = timegrains.Count() > 1;
                }
                else
                {
                    LoadGraphEnabled.Value = true;
                    var md = new Metric.MetricDimension { LocalizedValue = "No dimensions supported" };
                    MetricDimensions.Add(md);
                    SelectedMetricDimension.Value = md;
                    MetricDimensionsSelectorEnabled.Value = false;
                }
            }
        }

        public async void SelectMetricDimension(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 1)
            {
                var md = args.AddedItems[0] as Metric.MetricDimension;
                DimensionValues.Clear();
                if (md.Value != null)
                {
                    var res = SelectedResource.Value;
                    var m = SelectedMetric.Value;
                    var dimVals = await _client.DimensionValues(res, m, md.Value, Start.Value, End.Value);
                    
                    if (dimVals.Count() > 0)
                    {
                        foreach (var dv in dimVals)
                        {
                            DimensionValues.Add(dv);
                        }
                        SelectedDimensionValue.Value = dimVals.First();
                        DimensionValuesSelectorEnabled.Value = dimVals.Count() > 1;
                    }
                    else
                    {
                        DimensionValuesSelectorEnabled.Value = false;
                    }
                }
                else
                {
                    DimensionValuesSelectorEnabled.Value = false;
                }
            }
        }

        public async void SelectDimensionValue(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 1)
            {
                LoadGraphEnabled.Value = true;
            }
        }

        public async void LoadGraph()
        {
            var tg = GetTimeGrain();
            var result = await _client.MetricValues(
                SelectedResource.Value,
                SelectedMetric.Value,
                tg,
                Start.Value,
                End.Value,
                SelectedMetricDimension.Value?.Value,
                SelectedDimensionValue.Value?.Value
            );
            var data = result.Single().Data;
            var title = SelectedMetric.Value.Name.LocalizedValue;
            var md = result.Single().MetadataValues.FirstOrDefault();
            if (md != null)
            {
                title += $" ({md.Name.LocalizedValue} = {md.Value})";
            }
            var labels = data.Select(x => x.TimeStamp).ToList();
            Labels.Clear();
            foreach(var l in labels)
            {
                Labels.Add(l.ToString());
            }
            MinLabel.Value = labels.First();
            MaxLabel.Value = labels.Last();
            Series.Clear();
            Series.Add(new LineSeries
            {
                Title = title,
                Values = new ChartValues<float>(data.Select(x => x.Average))
            });
        }
        
        string GetTimeGrain()
        {
            var timegrain = SelectedTimeGrain.Value;
            
            if (timegrain == "Automatic")
            {
                var vals = TimeGrains.Where(x => x != timegrain).Select(x =>
                {
                    x = x.Substring(1 + (x[1] == 'T' ? 1 : 0));
                    return x == "1M"  ? new TimeSpan(0, 1, 0) :
                           x == "5M"  ? new TimeSpan(0, 5, 0) : 
                           x == "15M" ? new TimeSpan(0, 15, 0) :
                           x == "30M" ? new TimeSpan(0, 30, 0) :
                           x == "1H" ? new TimeSpan(1, 0, 0) :
                           x == "6H" ? new TimeSpan(6, 0, 0) :
                           x == "12H" ? new TimeSpan(12, 0, 0) : 
                           x == "1D" ? new TimeSpan(24, 0, 0) :
                           default(TimeSpan);
                });
                var durationSecs = (End.Value - Start.Value).Ticks;
                var points = vals.Select(x => durationSecs / x.Ticks).ToList();
                var maxpoints = 500;
                
                for(var i = 0; i < points.Count; i++)
                {
                    if (points[i] < maxpoints)
                    {
                        timegrain = TimeGrains.ElementAt(i + 1);
                        break;
                    }
                }
            }
            return timegrain;
        }
        
        protected override async void OnInitialize()
        {
            await _client.Initialize();
            var subs = await _client.Subscriptions();
            foreach (var sub in subs)
            {
                Subscriptions.Add(sub);
            }
            SelectedSubscription.Value = subs.First();
            SubscriptionsSelectorEnabled.Value = subs.Count() > 1;
            base.OnInitialize();
        }
    }
}
