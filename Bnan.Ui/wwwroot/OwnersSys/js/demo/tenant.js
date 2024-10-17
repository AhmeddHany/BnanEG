//First_City_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 35.5; // Define the percentage value

var data = anychart.data.set([
  ['مكة', percentage],
  [' اخري', 100 - percentage]
]);


// set the colors according to the brands
palette.items([
  { color: '#9747FF' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('First_City_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();
//Second_City_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 20.5; // Define the percentage value

  

var data = anychart.data.set([
  ['المدينة ', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#FF9F40' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Second_City_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();

//Third_City_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 15.5; // Define the percentage value
var data = anychart.data.set([
  ['الرياض', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#4BC0C0' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Third_City_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();



//First_rate_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 35.5; // Define the percentage value

var data = anychart.data.set([
  ['صبياء', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#9747FF' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('First_rate_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();
//Second_rate_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 20.5; // Define the percentage value
var data = anychart.data.set([
  ['الرياض', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#FF9F40' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Second_rate_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();

//Third_rate_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 15.5; // Define the percentage value
var data = anychart.data.set([
  ['الخبر', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#4BC0C0' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Third_rate_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();

//First_membership_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 35.5; // Define the percentage value
var data = anychart.data.set([
  ['ماسية', percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#9747FF' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('First_membership_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();
//Second_membership_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 20.5; // Define the percentage value

var data = anychart.data.set([
  ["فضية", percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#FF9F40' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Second_membership_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();
//Third_membership_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 15.5; // Define the percentage value

var data = anychart.data.set([
  ["ذهبية", percentage],
  [' اخري', 100 - percentage]
]);

// set the colors according to the brands
palette.items([
  { color: '#4BC0C0' },
  { color: '#CCC' },
]);

// apply the donut chart color palette
// create a pie chart with the data
var chart = anychart.pie(data)
// set the chart radius making a donut chart
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Third_membership_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.draw();