// //////////////////////////////////////////////////  الرئيسية /////////////////////////////////////////////////////////////////////////
// Pie Chart contracts
const pieChartEl = document.getElementById('myPieChart');
const pieChartData = {
  labels: ['منتهيه',' تنتهي اليوم',  ' تنتهي غدا', 'تنتهي لاحقا'],
  datasets: [{
    data: [342, 313, 245, 210],
    backgroundColor: [ 'rgba(255, 64, 105, .9)','rgba(255, 159, 64, .9)', 'rgba(153, 102, 255,.9 )','rgba(54, 162, 235, .9)'],
    borderColor: ['rgba(255, 64, 105, .9)','rgba(255, 159, 64, .9)',  'rgba(153, 102, 255,.9 )','rgba(54, 162, 235, .9)'],

  }]
};

const pieChart = new Chart(pieChartEl, {
  type: 'doughnut',
  data: pieChartData,
  options: {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      tooltip: {
        backgroundColor: "#7A7A7A",
        bodyFontColor: "#060A10",
        borderColor: '#ffffff',
        borderWidth: 1,
        xPadding: 15,
        yPadding: 15,
        displayColors: false,
        caretPadding: 10,
        
      },
      legend: {
        position: 'right',
        rtl: true,
        labels: {
            font: {
                family: "'Cairo', sans-serif",
                size: 11
            }
        }
    }
    }
  }
});

//// دائنون
//var palette = anychart.palettes.distinctColors();
//var percentage = 35.58; 
//var label = anychart.standalones.label();
//label
//  .useHtml(true)
//  .text(
//    '<span style = "color: #313136; font-weight:600 ; font-family: "Cairo", sans-serif;">' + percentage + '%</span>'
//  )
//  .position('center')
//  .anchor('center')
//  .hAlign('center')
//  .vAlign('middle');
//var data = anychart.data.set([
//  ['فورد', percentage],
//  [' اخري', 100 - percentage]
//]);

//palette.items([
//  { color: '#FF9FB3' },
//  { color: '#E6E6E6' },
//]);

//var chart = anychart.pie(data)
//chart.palette(palette);
//chart.innerRadius('70%');
//chart.container('chart-creditor');
//chart.legend(false);
//chart.labels(false)
//chart.tooltip(false)
//chart.center().content(label);
//chart.draw();

//// مدينون

//var palette = anychart.palettes.distinctColors();
//var percentage = 64.48; 
//var label = anychart.standalones.label();
//label
//  .useHtml(true)
//  .text(
//    '<span style = "color: #313136; font-weight:600 ; font-family: "Cairo", sans-serif;">' + percentage + '%</span>'
//  )
//  .position('center')
//  .anchor('center')
//  .hAlign('center')
//  .vAlign('middle');
//var data = anychart.data.set([
//  ['فورد', percentage],
//  [' اخري', 100 - percentage]
//]);

//palette.items([
//  { color: '#9AD0F4' },
//  { color: '#E6E6E6' },
//]);

//var chart = anychart.pie(data)
//chart.palette(palette);
//chart.innerRadius('70%');
//chart.container('chart-debtors');
//chart.legend(false);
//chart.labels(false)
//chart.tooltip(false)
//chart.center().content(label);
//chart.draw();

//// 

// Bar Chart 
var barChart = document.getElementById("barChart").getContext("2d");
var myChart = new Chart(barChart, {
  type: "bar",
  data: {
    labels: ['نقدًا', 'مدى', 'فيزا', 'امريكن اكسبريس', ' ماستر كارد'],
    datasets: [
      {
        data: [100, 20, 50, 45, 40, 10],
        backgroundColor: [
          "rgba(255, 99, 132, 1)",
          "rgba(54, 162, 235, 1)",
          "rgba(255, 206, 86, 1)",
          "rgba(75, 192, 192, 1)",
          "rgba(153, 102, 255, 1)",
        ],
        barThickness: 35
      }
    ]
  },
  options: {
    responsive: true,
    plugins: {
      
      legend: {
        display: false
      }

    },
    scales: {
      x: {
        grid: {
          display: true 
        },
        ticks: {
          display: true,
          font: {
              family: "'Cairo', sans-serif",
              size: 11
          }
      }
      },
      y: {
        grid: {
          display: true
        },
        beginAtZero: true,
        ticks: {
          font: {
            size: 11 
          }
        }
      }

    }
  }
});

