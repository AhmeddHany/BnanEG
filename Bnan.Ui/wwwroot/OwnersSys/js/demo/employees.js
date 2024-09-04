// // Set new default font family and font color to mimic Bootstrap's default styling
// Chart.defaults.global.defaultFontFamily = 'Cairo', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
// Chart.defaults.global.defaultFontColor = '#858796';

// Pie Chart contracts
const pieChartEl = document.getElementById('EmployeePieChart');
const pieChartData = {
  labels: [' تنتهي اليوم', 'منتهيه', ' تنتهي غدا', 'تنتهي لاحقا'],
  datasets: [{
    data: [342, 313, 245, 210],
    backgroundColor: ['rgba(255, 159, 64, .9)', 'rgba(255, 64, 105, .9)', 'rgba(153, 102, 255,.9 )','rgba(54, 162, 235, .9)'],
      hoverBackgroundColor: ['rgba(255, 159, 64, 1)', 'rgba(255, 64, 105, 1)', 'rgba(153, 102, 255,1 )','rgba(54, 162, 235, 1)'],  }]
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
        display: false
      }
    }
  }
});
// Bar Chart 
var barChart = document.getElementById("EmployeebarChart").getContext("2d");
var myChart = new Chart(barChart, {
  type: "bar",
  data: {
    labels: ['نقدًا', 'مدى', 'فيزا', 'اكسبرس', ' ماستر ', 'تحويل'],
    datasets: [
      {
        data: [100, 20, 50, 45, 40, 10],
        backgroundColor: [
          "rgba(255, 99, 132, 1)",
          "rgba(54, 162, 235, 1)",
          "rgba(255, 206, 86, 1)",
          "rgba(75, 192, 192, 1)",
          "rgba(153, 102, 255, 1)",
          "rgba(255, 159, 64, 1)"
        ],
       
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

    },
    responsive: true,
    maintainAspectRatio: false,
    barPercentage: 0.6, 
    categoryPercentage: 0.8 
  }
});