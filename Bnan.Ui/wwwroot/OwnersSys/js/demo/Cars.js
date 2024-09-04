//First_Brand_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 42; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['هيونداي', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#9747FF' },
  { color: '#CCC' },
]);


var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('First_Brand_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();
//Second_Brand_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 35.58; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['فورد', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#FF9F40' },
  { color: '#CCC' },
]);

var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Second_Brand_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();

//Third_Brand_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 25; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['مرسيدس', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#4BC0C0' },
  { color: '#CCC' },
]);

var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Third_Brand_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();


//First_year_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 42; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['2020', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#9747FF' },
  { color: '#CCC' },
]);


var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('First_year_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();
//Second_year_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 35.58; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['2021', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#FF9F40' },
  { color: '#CCC' },
]);

var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Second_year_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();

//Third_year_Charts ///////////////////////////////////////////////////////////////////////
var palette = anychart.palettes.distinctColors();
var percentage = 25; 
var label = anychart.standalones.label();
label
  .useHtml(true)
  .text(
    '<span style = "color: #313136; font-weight:600">' + percentage + '%</span>'
  )
  .position('center')
  .anchor('center')
  .hAlign('center')
  .vAlign('middle');
var data = anychart.data.set([
  ['2018', percentage],
  [' اخري', 100 - percentage]
]);

palette.items([
  { color: '#4BC0C0' },
  { color: '#CCC' },
]);

var chart = anychart.pie(data)
chart.palette(palette);
chart.innerRadius('80%');
chart.container('Third_year_Charts');
chart.legend(false);
chart.labels(false)
chart.tooltip(false)
chart.center().content(label);
chart.draw();

